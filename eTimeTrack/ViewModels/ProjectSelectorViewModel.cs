using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ProjectSelectorViewModel
    {
        public int? SelectedProjectId { get; set; }
        public SelectList Projects { get; set; }
    }
}