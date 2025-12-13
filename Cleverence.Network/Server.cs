using Cleverence.Network.Synchronization;

namespace Cleverence.Network
{
    /// <summary>
    /// Provides thread-safe access to a shared integer counter using a custom reader-writer lock.
    /// </summary>
    public static class Server
    {
        private static int _count = 0;
        private readonly static ReadWriteLockCustom _lock = new ReadWriteLockCustom();

        /// <summary>
        /// Gets the current value of the shared counter in a thread-safe manner.
        /// </summary>
        /// <returns>The current value of the counter.</returns>
        /// <remarks>
        /// This method acquires a read lock, ensuring safe concurrent access from multiple readers.
        /// </remarks>
        public static int GetCount()
        {
            using (_lock.ReadLock())
            {
                return _count;
            }
        }

        /// <summary>
        /// Atomically adds a specified value to the shared counter in a thread-safe manner.
        /// </summary>
        /// <param name="value">The value to add to the counter. Can be negative.</param>
        /// <remarks>
        /// This method acquires a write lock, blocking all other readers and writers until the operation completes.
        /// </remarks>
        public static void AddToCount(int value)
        {
            using (_lock.WriteLock())
            {
                _count += value;
            }
        }

#if DEBUG
        public static int GetCountDelay(TimeSpan delay)
        {
            using (_lock.ReadLock())
            {
                Thread.Sleep(delay);
                return _count;
            }
        }

        public static void AddToCountDelay(int value, TimeSpan delay)
        {
            using (_lock.WriteLock())
            {
                Thread.Sleep(delay);
                _count += value;
            }
        }
#endif
    }
}
