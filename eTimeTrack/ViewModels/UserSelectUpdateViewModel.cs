using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class UserSelectUpdateViewModel
    {
        public Employee employee { get; set; }
        public Company company { get; set; }
        public UserRate userRate { get; set; }
        public bool Transfer { get; set; }
    }
}