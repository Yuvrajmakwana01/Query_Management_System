using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class t_Query
    {
        public int c_QueryId { get; set; }

        [Required]
        public int c_UserId { get; set; }

        public int? c_EmpId { get; set; }

        public string c_EmpName{ get; set;}

        [Required(ErrorMessage = "Please provide a title for the query")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string c_Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is mandatory")]
        [MinLength(10, ErrorMessage = "Please describe the issue in at least 10 characters")]
        public string c_Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Low|Medium|High", ErrorMessage = "Priority must be Low, Medium, or High")]
        public string c_Priority { get; set; } = "Low";

        [Required]
        [RegularExpression("Open|In Progress|Solved", ErrorMessage = "Invalid status")]
        public string c_Status { get; set; } = "Open";


        [StringLength(1000)]
        public string? c_Comment { get; set; }

        public DateOnly  c_QueryDate { get; set; }


        [ForeignKey("c_UserId")]
        public virtual t_User? User { get; set; }

        [ForeignKey("c_empid")]
        public virtual t_Employee? Employee { get; set; }

    }
}