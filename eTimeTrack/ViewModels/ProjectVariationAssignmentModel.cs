using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectVariationAssignmentModel
    {
        public ProjectVariation ProjectVariation { get; set; }
        public List<ProjectTask> ProjectTasks { get; set; }
    }
}