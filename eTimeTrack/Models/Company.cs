using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class Company : IMergeable, ITrackableModel
    {
        [Key]
        public int Company_Id { get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        [Display(Name = "Company Code")]
        [Required]
        public string Company_Code{ get; set; }
        [StringLength(50, ErrorMessage = "Maximum length is 50")]
        [Display(Name = "Company Name")]
        [Required]
        public string Company_Name { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Address { get; set; }
        [Display(Name = "E Org")]
        public int? E_Org { get; set; }
        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }

        [JsonIgnore]
        public virtual ICollection<Employee> Employees { get; set; }
        [JsonIgnore]
        public virtual ICollection<Office> Offices { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProjectCompany> Projects { get; set; }
        [JsonIgnore]
        public virtual ICollection<ReconciliationTemplate> ReconciliationTemplates { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public string GetId()
        {
            return Company_Id.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}