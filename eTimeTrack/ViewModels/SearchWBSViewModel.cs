using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class SearchWBSViewModel
    {

        public bool VarItemIsClosed { get; set; }
        public bool TaskIsClosed { get; set; }
        public bool VariationIsClosed { get; set; }
        public string TaskNo { get; set; }
        public string TaskName { get; set; }
        public string VariationNo { get; set; }
        public string VariationName { get; set; }

    }

    public class ShowSearchWPSViewModel
    {
        public List<SearchWBSViewModel> searchWPS {  get; set; }
        public bool HideClosed { get; set; }
    }
}