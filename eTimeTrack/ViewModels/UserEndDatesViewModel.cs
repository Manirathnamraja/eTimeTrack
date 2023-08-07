using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class UserEndDatesViewModel
    {

        public int ProjectId { get; set; }

        //public string NewDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");

        //public int? EndDate { get; set; } = 0;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? NewDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? EndDate { get; set; }

        public SelectList Project { get; set; }

        public SelectList Company { get; set; }

        public SelectList DateSelect { get; set; }
    }
}