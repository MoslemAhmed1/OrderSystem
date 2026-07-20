using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Required][StringLength(50)] public string FirstName { get; set; }
        [Required][StringLength(50)] public string LastName { get; set; }
        [Required] public CustomerType CustomerType { get; set; }
    }
}

/*
    Employee: 0.5
    VIP: 0.8
    WholeSale: 0.85
    Regular: 1
*/