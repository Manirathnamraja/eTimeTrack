using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class Office : ITrackableModel, IMergeable
    {
        [Key]
        public int Office_Id { get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        [Display(Name = "Office Code")]
        public string Office_Code { get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        [Display(Name = "Office Name")]
        public string Office_Name { get; set; }
        [Display(Name = "Company")]
        public int Company_Id { get; set; }

        [JsonIgnore]
        public virtual Company Company { get; set; }
        [JsonIgnore]
        public virtual ICollection<Employee> Employees { get; set; }
        [JsonIgnore]
        public virtual ICollection<Project> Projects { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return Office_Id.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}