using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class UserSelectUpdateViewModel
    {
        public List<UserSelectviewmodel> UserRatesDetails {  get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? NewDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? EndDate { get; set; }

        public string Project { get; set; }

        public string Company { get; set; }

        public int CompanyId { get; set; }
    }
}