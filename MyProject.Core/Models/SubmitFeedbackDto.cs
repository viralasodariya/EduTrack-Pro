using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class SubmitFeedbackDto
    {
        public int TeacherId { get; set; }
        public int ExperienceRating { get; set; } // 1-5 rating
        public string Feedback { get; set; }
    }
}