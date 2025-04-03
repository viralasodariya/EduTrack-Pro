using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class SubjectProgressModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal CompletionPercentage { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; } = "Unassigned";
    }
}