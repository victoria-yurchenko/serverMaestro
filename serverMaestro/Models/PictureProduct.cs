using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class PictureProduct
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
        [ForeignKey($"FK_{nameof(PictureId)}")]
        public int PictureId { get; set; }
    }
}
