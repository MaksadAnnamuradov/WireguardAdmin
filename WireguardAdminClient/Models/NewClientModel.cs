using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdminClient.Models
{
    public class NewClientModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public string IPAddress { get; set; }
    }
}
