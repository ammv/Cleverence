namespace Cleverence.Network.Synchronization
{
    /// <summary>
    /// A custom wrapper around <see cref="ReaderWriterLockSlim"/> that exposes disposable lock tokens
    /// for safe and exception-safe acquisition and release of read and write locks.
    /// </summary>
    public class ReadWriteLockCustom : IDisposable
    {
        /// <summary>
        /// Represents a write lock token that automatically releases the write lock
        /// when disposed.
        /// </summary>
        public struct WriteLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _lockSlim;

            /// <summary>
            /// Initializes a new instance of the <see cref="WriteLockToken"/> struct and acquires the write lock.
            /// </summary>
            /// <param name="lockSlim">The underlying <see cref="ReaderWriterLockSlim"/> instance.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="lockSlim"/> is null.</exception>
            public WriteLockToken(ReaderWriterLockSlim lockSlim)
            {
                _lockSlim = lockSlim ?? throw new ArgumentNullException(nameof(lockSlim));
                lockSlim.EnterWriteLock();
            }

            /// <summary>
            /// Releases the write lock.
            /// </summary>
            public void Dispose() => _lockSlim?.ExitWriteLock();
        }

        /// <summary>
        /// Represents a read lock token that automatically releases the read lock
        /// when disposed.
        /// </summary>
        public struct ReadLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _lockSlim;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReadLockToken"/> struct and acquires the read lock.
            /// </summary>
            /// <param name="lockSlim">The underlying <see cref="ReaderWriterLockSlim"/> instance.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="lockSlim"/> is null.</exception>
            public ReadLockToken(ReaderWriterLockSlim lockSlim)
            {
                _lockSlim = lockSlim ?? throw new ArgumentNullException(nameof(lockSlim));
                _lockSlim.EnterReadLock();
            }

            /// <summary>
            /// Releases the read lock.
            /// </summary>
            public void Dispose() => _lockSlim?.ExitReadLock();
        }

        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        /// <summary>
        /// Acquires a read lock and returns a disposable token to manage its lifetime.
        /// </summary>
        /// <returns>A <see cref="ReadLockToken"/> that releases the lock when disposed.</returns>
        public ReadLockToken ReadLock() => new ReadLockToken(_lockSlim);

        /// <summary>
        /// Acquires a write lock and returns a disposable token to manage its lifetime.
        /// </summary>
        /// <returns>A <see cref="WriteLockToken"/> that releases the lock when disposed.</returns>
        public WriteLockToken WriteLock() => new WriteLockToken(_lockSlim);

        /// <summary>
        /// Releases all resources used by the underlying <see cref="ReaderWriterLockSlim"/>.
        /// </summary>
        /// <remarks>
        /// This method should be called when the lock is no longer needed.
        /// Once disposed, any further attempt to acquire locks will throw an exception.
        /// </remarks>
        public void Dispose() => _lockSlim?.Dispose();
    }
}
