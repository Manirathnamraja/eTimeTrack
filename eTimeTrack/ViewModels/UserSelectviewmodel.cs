using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eTimeTrack.ViewModels
{
    public class UserSelectviewmodel
    {
        public int ProjectId { get; set; }

        //[Required]
        //public string NewDate { get; set; } 

        //[Required]
        //public string EndDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? NewDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? EndDate { get; set; }

        [Required]
        public string Project { get; set; }

        [Required]
        public string Company { get; set; }

        public string DateSelect { get; set; }
        public string UserNumber { get; set; }
        public string UserName { get; set; }

        public bool Transfer { get; set; }

        public int EmployeeID { get; set; }

        public int Company_Id { get; set; }
        
        public int UserRateId { get; set; }
    }

    
}