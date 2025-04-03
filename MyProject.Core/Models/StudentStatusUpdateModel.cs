using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Core.Models
{
    public class StudentStatusUpdateModel
    {
        public int StudentId { get; set; }
        public bool NewStatus { get; set; }
    }
}