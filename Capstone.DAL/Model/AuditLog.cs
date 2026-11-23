using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.DAL.Model
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditId { get; set; }

        public int RequestId { get; set; }

        [Required]
        [StringLength(30)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PerformedBy { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? Details { get; set; }
    }
}