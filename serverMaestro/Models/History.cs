using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey($"FK_{nameof(UserId)}")]
        public int UserId { get; set; }
        [ForeignKey($"FK_{nameof(OrderId)}")]
        public int OrderId { get; set; }
    }
}
