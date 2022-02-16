using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace WireguardAdmin.Models
{
    public class NewUserModelDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
