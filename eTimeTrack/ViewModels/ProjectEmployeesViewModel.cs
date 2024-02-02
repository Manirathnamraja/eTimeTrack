using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectEmployeesViewModel
    {
        public Employee Employee { get; set; }
        public bool IsAdmin { get; set; }
        public string RoleName { get; set; }
    }
}