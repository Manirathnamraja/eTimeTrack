using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace eTimeTrack.ViewModels
{
    public class TimesheetCreateViewModel
    {
        [Range(1, 52)]
        [Required]
        public int Weeks { get; set; }
        [Display(Name="Period Closed")]
        public bool IsClosed { get; set; }
    }
}