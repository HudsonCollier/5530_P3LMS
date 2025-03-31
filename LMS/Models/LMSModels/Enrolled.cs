using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public string UId { get; set; } = null!;
        public string? Grade { get; set; }
        public uint ClassId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
