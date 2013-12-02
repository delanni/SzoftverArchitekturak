namespace PocsKft.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class harmadik : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MetaData", "UserDefinedProperties", c => c.String());
            DropColumn("dbo.Document", "IsLocked");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Document", "IsLocked", c => c.Boolean(nullable: false));
            DropColumn("dbo.MetaData", "UserDefinedProperties");
        }
    }
}
