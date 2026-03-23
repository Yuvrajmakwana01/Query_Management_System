using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class t_Registration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
        public int c_UserId{get; set;}

        [Required(ErrorMessage ="Please enter your name !")]
        public string c_EmpName{get; set;}

        [Required(ErrorMessage ="Please enter your company name !")]
        public string c_CompanyName{get; set;}

        [Required(ErrorMessage ="Please enter your email address !")]
        [EmailAddress(ErrorMessage ="Invalid email address !")]
        public string c_EmailId{get; set;}

        [Required(ErrorMessage ="Please enter your password !")]
        [MinLength(8,ErrorMessage ="Minimum length of password is 8")]
        [DataType(DataType.Password)]
        public string c_Password{get; set;}
        
        [Display(Name="Role")]
        public string c_Role{get; set;}

    }
}