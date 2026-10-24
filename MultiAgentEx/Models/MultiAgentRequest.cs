using System.ComponentModel.DataAnnotations;

namespace MultiAgentEx.Models
{
    public sealed class MultiAgentRequest
    {
        [Required]
        [StringLength(5000, MinimumLength = 5)]
        public string Question { get; set; } = string.Empty;
    }

}
