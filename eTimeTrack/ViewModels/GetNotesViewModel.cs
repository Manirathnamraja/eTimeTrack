using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class GetNotesViewModel
    {
        public List<ProjectGuidanceNotes> GuidanceNotes { get; set; }
    }
}