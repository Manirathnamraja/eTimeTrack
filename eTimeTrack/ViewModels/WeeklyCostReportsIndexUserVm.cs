using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ToDate { get; set; }
    }
}