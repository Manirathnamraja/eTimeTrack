namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectTimeCodeConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectTimeCodeConfigs",
                c => new
                    {
                        ProjectTimeCodeConfigId = c.Int(nullable: false),
                        ProjectID = c.Int(nullable: false),
                        DisplayOT1 = c.Boolean(nullable: false),
                        DisplayOT2 = c.Boolean(nullable: false),
                        DisplayOT3 = c.Boolean(nullable: false),
                        NTName = c.String(),
                        OT1Name = c.String(),
                        OT2Name = c.String(),
                        OT3Name = c.String(),
                        NTNotes = c.String(),
                        OT1Notes = c.String(),
                        OT2Notes = c.String(),
                        OT3Notes = c.String(),
                    })
                .PrimaryKey(t => t.ProjectTimeCodeConfigId)
                .ForeignKey("dbo.Projects", t => t.ProjectTimeCodeConfigId)
                .Index(t => t.ProjectTimeCodeConfigId);
            
            AddColumn("dbo.Projects", "ProjectTimeCodeConfigId", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectTimeCodeConfigs", "ProjectTimeCodeConfigId", "dbo.Projects");
            DropIndex("dbo.ProjectTimeCodeConfigs", new[] { "ProjectTimeCodeConfigId" });
            DropColumn("dbo.Projects", "ProjectTimeCodeConfigId");
            DropTable("dbo.ProjectTimeCodeConfigs");
        }
    }
}
