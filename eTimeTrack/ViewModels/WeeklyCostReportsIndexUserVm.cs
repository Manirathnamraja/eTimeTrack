using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class WeeklyCostReportsIndexUserVm
    {
        public int ProjectID { get; set; }
        public SelectList ProjectList { get; set; }
    }
}