using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace serverMaestro.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string OrderStatus { get; set; }
        [ForeignKey($"FK_{nameof(UserId)}")]
        public int UserId { get; set; }
        public string OrderNotes { get; set; }
        [ForeignKey($"FK_{nameof(OrderAddressId)}")]
        public int OrderAddressId { get; set; }
    }
}
