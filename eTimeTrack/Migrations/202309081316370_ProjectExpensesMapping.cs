namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesMapping : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectExpensesMappings",
                c => new
                    {
                        MapID = c.Int(nullable: false, identity: true),
                        ProjectMapTable = c.String(),
                        ProjectTypeID = c.Int(nullable: false),
                        ProjectID = c.Int(nullable: false),
                        CompanyID = c.Int(nullable: false),
                        StdExpTypeID = c.Int(nullable: false),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.MapID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProjectExpensesMappings");
        }
    }
}
