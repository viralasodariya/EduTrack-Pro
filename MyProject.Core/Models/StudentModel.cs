using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class StudentModel
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        [StringLength(255)]
        public string FullName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } // Must be "Male", "Female", or "Other"

        public int? ClassId { get; set; } // Nullable, references t_classes

      

        [Required]
        public DateTime EnrollmentDate { get; set; }

        public string ? ProfilePicture { get; set; } // Image URL

        public bool Status { get; set; } = true; // Active by default

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public int ? UserId { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? ClassName { get; set; }
        
    }
}