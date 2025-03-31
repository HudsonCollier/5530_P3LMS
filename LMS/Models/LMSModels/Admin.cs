using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Admin
    {
        public string UId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly Dob { get; set; }
    }
}
