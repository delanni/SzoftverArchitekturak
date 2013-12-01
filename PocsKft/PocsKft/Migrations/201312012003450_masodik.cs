namespace PocsKft.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class masodik : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Permission",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserOrGroupId = c.Int(nullable: false),
                        CommonAncestorId = c.Int(nullable: false),
                        IsFolder = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Lock",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        CommonAncestorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Lock");
            DropTable("dbo.Permission");
        }
    }
}
