using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using eTimeTrack.Helpers;

namespace eTimeTrack.Models
{
    public class UserType : ITrackableModel, IUserModified
    {
        [Key]
        [Display(Name = "User Type Id")]
        public int UserTypeID { get; set; }
        [Display(Name = "User type enabled")]
        public bool IsEnabled { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProjectUserType> ProjectUserTypes { get; set; }

        public UserType()
        {
            IsEnabled = true;
            Code = null;
            LastModifiedDate = DateTime.UtcNow;
            if (UserHelpers.GetCurrentUserId() != UserHelpers.Invalid)
            {
                LastModifiedBy = UserHelpers.GetCurrentUserId();
            }
        }

        public string GetId()
        {
            return UserTypeID.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void SetLastModifiedUserAndTime(int userId)
        {
            LastModifiedBy = userId;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}