using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using eTimeTrack.Enums;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class TimesheetApprovalViewModel
    {
        public List<TimesheetApprovaldetails> timesheetApprovaldetails { get; set; }

    }
    public class TimesheetApprovaldetails
    {
        [Range(0.0, 24.0)]
        [Display(Name = "Saturday Hours")]
        public decimal? Day1Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day1Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Sunday Hours")]
        public decimal? Day2Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day2Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Monday Hours")]
        public decimal? Day3Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day3Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Tuesday Hours")]
        public decimal? Day4Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day4Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Wednesday Hours")]
        public decimal? Day5Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day5Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Thursday Hours")]
        public decimal? Day6Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day6Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Friday Hours")]
        public decimal? Day7Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day7Comments { get; set; }

        public string Comments { get; set; }

        [StringLength(30, ErrorMessage = "Maximum length is 30"), Required]
        [Display(Name = "Task No")]
        public string TaskNo { get; set; }

        [StringLength(150, ErrorMessage = "Maximum length is 150")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessage = "Maximum length is 20"), Required]
        [Display(Name = "Variation Number")]
        public string VariationNo { get; set; }

        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        [Display(Name = "Name")]
        public string Names { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        public bool IsApproval { get; set; }
        public string Reviewercomments { get; set; }

        public string days { get; set; }

        public decimal? Hours { get; set; }

        public string DailyComments { get; set; }

        public int LastApprovedBy { get; set; }

        public DateTime LastApprovedDate { get; set; }

        public int TimesheetItemID { get; set; }

        public TimeCode TimeCode { get; set; }

        public int Timecodes { get; set; }

        public string TimecodesName { get; set; }

    }
}