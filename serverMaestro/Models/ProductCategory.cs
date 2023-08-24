using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey($"FK_{nameof(CategoryId)}")]
        public int CategoryId { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
    }
}
