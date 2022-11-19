namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Reconciliation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReconciliationEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeId = c.Int(nullable: false),
                        TimesheetPeriodId = c.Int(nullable: false),
                        Hours = c.Decimal(precision: 18, scale: 2),
                        OriginalReconciliationUploadId = c.Int(nullable: false),
                        CurrentReconciliationUploadId = c.Int(nullable: false),
                        HoursEqual = c.Boolean(nullable: false),
                        Status = c.Int(),
                        ReconciliationComment = c.String(maxLength: 255),
                        ReconciliationTypeId = c.Int(nullable: false),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReconciliationUploads", t => t.CurrentReconciliationUploadId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.ReconciliationUploads", t => t.OriginalReconciliationUploadId)
                .ForeignKey("dbo.ReconciliationTypes", t => t.ReconciliationTypeId)
                .ForeignKey("dbo.TimesheetPeriods", t => t.TimesheetPeriodId)
                .Index(t => t.EmployeeId)
                .Index(t => t.TimesheetPeriodId)
                .Index(t => t.OriginalReconciliationUploadId)
                .Index(t => t.CurrentReconciliationUploadId)
                .Index(t => t.ReconciliationTypeId);
            
            CreateTable(
                "dbo.ReconciliationUploads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Filename = c.String(),
                        UploadDateTimeUtc = c.DateTime(nullable: false),
                        ReconciliationTemplateId = c.Int(nullable: false),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReconciliationTemplates", t => t.ReconciliationTemplateId)
                .Index(t => t.ReconciliationTemplateId);
            
            CreateTable(
                "dbo.ReconciliationTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        EmployeeNumberColumn = c.String(maxLength: 2),
                        WeekEndingColumn = c.String(maxLength: 2),
                        HoursColumn = c.String(maxLength: 2),
                        CompanyId = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                        Company_Company_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.Company_Company_Id)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => t.ProjectId)
                .Index(t => t.Company_Company_Id);
            
            CreateTable(
                "dbo.ReconciliationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(maxLength: 511),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReconciliationEntries", "TimesheetPeriodId", "dbo.TimesheetPeriods");
            DropForeignKey("dbo.ReconciliationEntries", "ReconciliationTypeId", "dbo.ReconciliationTypes");
            DropForeignKey("dbo.ReconciliationEntries", "OriginalReconciliationUploadId", "dbo.ReconciliationUploads");
            DropForeignKey("dbo.ReconciliationEntries", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.ReconciliationEntries", "CurrentReconciliationUploadId", "dbo.ReconciliationUploads");
            DropForeignKey("dbo.ReconciliationUploads", "ReconciliationTemplateId", "dbo.ReconciliationTemplates");
            DropForeignKey("dbo.ReconciliationTemplates", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.ReconciliationTemplates", "Company_Company_Id", "dbo.Companies");
            DropIndex("dbo.ReconciliationTemplates", new[] { "Company_Company_Id" });
            DropIndex("dbo.ReconciliationTemplates", new[] { "ProjectId" });
            DropIndex("dbo.ReconciliationUploads", new[] { "ReconciliationTemplateId" });
            DropIndex("dbo.ReconciliationEntries", new[] { "ReconciliationTypeId" });
            DropIndex("dbo.ReconciliationEntries", new[] { "CurrentReconciliationUploadId" });
            DropIndex("dbo.ReconciliationEntries", new[] { "OriginalReconciliationUploadId" });
            DropIndex("dbo.ReconciliationEntries", new[] { "TimesheetPeriodId" });
            DropIndex("dbo.ReconciliationEntries", new[] { "EmployeeId" });
            DropTable("dbo.ReconciliationTypes");
            DropTable("dbo.ReconciliationTemplates");
            DropTable("dbo.ReconciliationUploads");
            DropTable("dbo.ReconciliationEntries");
        }
    }
}
