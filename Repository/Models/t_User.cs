using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Models
{
    public class t_User
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_UserId { get; set; }

        [Required(ErrorMessage = "User name is required")]
        [StringLength(40)]
        public string c_UserName { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(40)]
        public string? c_CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress]
        [StringLength(50)]
        public string? c_EmailId { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [StringLength(30, MinimumLength = 8)]
        public string? c_Password { get; set; }
    }
}