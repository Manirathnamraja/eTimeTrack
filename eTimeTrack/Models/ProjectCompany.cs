using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ProjectCompany : ITrackableModel
    {
        [Key]
        public int ProjectCompanyId { get; set; }
        [Index("IX_ProjectCompanyRestraint", 1, IsUnique = true)]
        public int ProjectId { get; set; }
        [Index("IX_ProjectCompanyRestraint", 2, IsUnique = true)]
        public int CompanyId { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }
        [JsonIgnore]
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        
        public string GetId()
        {
            return ProjectCompanyId.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}