
namespace Capstone.API.DTOs
{
    /// <summary>
    /// Represents a paginated API response containing a collection of records.
    /// </summary>
    /// <typeparam name="T">The type of records returned in the collection.</typeparam>
    public class PagedResponseDto<T>
    {
        /// <summary>
        /// Gets or sets the records returned on the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of matching records.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Gets or sets the total number of available pages.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
