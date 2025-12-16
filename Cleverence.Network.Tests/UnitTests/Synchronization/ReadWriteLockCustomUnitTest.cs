using Cleverence.Network.Synchronization;

namespace Cleverence.Network.Tests;

[TestFixture]
public class ReadWriteLockCustomUnitTest
{
    [Test]
    public void MultipleReaders_CanAcquireLockConcurrently()
    {
        const int readerCount = 10;
        var barrier = new Barrier(readerCount);
        var completed = new CountdownEvent(readerCount);
        var lockInstance = new ReadWriteLockCustom();

        var tasks = Enumerable.Range(0, readerCount).Select(_ => Task.Run(() =>
        {
            using (lockInstance.ReadLock())
            {
                barrier.SignalAndWait();
                completed.Signal();
            }
        })).ToArray();

        Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Task.WaitAll(tasks);
    }

    [Test]
    public void Writer_BlocksReadersUntilDone()
    {
        var lockInstance = new ReadWriteLockCustom();
        var writerEntered = new ManualResetEventSlim(false);
        var readerStarted = new ManualResetEventSlim(false);
        var readerCompleted = new ManualResetEventSlim(false);
        var writerCompleted = new ManualResetEventSlim(false);

        var writerTask = Task.Run(() =>
        {
            using (lockInstance.WriteLock())
            {
                writerEntered.Set();
                readerStarted.Wait();
                Thread.Sleep(100);
            }
            writerCompleted.Set();
        });

        writerEntered.Wait();

        var readerTask = Task.Run(() =>
        {
            readerStarted.Set();
            using (lockInstance.ReadLock())
            {
                readerCompleted.Set();
            }
        });

        Assert.That(readerCompleted.Wait(TimeSpan.FromMilliseconds(50)), Is.False, "Reader entered lock during writing");
        writerCompleted.Wait();

        Assert.That(readerCompleted.Wait(TimeSpan.FromSeconds(1)), Is.True, "Reader was unable to enter the lock");
    }

    [Test]
    public void ExceptionInsideReadLock_DoesNotLeakLock()
    {
        var lockInstance = new ReadWriteLockCustom();
        bool exceptionThrown = false;

        try
        {
            using (lockInstance.ReadLock())
            {
                throw new InvalidOperationException("Some bug");
            }
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        Assert.That(exceptionThrown, Is.True);

        using (lockInstance.ReadLock()) { Assert.Pass(); }
    }
}
