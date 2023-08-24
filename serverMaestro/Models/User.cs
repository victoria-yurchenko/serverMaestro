using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string AvatarUrl { get; set; }
        [ForeignKey($"FK_{nameof(RoleId)}")]
        public int RoleId { get; set; }
    }
}
