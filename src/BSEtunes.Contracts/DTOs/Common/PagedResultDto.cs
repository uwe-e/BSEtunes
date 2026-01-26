namespace BSEtunes.Contracts.DTOs.Common
{
    /// <summary>
    /// Represents a paginated result set containing a subset of items and pagination metadata.
    /// </summary>
    /// <remarks>Use this class to encapsulate the results of a query that supports pagination, such as
    /// database queries or API responses. The pagination properties provide information about the current page, total
    /// items, and navigation between pages. This type is commonly used to facilitate efficient data retrieval and
    /// display in user interfaces that support paging.</remarks>
    /// <typeparam name="T">The type of items contained in the paginated result set.</typeparam>
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        /// <summary>
        /// Gets or sets the total number of items in the collection.
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// Gets or sets the current page number in a paginated result set.
        /// </summary>
        /// <remarks>The page number is typically 1-based, where 1 refers to the first page. Setting this
        /// property to a value less than 1 may result in no data being returned or an error, depending on the
        /// implementation.</remarks>
        public int PageNumber { get; set; }
        /// <summary>
        /// Gets or sets the number of items to include on each page of results.
        /// </summary>
        /// <remarks>Setting a larger value may improve performance by reducing the number of pages, but
        /// can increase memory usage. The value should be a positive integer.</remarks>
        public int PageSize { get; set; }
        /// <summary>
        /// Gets or sets the total number of pages available in the paginated result set.
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether there is a previous page available in the paginated result set.
        /// </summary>
        public bool HasPreviousPage { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether there is a next page available in the paginated result set.
        /// </summary>
        public bool HasNextPage { get; set; }
    }
}