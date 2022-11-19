namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectCompanyJoin : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectCompanies",
                c => new
                    {
                        ProjectCompanyId = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        CompanyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProjectCompanyId)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => new { t.ProjectId, t.CompanyId }, unique: true, name: "IX_ProjectCompanyRestraint");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectCompanies", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.ProjectCompanies", "CompanyId", "dbo.Companies");
            DropIndex("dbo.ProjectCompanies", "IX_ProjectCompanyRestraint");
            DropTable("dbo.ProjectCompanies");
        }
    }
}
