using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class t_Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int c_EmpId { get; set; }

        [Required(ErrorMessage = "Please Enter Employee Name")]
        [Display(Name = "Employee Name")]
        [StringLength(150, MinimumLength = 3)]
        public string? c_EmpName { get; set; } 

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string c_Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please Enter Password")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 6)]
        public string? c_Password { get; set; }

        [Required(ErrorMessage = "Please Select Role")]
        [Display(Name = "Role")]
        public string? c_Role { get; set; } = "employee";

        public int SolvedCount { get; set; }
    }
}