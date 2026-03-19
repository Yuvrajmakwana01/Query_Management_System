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

        [Required(ErrorMessage ="Employee/User Name is required !!")]
        public string c_EmpName{get; set;}

        [Required(ErrorMessage ="Company Name is required !! ")]
        public string c_CompanyName{get; set;}

        [Required(ErrorMessage ="Email Id is required !! ")]
        [EmailAddress(ErrorMessage ="Invalid Email Format !! ")]
        public string c_EmailId{get; set;}

        [Required(ErrorMessage ="Password is required !")]
        [MinLength(8,ErrorMessage ="Minimum length of password is 8 !")]
        public string c_Password{get; set;}
        
        public string c_Role{get; set;}

    }
}