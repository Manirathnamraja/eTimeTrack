using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class UserRole : IdentityUserRole<int> { }
    public class UserClaim : IdentityUserClaim<int> { }
    public class UserLogin : IdentityUserLogin<int> { }

    public class Role : IdentityRole<int, UserRole>
    {
        public Role() { }
        public Role(string name) { Name = name; }
    }

    public class UserStore : UserStore<Employee, Role, int, UserLogin, UserRole, UserClaim>
    {
        public UserStore(ApplicationDbContext context) : base(context) { }
    }

    public class RoleStore : RoleStore<Role, int, UserRole>
    {
        public RoleStore(ApplicationDbContext context) : base(context)
        {
        }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class Employee : IdentityUser<int, UserLogin, UserRole, UserClaim>, ITrackableModel, IUserModified, IMergeable
    {
        public Employee()
        {
            IsActive = true;
        }

        [StringLength(20, ErrorMessage = "Maximum length is 20"), Required]
        [Display(Name = "Employee Number")]
        public string EmployeeNo { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        [Display(Name = "Name")]
        public string Names { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [ForeignKey("Manager")]
        [Display(Name = "Manager")]
        public int? ManagerID { get; set; }
        [Display(Name = "Company Name")]
        public int? CompanyID { get; set; }
        [Display(Name = "Office")]
        public int? OfficeID { get; set; }
        [Display(Name = "Allow Overtime")]
        public bool? AllowOT { get; set; }
        [ForeignKey("LastModifiedByUser")]
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public override DateTime? LockoutEndDateUtc
        {
            get { return base.LockoutEndDateUtc; }
            set
            {
                base.LockoutEndDateUtc = value;
                if (value == null)
                {
                    LockoutDateTimeUtc = null;
                }
                else
                {
                    if (LockoutDateTimeUtc == null)
                    {
                        LockoutDateTimeUtc = DateTime.UtcNow;
                    }
                }
            }
        }

        public DateTime? LockoutDateTimeUtc { get; set; }

        public string FullNameSurname
        {
            get
            {
                string[] names = Names?.Split(new[] { ", " }, StringSplitOptions.None);
                return names?.FirstOrDefault() ?? string.Empty;
            }
        }

        public string FullNameFirstName
        {
            get
            {
                string[] names = Names?.Split(new[] { ", " }, StringSplitOptions.None);
                return names != null && names.Length > 1 ? names[1] : string.Empty;
            }
        }

        [JsonIgnore]
        public virtual Employee Manager { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }
        [JsonIgnore]
        public virtual Office Office { get; set; }
        [JsonIgnore]
        public virtual Employee LastModifiedByUser { get; set; }
        [JsonIgnore]
        public virtual ICollection<EmployeeTimesheet> Timesheets { get; set; }
        //[JsonIgnore]
        //public virtual ICollection<ProjectUserType> ProjectUserTypes { get; set; }
        [JsonIgnore]
        public virtual ICollection<EmployeeProject> Projects { get; set; }

        [JsonIgnore]
        [ForeignKey("ManagerID")]
        public virtual ICollection<Employee> ManagedEmployees { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual ICollection<Employee> LastModifiedEmployees { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> ReconciliationEntries { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string GetId()
        {
            return Id.ToString();
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