namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectGuidanceNotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectGuidanceNotes",
                c => new
                    {
                        GuidanceNoteId = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        TimeAdministratorContact = c.String(),
                        WBSCodes = c.String(),
                        CapWeeklyHours = c.String(),
                        MatchHours = c.String(),
                        OvertimeApproval = c.String(),
                        NTTimeCodeNote = c.String(),
                        OT1TimeCodeNote = c.String(),
                        OT2TimeCodeNote = c.String(),
                        OT3TimeCodeNote = c.String(),
                        OT4TimeCodeNote = c.String(),
                        OT5TimeCodeNote = c.String(),
                        OT6TimeCodeNote = c.String(),
                        OT7TimeCodeNote = c.String(),
                        TimesheetComments = c.String(),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.GuidanceNoteId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProjectGuidanceNotes");
        }
    }
}
