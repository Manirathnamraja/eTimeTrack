using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.ViewModels
{
    public class EmployeeProjectDetailsViewModel
    {
        public int EmployeeID { get; set; }
        [DisplayName("Employee Number")]
        public string EmployeeNo { get; set; }
        [DisplayName("Name")]
        public string Names { get; set; }
        [DisplayName("Email")]
        public string EmailAddress { get; set; }
        [DisplayName("Project User Type")]
        public string ProjectUserTypeID { get; set; }
        [DisplayName("Project Role")]
        public string ProjectRole { get; set; }

        [DisplayName("Project Discipline")]
        public string ProjectDisciplineID { get; set; }
        [DisplayName("Office")]
        public string OfficeID { get; set; }
    }
}