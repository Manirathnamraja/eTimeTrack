using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class TaskTransferSelectViewModel
    {
        public int ProjectId { get; set; }
        public ProjectTask TaskFrom { get; set; }
        public ProjectVariation VariationFrom { get; set; }
        public ProjectTask TaskTo { get; set; }
        public ProjectVariation VariationTo { get; set; }
        public int? StartDate { get; set; }
        public int? EndDate { get; set; }

        public SelectList ProjectParts { get; set; }
        public SelectList DateSelect { get; set; }
    }
}