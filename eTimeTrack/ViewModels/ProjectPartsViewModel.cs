using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectPartsViewModel
    {
        public ProjectPart part { get; set; }
        public SelectList employees { get; set; }
    }
}