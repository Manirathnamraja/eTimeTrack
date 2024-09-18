namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeeProjectDetails1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.EmployeeProjectDetails");
            AddColumn("dbo.EmployeeProjectDetails", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EmployeeProjectDetails", "EmployeeProjectDetailsID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.EmployeeProjectDetails", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.EmployeeProjectDetails");
            AlterColumn("dbo.EmployeeProjectDetails", "EmployeeProjectDetailsID", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.EmployeeProjectDetails", "Id");
            AddPrimaryKey("dbo.EmployeeProjectDetails", "EmployeeProjectDetailsID");
        }
    }
}
