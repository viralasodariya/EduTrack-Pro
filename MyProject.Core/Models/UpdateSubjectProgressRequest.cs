using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class UpdateSubjectProgressRequest
    {
        public int AssignmentId { get; set; } // Unique ID for the teacher-subject assignment
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}