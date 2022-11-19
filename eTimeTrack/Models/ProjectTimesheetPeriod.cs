using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectTimesheetPeriod
    {
        [Key]
        [Column(Order = 1)]
        public int ProjectID { get; set; }
        [Key]
        [Column(Order = 2)]
        public int TimesheetPeriodID { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }
        [JsonIgnore]
        [ForeignKey("TimesheetPeriodID")]
        public virtual TimesheetPeriod TimesheetPeriod { get; set; }
    }
}