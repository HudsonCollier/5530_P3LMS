using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public uint Semester { get; set; }
        public string Season { get; set; } = null!;
        public uint CourseId { get; set; }
        public string Teacher { get; set; } = null!;
        public uint ClassId { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Professor TeacherNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
