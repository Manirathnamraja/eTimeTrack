using System.Collections.Generic;
using eTimeTrack.Controllers;

namespace eTimeTrack.Models
{
    public class ProjectComponentViewModel
    {
        public ProjectComponentType Type { get; set; }
        public List<ProjectComponent> ProjectComponents { get; set; }
    }
}