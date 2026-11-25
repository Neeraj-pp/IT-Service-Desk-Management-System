using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.DAL.Model
{
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Details { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RaisedBy { get; set; } = string.Empty;

        [Required]
        public DateTime RaisedOn { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Justification { get; set; } = string.Empty;

        public int ReqStatus { get; set; }

        public Status Status { get; set; } = null!;
    }
}