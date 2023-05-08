using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Web.Security;
using eTimeTrack.Helpers;

namespace eTimeTrack.Models
{
    public class ApplicationDbContext : IdentityDbContext<Employee, Role, int, UserLogin, UserRole, UserClaim>
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer(new ApplicationDbContextInitializer());
        }

        public ApplicationDbContext() : base("DefaultConnection")
        {
            // this.Database.CommandTimeout = 180;
           // this.Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Entity<Employee>().ToTable("Employees", "dbo").Property(p => p.Id).HasColumnName("EmployeeID");

            modelBuilder.Entity<Employee>().HasOptional(x => x.Company).WithMany(x => x.Employees).HasForeignKey(x => x.CompanyID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasOptional(x => x.Office).WithMany(x => x.Employees).HasForeignKey(x => x.OfficeID).WillCascadeOnDelete(false);
            modelBuilder.Entity<Project>().HasOptional(x => x.Office).WithMany(x => x.Projects).HasForeignKey(x => x.OfficeID).WillCascadeOnDelete(false);
            modelBuilder.Entity<EmployeeTimesheet>().HasRequired(x => x.Employee).WithMany(x => x.Timesheets).HasForeignKey(x => x.EmployeeID).WillCascadeOnDelete(false);
            modelBuilder.Entity<ProjectTask>().HasRequired(x => x.ProjectGroup).WithMany(x => x.Tasks).HasForeignKey(x => x.GroupID).WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectGroup>().HasOptional(x => x.GroupType).WithMany(x => x.ProjectGroups).HasForeignKey(x => x.GroupTypeID).WillCascadeOnDelete(false);

            modelBuilder.Entity<ReconciliationEntry>()
                .HasRequired(m => m.OriginalReconciliationUpload)
                .WithMany(t => t.OriginalReconciliationEntries)
                .HasForeignKey(m => m.OriginalReconciliationUploadId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ReconciliationEntry>()
                .HasRequired(m => m.CurrentReconciliationUpload)
                .WithMany(t => t.CurrentReconciliationEntries)
                .HasForeignKey(m => m.CurrentReconciliationUploadId)
                .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<EmployeeTimesheet> EmployeeTimesheets { get; set; }
        public DbSet<EmployeeTimesheetItem> EmployeeTimesheetItems { get; set; }
        public DbSet<LU_GroupType> LU_GroupTypes { get; set; }
        public DbSet<LU_PayType> LU_PayTypes { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectGroup> ProjectGroups { get; set; }
        public DbSet<ProjectPart> ProjectParts { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectVariation> ProjectVariations { get; set; }
        public DbSet<ProjectVariationItem> ProjectVariationItems { get; set; }
        public DbSet<TimesheetPeriod> TimesheetPeriods { get; set; }
        public DbSet<Changelog> Changes { get; set; }
        public DbSet<EmployeeProject> EmployeeProjects { get; set; }
        public DbSet<ProjectCompany> ProjectCompanies { get; set; }
        public DbSet<ProjectTimesheetPeriod> ProjectTimesheetPeriods { get; set; }
        public DbSet<SystemEvent> SystemEvents { get; set; }
        public DbSet<ProjectTimeCodeConfig> ProjectTimeCodeConfigs { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<ProjectUserType> ProjectUserTypes { get; set; }

        public DbSet<ReconciliationEntry> ReconciliationEntries { get; set; }
        public DbSet<ReconciliationTemplate> ReconciliationTemplates { get; set; }
        public DbSet<ReconciliationType> ReconciliationTypes { get; set; }
        public DbSet<ProjectDiscipline> ProjectDisciplines { get; set; }
        public DbSet<ProjectOffice> ProjectOffices { get; set; }
        public DbSet<AECOMUserClassification> AECOMUserClassifications { get; set; }
        public DbSet<ProjectUserClassification> ProjectUserClassifications { get; set; }
        public DbSet<ReconciliationUpload> ReconciliationUploads { get; set; }
        public DbSet<UserRate> UserRates { get; set; }
        public DbSet<UserRatesUpload> UserRatesUploads { get; set; }


        internal int SaveChangesWithoutLogging()
        {
            return base.SaveChanges();
        }

        public int SaveChangesWithChangelog(int currentUserId)
        {
            IEnumerable<DbEntityEntry<IUserModified>> updatedUserInfo = ChangeTracker.Entries<IUserModified>().Where(x => x.State != EntityState.Unchanged);
            foreach (DbEntityEntry<IUserModified> entry in updatedUserInfo)
            {
                entry.Entity.SetLastModifiedUserAndTime(currentUserId);
            }

            IEnumerable<DbEntityEntry<ITrackableModel>> updatedToTrack = ChangeTracker.Entries<ITrackableModel>().Where(x => x.State != EntityState.Unchanged);
            List<Tuple<DbEntityEntry<ITrackableModel>, Changelog>> toFix = new List<Tuple<DbEntityEntry<ITrackableModel>, Changelog>>();

            List<Changelog> changelogs = new List<Changelog>();

            foreach (DbEntityEntry<ITrackableModel> entry in updatedToTrack)
            {
                string originalData = string.Empty;
                Type entityType = GetEntityType(entry.Entity.GetType());
                if (entry.State != EntityState.Added)
                {
                    originalData = CreateWithValues(entityType, entry.OriginalValues).ToJson();
                }

                Changelog change = new Changelog
                {
                    Id = Guid.NewGuid(),
                    DateTimeUtc = DateTime.UtcNow,
                    EmployeeId = currentUserId,
                    IdOfChangedObject = entry.Entity.GetId(),
                    NewData = entry.Entity.ToJson(),
                    OriginalData = originalData,
                    TypeOfChangedObject = entityType.Name,
                    ChangeType = entry.State
                };

                if (entry.State == EntityState.Added && change.IdOfChangedObject == "0")
                    toFix.Add(new Tuple<DbEntityEntry<ITrackableModel>, Changelog>(entry, change));
                else
                    changelogs.Add(change);
            }

            Changes.AddRange(changelogs);
            int saveValue = base.SaveChanges();

            if (toFix.Any())
            {

                List<Changelog> toFixChangelogs = new List<Changelog>();
                //fix the Id's of those that we determined are bad
                foreach (Tuple<DbEntityEntry<ITrackableModel>, Changelog> tuple in toFix)
                {
                    tuple.Item2.IdOfChangedObject = tuple.Item1.Entity.GetId();
                    tuple.Item2.NewData = tuple.Item1.Entity.ToJson();
                    toFixChangelogs.Add(tuple.Item2);
                }

                Changes.AddRange(toFixChangelogs);
                return base.SaveChanges();
            }

            return saveValue;
        }

        public override int SaveChanges()
        {
            int currentUserId = UserHelpers.GetCurrentUserId();
            return SaveChangesWithChangelog(currentUserId);

        }

        private ITrackableModel CreateWithValues(Type t, DbPropertyValues values)
        {
            ITrackableModel entity = Activator.CreateInstance(t) as ITrackableModel;

            foreach (string name in values.PropertyNames)
            {
                PropertyInfo property = t.GetProperty(name);
                property.SetValue(entity, values.GetValue<object>(name));
            }

            return entity;
        }

        //When the type is the auto generated EF type
        private static Type GetEntityType(Type type)
        {
            if (type.Namespace != "eTimeTrack.Models")
                type = type.BaseType;

            return type;
        }
    }

    public class ApplicationDbContextInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            //Change all the primary key names so that Access can link to them
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.Companies]', @newname=N'PK_Companies'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.Employees]', @newname=N'PK_Employees'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.EmployeeTimesheetItems]', @newname=N'PK_EmployeeTimesheetItems'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.EmployeeTimesheets]', @newname=N'PK_EmployeeTimesheets'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.LU_GroupType]', @newname=N'PK_LU_GroupType'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.LU_PayType]', @newname=N'PK_LU_PayType'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.Offices]', @newname=N'PK_Offices'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.ProjectGroups]', @newname=N'PK_ProjectGroups'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.ProjectParts]', @newname=N'PK_ProjectParts'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.Projects]', @newname=N'PK_Projects'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.ProjectTasks]', @newname=N'PK_ProjectTasks'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.ProjectVariationItems]', @newname=N'PK_ProjectVariationItems'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.ProjectVariations]', @newname=N'PK_ProjectVariations'");
            context.Database.ExecuteSqlCommand($"exec sp_rename @objname=N'[dbo].[PK_dbo.TimesheetPeriods]', @newname=N'PK_TimesheetPeriods'");

            context.Roles.Add(new Role(UserHelpers.RoleUser));
            context.Roles.Add(new Role(UserHelpers.RoleAdmin));

            //SeedCompanies(context);
            //SeedTestUsers(context);

            //SeedTimesheetPeriods(context);
            //SeedProjects(context);
            //SeedProjectParts(context);
            //SeedGroupTypes(context);
            //SeedProjectGroups(context);
            //SeedVariations(context);
            //SeedTasks(context);
            //SeedVariationitems(context);
            //SeedPayTypes(context);



            context.SaveChangesWithoutLogging();
        }

        private static void SeedCompanies(ApplicationDbContext context)
        {
            context.Companies.Add(new Company { Company_Id = 1, Company_Code = "AECOM", Company_Name = "AECOM Australia", E_Org = 2 });
            context.Companies.Add(new Company { Company_Id = 2, Company_Code = "Hyder", Company_Name = "Hyder", E_Org = 2 });
        }

        private static void SeedTestUsers(ApplicationDbContext context)
        {
            UserStore<Employee, Role, int, UserLogin, UserRole, UserClaim> store = new UserStore<Employee, Role, int, UserLogin, UserRole, UserClaim>(context);
            UserManager<Employee, int> manager = new UserManager<Employee, int>(store);

            Employee e1 = new Employee { EmployeeNo = "ABC123", UserName = "test@test.com", Email = "test@test.com", EmailConfirmed = true, IsActive = true, CompanyID = 1 };
            manager.Create(e1, "Pa$$w0rd");
            manager.AddToRole(e1.Id, UserHelpers.RoleAdmin);
            Employee e2 = new Employee { EmployeeNo = "AZ3960", UserName = "test@aecom.com", Email = "test@aecom.com", EmailConfirmed = true, IsActive = true, CompanyID = 1 };
            manager.Create(e2, "Aecom123!");
            Employee e3 = new Employee { EmployeeNo = "108139", UserName = "aecom@aecom.com", Email = "aecom@aecom.com", EmailConfirmed = true, IsActive = true, CompanyID = 1, };
            manager.Create(e3, "Aecom123!");
        }

        private void SeedProjects(ApplicationDbContext context)
        {
            context.Projects.Add(new Project { ProjectID = 1, ProjectNo = "60437339", RegistrationNo = "M4", Name = "M4 East Project", DateOpened = DateTime.Parse("2015-08-18"), });
            context.Projects.Add(new Project { ProjectID = 2, ProjectNo = "12345678", RegistrationNo = "M5", Name = "Another Highway", DateOpened = DateTime.Parse("2017-03-16"), });
        }

        private void SeedProjectParts(ApplicationDbContext context)
        {
            context.ProjectParts.Add(new ProjectPart { PartID = 1, ProjectID = 1, PartNo = "00", Name = "Project Wide", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 2, ProjectID = 1, PartNo = "10", Name = "Tunnel", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 3, ProjectID = 1, PartNo = "20", Name = "Homebush Bay Drive", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 4, ProjectID = 1, PartNo = "30", Name = "Concord Rd", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 5, ProjectID = 1, PartNo = "40", Name = "Wattle St", LastModifiedDate = DateTime.Parse("2017-03-09") });

            context.ProjectParts.Add(new ProjectPart { PartID = 6, ProjectID = 2, PartNo = "50", Name = "Project Wide M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 7, ProjectID = 2, PartNo = "60", Name = "Tunnel M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 8, ProjectID = 2, PartNo = "70", Name = "Homebush Bay Drive M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 9, ProjectID = 2, PartNo = "80", Name = "Concord Rd M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectParts.Add(new ProjectPart { PartID = 10, ProjectID = 2, PartNo = "90", Name = "Wattle St M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedGroupTypes(ApplicationDbContext context)
        {
            context.LU_GroupTypes.Add(new LU_GroupType { GroupTypeID = 1, GroupTypeCode = "test", Description = "test", SortOrder = 0, IsActive = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedPayTypes(ApplicationDbContext context)
        {
            context.LU_PayTypes.Add(new LU_PayType { PayTypeID = 1, PayTypeCode = "NT", PayTypeDescription = "Normal Time", IsActive = true });
            context.LU_PayTypes.Add(new LU_PayType { PayTypeID = 2, PayTypeCode = "OT15", PayTypeDescription = "Overtime 1.5", IsActive = true });
            context.LU_PayTypes.Add(new LU_PayType { PayTypeID = 3, PayTypeCode = "OT25", PayTypeDescription = "Overtime 2.5", IsActive = true });
            context.LU_PayTypes.Add(new LU_PayType { PayTypeID = 4, PayTypeCode = "AL", PayTypeDescription = "Annual Leave", IsActive = true });
        }

        private void SeedProjectGroups(ApplicationDbContext context)
        {
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 1, ProjectID = 1, PartID = 1, GroupNo = "00-000", GroupTypeID = 1, Name = "Project Management", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 2, ProjectID = 1, PartID = 1, GroupNo = "00-100", GroupTypeID = 1, Name = "Plans", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 3, ProjectID = 1, PartID = 1, GroupNo = "00-120", GroupTypeID = 1, Name = "Reports & Studies", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 4, ProjectID = 1, PartID = 1, GroupNo = "00-200", GroupTypeID = 1, Name = "Geometry & Road Engineering", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 5, ProjectID = 1, PartID = 1, GroupNo = "00-300", GroupTypeID = 1, Name = "Pavement Design", LastModifiedDate = DateTime.Parse("2017-03-09") });

            context.ProjectGroups.Add(new ProjectGroup { GroupID = 6, ProjectID = 2, PartID = 6, GroupNo = "99-000", GroupTypeID = 1, Name = "Project Management M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 7, ProjectID = 2, PartID = 6, GroupNo = "99-100", GroupTypeID = 1, Name = "Plans M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 8, ProjectID = 2, PartID = 6, GroupNo = "99-120", GroupTypeID = 1, Name = "Reports & Studies M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 9, ProjectID = 2, PartID = 6, GroupNo = "99-200", GroupTypeID = 1, Name = "Geometry & Road Engineering M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectGroups.Add(new ProjectGroup { GroupID = 10, ProjectID = 2, PartID = 6, GroupNo = "99-300", GroupTypeID = 1, Name = "Pavement Design M5", LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedVariations(ApplicationDbContext context)
        {
            context.ProjectVariations.Add(new ProjectVariation { VariationID = 1, ProjectID = 1, VariationNo = "BU - NCR", Description = "Buildings NCR's", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariations.Add(new ProjectVariation { VariationID = 2, ProjectID = 1, VariationNo = "BU-RFI-1", Description = "Buildings RFI's - General", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariations.Add(new ProjectVariation { VariationID = 3, ProjectID = 1, VariationNo = "BU-RFI-2", Description = "Buildings RFI's - Additional", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });

            context.ProjectVariations.Add(new ProjectVariation { VariationID = 4, ProjectID = 2, VariationNo = "BU - NCR M5", Description = "Buildings NCR's M5", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariations.Add(new ProjectVariation { VariationID = 5, ProjectID = 2, VariationNo = "BU-RFI-1 M5", Description = "Buildings RFI's - General M5", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariations.Add(new ProjectVariation { VariationID = 6, ProjectID = 2, VariationNo = "BU-RFI-2 M5", Description = "Buildings RFI's - Additional M5", IsApproved = true, LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedTasks(ApplicationDbContext context)
        {
            context.ProjectTasks.Add(new ProjectTask { TaskID = 1, ProjectID = 1, GroupID = 1, Name = "Management", TaskNo = "00-000-020", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectTasks.Add(new ProjectTask { TaskID = 2, ProjectID = 1, GroupID = 1, Name = "Project Function", TaskNo = "00-000-998", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectTasks.Add(new ProjectTask { TaskID = 3, ProjectID = 1, GroupID = 1, Name = "Non-Recoverable", TaskNo = "00-000-999", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });

            context.ProjectTasks.Add(new ProjectTask { TaskID = 4, ProjectID = 2, GroupID = 6, Name = "Management M5", TaskNo = "00-000-020 M5", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectTasks.Add(new ProjectTask { TaskID = 5, ProjectID = 2, GroupID = 6, Name = "Project Function M5", TaskNo = "00-000-998 M5", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectTasks.Add(new ProjectTask { TaskID = 6, ProjectID = 2, GroupID = 6, Name = "Non-Recoverable M5", TaskNo = "00-000-999 M5", IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedVariationitems(ApplicationDbContext context)
        {
            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 1, VariationID = 1, IsApproved = true, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 2, VariationID = 1, IsApproved = false, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 3, VariationID = 2, IsApproved = true, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });

            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 4, VariationID = 4, IsApproved = true, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 5, VariationID = 4, IsApproved = true, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
            context.ProjectVariationItems.Add(new ProjectVariationItem { TaskID = 6, VariationID = 5, IsApproved = true, IsClosed = false, LastModifiedDate = DateTime.Parse("2017-03-09") });
        }

        private void SeedTimesheetPeriods(ApplicationDbContext context)
        {
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 1, StartDate = new DateTime(2016, 12, 31), EndDate = new DateTime(2017, 1, 6), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 2, StartDate = new DateTime(2017, 1, 7), EndDate = new DateTime(2017, 1, 13), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 3, StartDate = new DateTime(2017, 1, 14), EndDate = new DateTime(2017, 1, 20), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 4, StartDate = new DateTime(2017, 1, 21), EndDate = new DateTime(2017, 1, 27), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 5, StartDate = new DateTime(2017, 1, 28), EndDate = new DateTime(2017, 2, 3), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 6, StartDate = new DateTime(2017, 2, 4), EndDate = new DateTime(2017, 2, 10), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 7, StartDate = new DateTime(2017, 2, 11), EndDate = new DateTime(2017, 2, 17), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 8, StartDate = new DateTime(2017, 2, 18), EndDate = new DateTime(2017, 2, 24), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 9, StartDate = new DateTime(2017, 2, 25), EndDate = new DateTime(2017, 3, 3), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 10, StartDate = new DateTime(2017, 3, 4), EndDate = new DateTime(2017, 3, 10), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 11, StartDate = new DateTime(2017, 3, 11), EndDate = new DateTime(2017, 3, 17), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 12, StartDate = new DateTime(2017, 3, 18), EndDate = new DateTime(2017, 3, 24), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 13, StartDate = new DateTime(2017, 3, 25), EndDate = new DateTime(2017, 3, 31), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 14, StartDate = new DateTime(2017, 4, 1), EndDate = new DateTime(2017, 4, 7), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 15, StartDate = new DateTime(2017, 4, 8), EndDate = new DateTime(2017, 4, 14), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 16, StartDate = new DateTime(2017, 4, 15), EndDate = new DateTime(2017, 4, 21), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 17, StartDate = new DateTime(2017, 4, 22), EndDate = new DateTime(2017, 4, 28), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 18, StartDate = new DateTime(2017, 4, 29), EndDate = new DateTime(2017, 5, 5), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 19, StartDate = new DateTime(2017, 5, 6), EndDate = new DateTime(2017, 5, 12), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 20, StartDate = new DateTime(2017, 5, 13), EndDate = new DateTime(2017, 5, 19), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 21, StartDate = new DateTime(2017, 5, 20), EndDate = new DateTime(2017, 5, 26), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 22, StartDate = new DateTime(2017, 5, 27), EndDate = new DateTime(2017, 6, 2), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 23, StartDate = new DateTime(2017, 6, 3), EndDate = new DateTime(2017, 6, 9), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 24, StartDate = new DateTime(2017, 6, 10), EndDate = new DateTime(2017, 6, 16), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 25, StartDate = new DateTime(2017, 6, 17), EndDate = new DateTime(2017, 6, 23), IsClosed = false });
            context.TimesheetPeriods.Add(new TimesheetPeriod { WeekNo = 26, StartDate = new DateTime(2017, 6, 24), EndDate = new DateTime(2017, 6, 30), IsClosed = false });
        }
    }
}