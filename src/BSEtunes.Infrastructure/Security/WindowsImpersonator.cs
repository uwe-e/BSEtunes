using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace BSEtunes.Infrastructure.Security
{
    /// <summary>
    /// Provides functionality to impersonate a Windows user and execute code under that user's security context.
    /// </summary>
    /// <remarks>Use this class to run code as a different Windows user by supplying valid credentials. The
    /// impersonation is applied only to the code executed within the provided delegate methods. This class is supported
    /// only on Windows platforms.</remarks>
    public class WindowsImpersonator
    {
        private readonly SafeAccessTokenHandle _tokenHandle;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string? lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeAccessTokenHandle phToken);

        // Logon types
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        // Logon provider
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        
        /// <summary>
        /// Initializes a new instance of the WindowsImpersonator class and attempts to impersonate the specified
        /// Windows user account.
        /// </summary>
        /// <remarks>This constructor is supported only on Windows platforms. The impersonation remains in
        /// effect for the lifetime of the WindowsImpersonator instance. Ensure that sensitive credentials are handled
        /// securely.</remarks>
        /// <param name="username">The user name of the Windows account to impersonate. Cannot be null or empty.</param>
        /// <param name="password">The password for the specified user account. Cannot be null or empty.</param>
        /// <param name="domain">The domain of the user account. If null, the local computer is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if the impersonation attempt fails, such as when the credentials are invalid or the logon process
        /// encounters an error.</exception>
        [SupportedOSPlatform("windows")]
        public WindowsImpersonator(string username, string password, string? domain = null)
        {
            bool returnValue = LogonUser(
                username,
                domain,
                password,
                LOGON32_LOGON_NEW_CREDENTIALS,
                LOGON32_PROVIDER_DEFAULT,
                out _tokenHandle);

            if (!returnValue)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException(
                    $"Failed to impersonate user. Error code: {errorCode}");
            }
        }

        /// <summary>
        /// Executes the specified asynchronous action as the impersonated Windows user and returns its result.
        /// </summary>
        /// <remarks>Use this method to run asynchronous code under the security context of the
        /// impersonated Windows user associated with this instance. The impersonation is applied only for the duration
        /// of the provided action. This method is supported only on Windows platforms.</remarks>
        /// <typeparam name="T">The type of the result returned by the asynchronous action.</typeparam>
        /// <param name="action">A function that represents the asynchronous operation to execute under impersonation. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the value returned by the
        /// action.</returns>
        [SupportedOSPlatform("windows")]
        public async Task<T> RunImpersonatedAsync<T>(Func<Task<T>> action)
        {
            return await WindowsIdentity.RunImpersonatedAsync(_tokenHandle, action);
        }

        /// <summary>
        /// Runs the specified asynchronous action under the security context of the impersonated Windows identity.
        /// </summary>
        /// <remarks>Use this method to perform asynchronous operations as a different Windows user. The
        /// impersonation is applied only for the duration of the provided action. This method is supported only on
        /// Windows platforms.</remarks>
        /// <param name="action">A delegate that represents the asynchronous action to execute while impersonating the Windows identity.
        /// Cannot be null.</param>
        /// <returns>A task that represents the asynchronous impersonation operation.</returns>
        [SupportedOSPlatform("windows")]
        public async Task RunImpersonatedAsync(Func<Task> action)
        {
            await WindowsIdentity.RunImpersonatedAsync(_tokenHandle, action);
        }

        /// <summary>
        /// Runs the specified action delegate in the security context of the impersonated Windows identity and returns
        /// its result.
        /// </summary>
        /// <remarks>Use this method to perform operations that require the permissions of the
        /// impersonated Windows identity. The impersonation is applied only for the duration of the action delegate.
        /// This method is supported only on Windows platforms.</remarks>
        /// <typeparam name="T">The type of the value returned by the action delegate.</typeparam>
        /// <param name="action">A delegate that represents the action to execute under the impersonated Windows identity. Cannot be null.</param>
        /// <returns>The value returned by the action delegate.</returns>
        [SupportedOSPlatform("windows")]
        public T RunImpersonated<T>(Func<T> action)
        {
            return WindowsIdentity.RunImpersonated(_tokenHandle, action);
        }

        /// <summary>
        /// Executes the specified action while impersonating the Windows identity associated with this instance.
        /// </summary>
        /// <remarks>Use this method to perform operations that require the security context of the
        /// impersonated user. This method is supported only on Windows platforms.</remarks>
        /// <param name="action">The action to execute under the impersonated Windows identity. Cannot be null.</param>
        [SupportedOSPlatform("windows")]
        public void RunImpersonated(Action action)
        {
            WindowsIdentity.RunImpersonated(_tokenHandle, action);
        }
    }
}