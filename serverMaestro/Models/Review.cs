using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ReviewText { get; set; }
        [Required]
        public int Rate { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
        [ForeignKey($"FK_{nameof(UserId)}")]
        public int UserId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
