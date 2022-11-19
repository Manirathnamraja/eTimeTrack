using System;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class ImportUserTimes
    {
        public int Row { get; set; }
        [StringLength(20, ErrorMessage = "Maximum length is 20"), Required]
        [Display(Name = "Employee Number")]
        public string EmployeeNo { get; set; }
        public string Username { get; set; }
        public DateTime EndDate { get; set; }
        public ProjectTask ProjectTask { get; set; }
        public ProjectVariation Variation { get; set; }
        public decimal Day1Hours { get; set; }
        public decimal Day2Hours { get; set; }
        public decimal Day3Hours { get; set; }
        public decimal Day4Hours { get; set; }
        public decimal Day5Hours { get; set; }
        public decimal Day6Hours { get; set; }
        public decimal Day7Hours { get; set; }
    }
}