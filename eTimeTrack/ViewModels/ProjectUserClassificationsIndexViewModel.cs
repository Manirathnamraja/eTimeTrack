using System.Collections.Generic;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectUserClassificationsIndexViewModel
    {
        public List<ProjectUserClassification> ProjectUserClassifications { get; set; }

        public IEnumerable<SelectListItem> AECOMUserClassifications { get; set; }
    }
}