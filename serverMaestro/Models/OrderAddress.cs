﻿using System.ComponentModel.DataAnnotations;

namespace serverMaestro.Models
{
    public class OrderAddress
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
    }
}
