using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Password { get; set; } //hashed
        [Required]

        //addressId here
        [ForeignKey($"FK_{nameof(OrderAddressId)}")]
        public int OrderAddressId { get; set; }
        //public string FirstName { get; set; }
        //[Required]
        //public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        //[Required]
        //public string Address { get; set; }
        //[Required]
        //public string City { get; set; }
        //[Required]
        //public string ZipCode { get; set; }
        //[Required]
        //public string Phone { get; set; }
        //

        [ForeignKey($"FK_{nameof(RoleId)}")]
        public int RoleId { get; set; }
    }
}
