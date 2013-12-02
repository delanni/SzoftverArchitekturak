using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public abstract class CommonAncestor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatorId { get; set; }
        public bool IsFolder { get; set; }
        public int MetadataId { get; set; }
        public int ParentFolderId { get; set; }
        public string PathOnServer { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedbyId { get; set; } //userId
        public DateTime LastModifiedDate { get; set; }
    }


    public class Folder : CommonAncestor
    {
        public bool IsRootFolder { get; set; }
        public string Description { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public int MetadataId { get; set; }
    }

    public class GroupMembership
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
    }


    public class Document : CommonAncestor
    {
        public int VersionNumber { get; set; }
        public int LockedByUserId { get; set; }
        public Status Status { get; set; } // Archived or Active
        public DocumentType DocumentType { get; set; }
        public bool Locked { get; set; }
    }


    public class Metadata
    {
        public int Id { get; set; }
        public string UserDefinedProperties { get; set; }
    }

public class Comment
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Content { get; set; }
        public DateTime createdDate { get; set; }
        public int WrittenbyId { get; set; } //userId
    }


    public class Lock
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FolderOrDocumentId { get; set; }
    }


    public class Permission
    {
        public int Id { get; set; }
        public int UserOrGroupId { get; set; }
        public int FolderOrDocumentId { get; set; } // Folder or Document Id
        public bool IsFolder { get; set; }
        public PermissionType Type { get; set; }
    }

    public enum Status
    {
        Active = 1,
        Archive = 2
    }

    public enum DocumentType
    {
        Excel = 1,
        Word = 2,
        PDF = 3,
        Other = 4
    }

    public enum PermissionType
    {
        WRITE = 1,
        READ = 2
    }
}