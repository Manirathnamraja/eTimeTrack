namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanyRequiredFields : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "Company_Code", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Companies", "Company_Name", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "Company_Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.Companies", "Company_Code", c => c.String(maxLength: 50));
        }
    }
}
