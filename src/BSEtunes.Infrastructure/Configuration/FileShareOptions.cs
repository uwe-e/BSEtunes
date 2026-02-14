namespace BSEtunes.Infrastructure.Configuration
{
    /// <summary>
    /// Represents configuration options for accessing a network file share, including authentication credentials and
    /// share path information.
    /// </summary>
    /// <remarks>Use this class to specify the necessary parameters when connecting to a file share that
    /// requires authentication. All properties should be set before attempting to establish a connection. The class
    /// does not perform validation on the provided values.</remarks>
    public class FileShareOptions
    {
        /// <summary>
        /// username of the account to use when connecting to the file share. This should be a valid user account that has
        /// access permissions to the target share.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the password associated with the current user or entity.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the domain of the user account. If null, the local computer is used.
        /// </summary>
        public string? Domain { get; set; }
        /// <summary>
        /// Gets or sets the network share path.
        /// </summary>
        public string SharePath { get; set; } = string.Empty;
    }
}