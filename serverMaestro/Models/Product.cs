using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace serverMaestro.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double NewPrice { get; set; }
        public double OldPrice { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        public int CountOnStock { get; set; }
        public DateTime AppearedDate { get; set; }
    }
}
