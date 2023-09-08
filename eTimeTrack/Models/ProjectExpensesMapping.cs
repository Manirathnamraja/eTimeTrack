using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eTimeTrack.Models
{
    public class ProjectExpensesMapping
    {
        [Key]
        public int MapID { get; set; }
        public string ProjectMapTable { get; set; }
        public int ProjectTypeID { get; set; }
        public int ProjectID { get; set; }
        public int CompanyID { get; set; }
        public int StdExpTypeID { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}