using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
        [ForeignKey($"FK_{nameof(UserId)}")]
        public int UserId { get; set; }
    }
}
