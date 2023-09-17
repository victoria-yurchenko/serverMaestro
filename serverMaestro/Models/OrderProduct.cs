using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class OrderProduct
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
        [ForeignKey($"FK_{nameof(OrderId)}")]
        public int OrderId { get; set; }
        public double SaledByPrice { get; set; }
    }
}
