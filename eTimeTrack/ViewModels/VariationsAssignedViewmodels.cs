using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eTimeTrack.ViewModels
{
    public class VariationsAssignedViewmodels
    {
        public int projectVariationId { get; set; }
        public int taskId { get; set; }
        public bool assigned { get; set; }
    }
}