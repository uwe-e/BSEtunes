using System.Runtime.Versioning;

namespace BSEtunes.Infrastructure.Security
{
    /// <summary>
    /// Provides methods to execute file operations under the security context of a specified Windows user account by
    /// impersonating that user.
    /// </summary>
    /// <remarks>Use this class to perform file or resource access that requires the permissions of another
    /// Windows user. All impersonation operations are supported only on Windows platforms. The class should be disposed
    /// after use to release any associated resources.</remarks>
    public class ImpersonatedFileAccessor : IDisposable
    {
        private readonly WindowsImpersonator _impersonator;
        /// <summary>
        /// creates an instance of ImpersonatedFileAccessor that impersonates the specified Windows user account. The
        /// </summary>
        /// <param name="username">the user name of the Windows account to impersonate. Cannot be null or empty.</param>
        /// <param name="password">the password for the specified user account. Cannot be null or empty.</param>
        /// <param name="domain">the domain of the user account. If null, the local computer is used.</param>
        [SupportedOSPlatform("windows")]
        public ImpersonatedFileAccessor(string username, string password, string? domain = null)
        {
            _impersonator = new WindowsImpersonator(username, password, domain);
        }
        /// <summary>
        /// Executes the specified asynchronous action under an impersonated Windows user context and returns its
        /// result.
        /// </summary>
        /// <remarks>This method is supported only on Windows platforms. The action is executed with the
        /// security context of the impersonated user. If the action throws an exception, the returned task will be
        /// faulted with that exception.</remarks>
        /// <typeparam name="T">The type of the result returned by the asynchronous action.</typeparam>
        /// <param name="action">A function that represents the asynchronous operation to execute under impersonation. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the value returned by the
        /// specified action.</returns>
        [SupportedOSPlatform("windows")]
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            return await _impersonator.RunImpersonatedAsync(action);
        }
        /// <summary>
        /// Executes the specified asynchronous action under an impersonated Windows user context.
        /// </summary>
        /// <remarks>This method is supported only on Windows platforms. The provided action is executed
        /// with the security context of the impersonated user. Any exceptions thrown by the action will be propagated
        /// to the returned task.</remarks>
        /// <param name="action">A delegate that represents the asynchronous action to execute while impersonated. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when the impersonated action has
        /// finished executing.</returns>
        [SupportedOSPlatform("windows")]
        public async Task ExecuteAsync(Func<Task> action)
        {
            await _impersonator.RunImpersonatedAsync(action);
        }
        /// <summary>
        /// Executes the specified action delegate under an impersonated Windows user context and returns its result.
        /// </summary>
        /// <remarks>This method is supported only on Windows platforms. The action is executed with the
        /// credentials of the impersonated user. Any exceptions thrown by the action delegate will propagate to the
        /// caller.</remarks>
        /// <typeparam name="T">The type of the value returned by the action delegate.</typeparam>
        /// <param name="action">A delegate that represents the action to execute under impersonation. Cannot be null.</param>
        /// <returns>The result of executing the specified action delegate.</returns>
        [SupportedOSPlatform("windows")]
        public T Execute<T>(Func<T> action)
        {
            return _impersonator.RunImpersonated(action);
        }
        /// <summary>
        /// Executes the specified action delegate under an impersonated Windows user context.
        /// </summary>
        /// <remarks>This method is supported only on Windows platforms. The impersonation context applies
        /// only for the duration of the specified action. Any exceptions thrown by the action delegate will propagate
        /// to the caller.</remarks>
        /// <param name="action">The delegate to execute while impersonation is active. Cannot be null.</param>
        [SupportedOSPlatform("windows")]
        public void Execute(Action action)
        {
            _impersonator.RunImpersonated(action);
        }
        /// <summary>
        /// disposes the ImpersonatedFileAccessor instance and releases any resources associated with the impersonation context. After
        /// </summary>
        public void Dispose()
        {
            // SafeAccessTokenHandle will be disposed by garbage collection
            GC.SuppressFinalize(this);
        }
    }
}