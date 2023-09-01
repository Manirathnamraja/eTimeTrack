namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectExpensesStdDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectExpensesStdDetails",
                c => new
                    {
                        StdTypeID = c.Int(nullable: false, identity: true),
                        StdType = c.String(),
                        CompanyID = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        LastModifiedBy = c.Int(),
                        LastModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.StdTypeID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProjectExpensesStdDetails");
        }
    }
}
