using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class NewClientModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public string IPAddress { get; set; }
    }
}
