using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class Feature
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FeatureName { get; set; }
        [Required]
        public string FeatureValue { get; set; }
        [ForeignKey($"FK_{nameof(ProductId)}")]
        public int ProductId { get; set; }
    }
}
