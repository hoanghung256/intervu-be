using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Intervu.Application.DTOs.SmartSearch
{
    public class SmartSearchExtractRequestDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        [RegularExpression("^(cv|jd)$", ErrorMessage = "DocType must be 'cv' or 'jd'")]
        public string DocType { get; set; } = "cv";
    }
}
