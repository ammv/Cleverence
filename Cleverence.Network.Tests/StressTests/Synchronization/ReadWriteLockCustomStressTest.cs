using Cleverence.Network.Synchronization;
using System.Collections.Concurrent;

namespace Cleverence.Network.Tests;

[TestFixture, Category("Stress")]
public class ReadWriteLockCustomStressTest
{
    [Test]
    public void ReadersAndWriters_InterleaveCorrectly()
    {
        const int iterations = 10_000;
        var value = 0;
        var lockInstance = new ReadWriteLockCustom();

        var writer = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                using (lockInstance.WriteLock())
                {
                    value = i;
                }
            }
        });

        var readers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                using (lockInstance.ReadLock())
                {
                    var v = value;
                    Assert.That(v >= 0 && v < iterations, Is.True);
                }
            }
        })).ToArray();

        Task.WaitAll(new[] { writer }.Concat(readers).ToArray());
    }

    [Test]
    public void ReadersAndWriters_NoDeadlocksOrCorruption()
    {
        const int writerCount = 4;
        const int readerCount = 8;
        const int operationsPerThread = 20_000;

        var lockInstance = new ReadWriteLockCustom();
        var sharedCounter = 0;

        var exceptions = new ConcurrentQueue<Exception>();

        var completedThreads = new CountdownEvent(writerCount + readerCount);

        void WriterAction()
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    using (lockInstance.WriteLock())
                    {
                        sharedCounter++;
                    }

                    if (i % 100 == 0)
                        Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
            finally
            {
                completedThreads.Signal();
            }
        }

        void ReaderAction()
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    using (lockInstance.ReadLock())
                    {
                        var value = sharedCounter;

                        if (value < 0 || value > writerCount * operationsPerThread)
                        {
                            exceptions.Enqueue(
                                new InvalidOperationException($"Incorrect counter value: {value}"));
                            break;
                        }
                    }

                    if (i % 100 == 0)
                        Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
            finally
            {
                completedThreads.Signal();
            }
        }

        var tasks = new Task[writerCount + readerCount];

        for (int i = 0; i < writerCount; i++)
            tasks[i] = Task.Run(WriterAction);

        for (int i = 0; i < readerCount; i++)
            tasks[writerCount + i] = Task.Run(ReaderAction);

        bool allCompleted = completedThreads.Wait(TimeSpan.FromSeconds(30));

        Assert.That(allCompleted, Is.True, "Test didn't finish in 30 seconds. Deadlock is possible.");

        if (!exceptions.IsEmpty)
        {
            var errorList = string.Join("\n", exceptions);
            Assert.Fail($"Detected exceptions in background threads:\n{errorList}");
        }

        int finalValue;

        using (lockInstance.ReadLock())
        {
            finalValue = sharedCounter;
        }

        int expected = writerCount * operationsPerThread;

        Assert.That(finalValue, Is.EqualTo(expected),
            $"Final counter value incorrect. Expected: {expected}, but was: {finalValue}");
    }
}
