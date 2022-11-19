namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectUserTypes",
                c => new
                    {
                        ProjectUserTypeID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        UserTypeID = c.Int(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        AliasName = c.String(),
                        Description = c.String(),
                        MaxNTHours = c.Single(),
                        MaxOT1Hours = c.Single(),
                        MaxOT2Hours = c.Single(),
                        MaxOT3Hours = c.Single(),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                        Employee_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ProjectUserTypeID)
                .ForeignKey("dbo.Projects", t => t.ProjectID)
                .ForeignKey("dbo.UserTypes", t => t.UserTypeID)
                .ForeignKey("dbo.Employees", t => t.Employee_Id)
                .Index(t => t.ProjectID)
                .Index(t => t.UserTypeID)
                .Index(t => t.Employee_Id);
            
            CreateTable(
                "dbo.UserTypes",
                c => new
                    {
                        UserTypeID = c.Int(nullable: false, identity: true),
                        IsEnabled = c.Boolean(nullable: false),
                        Name = c.String(),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserTypeID)
                .ForeignKey("dbo.Employees", t => t.LastModifiedBy)
                .Index(t => t.LastModifiedBy);
            
            AddColumn("dbo.EmployeeProjects", "ProjectUserTypeID", c => c.Int());
            CreateIndex("dbo.EmployeeProjects", "ProjectUserTypeID");
            AddForeignKey("dbo.EmployeeProjects", "ProjectUserTypeID", "dbo.ProjectUserTypes", "ProjectUserTypeID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectUserTypes", "Employee_Id", "dbo.Employees");
            DropForeignKey("dbo.ProjectUserTypes", "UserTypeID", "dbo.UserTypes");
            DropForeignKey("dbo.UserTypes", "LastModifiedBy", "dbo.Employees");
            DropForeignKey("dbo.ProjectUserTypes", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.EmployeeProjects", "ProjectUserTypeID", "dbo.ProjectUserTypes");
            DropIndex("dbo.UserTypes", new[] { "LastModifiedBy" });
            DropIndex("dbo.ProjectUserTypes", new[] { "Employee_Id" });
            DropIndex("dbo.ProjectUserTypes", new[] { "UserTypeID" });
            DropIndex("dbo.ProjectUserTypes", new[] { "ProjectID" });
            DropIndex("dbo.EmployeeProjects", new[] { "ProjectUserTypeID" });
            DropColumn("dbo.EmployeeProjects", "ProjectUserTypeID");
            DropTable("dbo.UserTypes");
            DropTable("dbo.ProjectUserTypes");
        }
    }
}
