using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WireguardAdminClient.Models
{
    public class SignupModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|" +
          "(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$",
          ErrorMessage = "Passwords must be at least 8 characters and contain one or more of the following: upper case (A-Z), lower case (a-z), number (0-9) and special characters (e.g. !@#$%^&*)")]
        public string Password { get; set; }

        public DateTime BirthDate { get; set; }
        public string ProfileDescription { get; set; }
        public string FavoritePet { get; set; }
    }
}
