using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CachingFramework.Redis
{
    /// <summary>
    /// Task Extensions.
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/a/13494570/122195
    /// </remarks>
    internal static partial class TaskExtensions
    {
        /// <summary>
        /// Configure a task to be awaited without re-entering to the request context (for performance).
        /// </summary>
        /// <param name="task">The task.</param>
        public static ConfiguredTaskAwaitable ForAwait(this Task task)
        {
            return task.ConfigureAwait(false);
        }
        /// <summary>
        /// Configure a task to be awaited without re-entering to the request context (for performance).
        /// </summary>
        /// <param name="task">The task.</param>
        public static ConfiguredTaskAwaitable<T> ForAwait<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
        /// <summary>
        /// Forgets the execution of a task. Useful to avoid compiler warnings.
        /// </summary>
        /// <param name="task">The task.</param>
        public static void Forget(this Task task)
        {
        }
    }
}
