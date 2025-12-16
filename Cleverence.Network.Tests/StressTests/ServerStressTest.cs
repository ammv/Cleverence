using NUnit.Framework;
using System.Collections.Concurrent;
using System.Reflection;

namespace Cleverence.Network.Tests;

[TestFixture, Category("Stress")]
public class ServerStressTest
{
    private const int RepeatCount = 100;

    [TearDown]
    public void ResetState()
    {
        var field = typeof(Server).GetField("_count", BindingFlags.Static | BindingFlags.NonPublic);
        ArgumentNullException.ThrowIfNull(field);
        field?.SetValue(null, 0);
    }

    [Test]
    public void MultipleWriters_UpdateCountCorrectly()
    {
        const int threadCount = 10;
        const int increment = 100;
        var barrier = new Barrier(threadCount);
        var countdown = new CountdownEvent(threadCount);

        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            Server.AddToCount(increment);
            countdown.Signal();
        })).ToArray();

        Assert.That(countdown.Wait(TimeSpan.FromSeconds(5)), Is.True, "Timeout: writers didn't complete");
        Task.WaitAll(tasks);

        Assert.That(Server.GetCount(), Is.EqualTo(threadCount * increment));
    }

    [Test]
    public void ReadersAndWriters_DoNotInterfere()
    {
        Server.AddToCount(1000);

        var writer = Task.Run(() => Server.AddToCount(500));
        var readers = Enumerable.Range(0, 20).Select(_ => Task.Run(() => Server.GetCount())).ToArray();

        Task.WaitAll(new[] { writer }.Concat(readers).ToArray());

        Assert.That(Server.GetCount(), Is.EqualTo(1500));
    }

    [Test]
    public void ConcurrentAddOperations_PreserveCorrectSum()
    {
        for (int iteration = 0; iteration < RepeatCount; iteration++)
        {
            ResetState();

            const int threadCount = 8;
            const int incrementPerThread = 123;

            var actualIncrements = new ConcurrentBag<int>();

            using var startBarrier = new Barrier(threadCount + 1);

            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    startBarrier.SignalAndWait();
                    Server.AddToCount(incrementPerThread);
                    actualIncrements.Add(incrementPerThread);
                });
            }

            startBarrier.SignalAndWait();
            Task.WaitAll(tasks);

            int expectedTotal = threadCount * incrementPerThread;
            int actualTotal = Server.GetCount();

            Assert.That(actualTotal, Is.EqualTo(expectedTotal),
                $"Iteration {iteration}: Expected {expectedTotal}, but got {actualTotal}");

            Assert.That(actualIncrements.Count, Is.EqualTo(threadCount));
        }
    }

    [Test]
    public void ConcurrentReadersAndWriters_DoNotObserveTornOrInconsistentValues()
    {
        for (int iteration = 0; iteration < RepeatCount; iteration++)
        {
            ResetState();

            Server.AddToCount(1000);

            var observedValues = new ConcurrentBag<int>();
            const int readerCount = 20;
            const int writerCount = 2;

            using var startBarrier = new Barrier(readerCount + writerCount + 1);

            var tasks = new List<Task>();

            for (int i = 0; i < readerCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    startBarrier.SignalAndWait();
                    for (int j = 0; j < 100; j++)
                    {
                        int value = Server.GetCount();
                        observedValues.Add(value);
                    }
                }));
            }

            for (int i = 0; i < writerCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    startBarrier.SignalAndWait();
                    Server.AddToCount(500);
                }));
            }

            startBarrier.SignalAndWait();

            Task.WaitAll(tasks.ToArray());

            int[] validValues = [1000, 1500, 2000];
            var invalidValues = observedValues.Where(x => !validValues.Contains(x)).ToList();
            Assert.That(invalidValues, Is.Empty,
                $"Iteration {iteration}: Found invalid values: {string.Join(", ", invalidValues)}");

            Assert.That(Server.GetCount(), Is.EqualTo(2000));
        }
    }
}
