using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class EmployeeProject : ITrackableModel
    {
        [Key]
        public int EmployeeProjectId { get; set; }
        [Index("IX_EmployeeProjectRestraint", 1, IsUnique = true)]
        public int EmployeeId { get; set; }
        [Index("IX_EmployeeProjectRestraint", 2, IsUnique = true)]
        public int ProjectId { get; set; }
        public int? ProjectUserTypeID { get; set; }
        public int? ProjectDisciplineID { get; set; }
        public string ProjectRole { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectUserTypeID")]
        public virtual ProjectUserType ProjectUserType { get; set; }

        [JsonIgnore]
        [ForeignKey("ProjectDisciplineID")]
        public virtual ProjectDiscipline ProjectDiscipline { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }
        
        public string GetId()
        {
            return EmployeeProjectId.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}