namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanyActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsClosed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsClosed");
        }
    }
}
