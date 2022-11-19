namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReconProjectTemplateToUpload : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReconciliationUploads", "ProjectId", c => c.Int(nullable: true));

            Sql("UPDATE ReconciliationUploads SET ReconciliationUploads.ProjectId = RT.ProjectId FROM ReconciliationUploads RU INNER JOIN ReconciliationTemplates RT ON RU.ReconciliationTemplateId = RT.Id");

            AlterColumn("dbo.ReconciliationUploads", "ProjectId", c => c.Int(nullable: false));

            CreateIndex("dbo.ReconciliationUploads", "ProjectId");
            AddForeignKey("dbo.ReconciliationUploads", "ProjectId", "dbo.Projects", "ProjectID");

            DropForeignKey("dbo.ReconciliationTemplates", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ReconciliationTemplates", new[] { "ProjectId" });
            DropColumn("dbo.ReconciliationTemplates", "ProjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReconciliationTemplates", "ProjectId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ReconciliationUploads", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ReconciliationUploads", new[] { "ProjectId" });
            DropColumn("dbo.ReconciliationUploads", "ProjectId");
            CreateIndex("dbo.ReconciliationTemplates", "ProjectId");
            AddForeignKey("dbo.ReconciliationTemplates", "ProjectId", "dbo.Projects", "ProjectID");
        }
    }
}
