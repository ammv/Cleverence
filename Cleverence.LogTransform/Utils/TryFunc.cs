namespace Cleverence.LogTransform.Utils
{
    /// <summary>
    /// Wraps a function in a safe execution context that suppresses exceptions and returns a success flag.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// This class is useful for scenarios where functions may throw exceptions during normal operation
    /// (e.g., parsing, formatting), and callers prefer to handle failures via a boolean return value
    /// rather than exception handling.
    /// </para>
    /// <para>
    /// When <see cref="TryInvoke"/> is called:
    /// <list type="bullet">
    ///   <item>If the wrapped function executes successfully, the result is returned and <see langword="true"/> is returned.</item>
    ///   <item>If the wrapped function throws any exception, <see langword="false"/> is returned and <paramref name="output"/> is set to <c>default(TResult)</c>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Instances are immutable and thread-safe as long as the wrapped function is thread-safe.
    /// </para>
    /// </remarks>
    public class TryFunc<T, TResult>
    {
        private readonly Func<T, TResult> _func;

        /// <summary>
        /// Initializes a new instance of <see cref="TryFunc{T, TResult}"/> with the specified function.
        /// </summary>
        /// <param name="func">
        /// The function to wrap. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="func"/> is <see langword="null"/>.
        /// </exception>
        public TryFunc(Func<T, TResult> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Invokes the wrapped function with the specified input, suppressing any exceptions.
        /// </summary>
        /// <param name="input">The input to pass to the function.</param>
        /// <param name="output">
        /// When this method returns, contains the result of the function if it succeeded;
        /// otherwise, <c>default(TResult)</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the function executed without throwing an exception;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryInvoke(T input, out TResult output)
        {
            try
            {
                output = _func(input);
                return true;
            }
            catch
            {
                output = default;
                return false;
            }
        }
    }
}
