using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class UserEndDatesViewModel
    {

        public int ProjectId { get; set; }

        public string NewDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");

        public int? EndDate { get; set; } = 0;

        public SelectList Project { get; set; }

        public SelectList Company { get; set; }

        public SelectList DateSelect { get; set; }
    }
}