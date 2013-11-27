namespace PocsKft.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class elso : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Group_GroupId = c.Int(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Group", t => t.Group_GroupId)
                .Index(t => t.Group_GroupId);
            
            CreateTable(
                "dbo.Group",
                c => new
                    {
                        GroupId = c.Int(nullable: false, identity: true),
                        GroupName = c.String(),
                        Metadata_Id = c.Int(),
                    })
                .PrimaryKey(t => t.GroupId)
                .ForeignKey("dbo.MetaData", t => t.Metadata_Id)
                .Index(t => t.Metadata_Id);
            
            CreateTable(
                "dbo.MetaData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        description = c.String(),
                        createdDate = c.DateTime(nullable: false),
                        lastModifiedby = c.Int(nullable: false),
                        lastModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Folder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsRootFolder = c.Boolean(nullable: false),
                        Description = c.String(),
                        Name = c.String(),
                        CreatorId = c.Int(nullable: false),
                        IsFolder = c.Boolean(nullable: false),
                        ParentFolderId = c.Int(nullable: false),
                        PathOnServer = c.String(),
                        Folder_Id = c.Int(),
                        Metadata_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Folder", t => t.Folder_Id)
                .ForeignKey("dbo.MetaData", t => t.Metadata_Id)
                .Index(t => t.Folder_Id)
                .Index(t => t.Metadata_Id);
            
            CreateTable(
                "dbo.Document",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VersionNumber = c.Int(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        LockedByUserId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        DocumentType = c.Int(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Name = c.String(),
                        CreatorId = c.Int(nullable: false),
                        IsFolder = c.Boolean(nullable: false),
                        ParentFolderId = c.Int(nullable: false),
                        PathOnServer = c.String(),
                        Metadata_Id = c.Int(),
                        Folder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MetaData", t => t.Metadata_Id)
                .ForeignKey("dbo.Folder", t => t.Folder_Id)
                .Index(t => t.Metadata_Id)
                .Index(t => t.Folder_Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        createdDate = c.DateTime(nullable: false),
                        Writtenby = c.Int(nullable: false),
                        Document_Id = c.Int(),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Document", t => t.Document_Id)
                .Index(t => t.Document_Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Comments", new[] { "Document_Id" });
            DropIndex("dbo.Document", new[] { "Folder_Id" });
            DropIndex("dbo.Document", new[] { "Metadata_Id" });
            DropIndex("dbo.Folder", new[] { "Metadata_Id" });
            DropIndex("dbo.Folder", new[] { "Folder_Id" });
            DropIndex("dbo.Group", new[] { "Metadata_Id" });
            DropIndex("dbo.UserProfile", new[] { "Group_GroupId" });
            DropForeignKey("dbo.Comments", "Document_Id", "dbo.Document");
            DropForeignKey("dbo.Document", "Folder_Id", "dbo.Folder");
            DropForeignKey("dbo.Document", "Metadata_Id", "dbo.MetaData");
            DropForeignKey("dbo.Folder", "Metadata_Id", "dbo.MetaData");
            DropForeignKey("dbo.Folder", "Folder_Id", "dbo.Folder");
            DropForeignKey("dbo.Group", "Metadata_Id", "dbo.MetaData");
            DropForeignKey("dbo.UserProfile", "Group_GroupId", "dbo.Group");
            DropTable("dbo.Comments");
            DropTable("dbo.Document");
            DropTable("dbo.Folder");
            DropTable("dbo.MetaData");
            DropTable("dbo.Group");
            DropTable("dbo.UserProfile");
        }
    }
}
