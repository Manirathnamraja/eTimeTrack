namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpenseType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectExpenseTypes",
                c => new
                    {
                        ExpenseTypeID = c.Int(nullable: false, identity: true),
                        ExpenseType = c.String(),
                        VariationID = c.Int(nullable: false),
                        TaskID = c.Int(nullable: false),
                        ProjectID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ExpenseTypeID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProjectExpenseTypes");
        }
    }
}
