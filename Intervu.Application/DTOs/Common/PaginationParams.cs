using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.Common
{
    public class PaginationParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;
    }
}
