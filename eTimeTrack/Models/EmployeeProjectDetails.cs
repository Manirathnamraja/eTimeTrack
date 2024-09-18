using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace eTimeTrack.Models
{
    public class EmployeeProjectDetails : ITrackableModel
    {
        [Key]
        public int EmployeeProjectDetailsID { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int? ProjectUserTypeID { get; set; }
        public int? ProjectDisciplineID { get; set; }
        public int? OfficeID { get; set; }
        public string ProjectRole { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectUserTypeID")]
        public virtual ProjectUserType ProjectUserType { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectDisciplineID")]
        public virtual ProjectDiscipline ProjectDiscipline { get; set; }

       
        public string GetId()
        {
            return EmployeeProjectDetailsID.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}