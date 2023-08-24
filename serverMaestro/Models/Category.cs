using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace serverMaestro.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [AllowNull]
        public string CategoryImagePath { get; set; }
    }
}
