using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class TimetableModel
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public string DayOfWeek { get; set; }
    }
}