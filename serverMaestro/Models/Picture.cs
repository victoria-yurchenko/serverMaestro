using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class Picture
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PictureUrl { get; set; }

    }
}
