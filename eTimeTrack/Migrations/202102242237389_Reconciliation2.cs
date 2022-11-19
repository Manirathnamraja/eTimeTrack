namespace eTimeTrack.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Reconciliation2 : DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM ReconciliationTemplates");
            DropIndex("dbo.ReconciliationTemplates", new[] { "Company_Company_Id" });
            DropColumn("dbo.ReconciliationTemplates", "CompanyId");
            RenameColumn(table: "dbo.ReconciliationTemplates", name: "Company_Company_Id", newName: "CompanyId");
            AlterColumn("dbo.ReconciliationTemplates", "CompanyId", c => c.Int(nullable: false));
            CreateIndex("dbo.ReconciliationTemplates", "CompanyId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ReconciliationTemplates", new[] { "CompanyId" });
            AlterColumn("dbo.ReconciliationTemplates", "CompanyId", c => c.Int());
            RenameColumn(table: "dbo.ReconciliationTemplates", name: "CompanyId", newName: "Company_Company_Id");
            AddColumn("dbo.ReconciliationTemplates", "CompanyId", c => c.Int(nullable: false));
            CreateIndex("dbo.ReconciliationTemplates", "Company_Company_Id");
        }
    }
}
