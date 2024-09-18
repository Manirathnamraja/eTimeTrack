namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProjectDetails1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeProjectDetails", "EmployeeProjectId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmployeeProjectDetails", "EmployeeProjectId");
        }
    }
}
