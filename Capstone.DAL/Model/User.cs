
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HelpDesk.DAL.Model
{
    public class User
    {
        [Key]
        [StringLength(20)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(512)]
        [Column("Password")]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int RoleId { get; set; }

        public Role Role { get; set; }
    }
}
