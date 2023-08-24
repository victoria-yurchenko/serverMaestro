using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class PasswordHash
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Hash { get; set; }
        [ForeignKey($"FK_{nameof(UserId)}")]
        public int UserId { get; set; }
    }
}
