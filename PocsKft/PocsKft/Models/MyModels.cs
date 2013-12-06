using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class File
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public int ParentFolderId { get; set; }
        public int LastModifiedbyId { get; set; } //userId
        public int PreviousVersionFileId { get; set; } // ha ő az első verzió, akkor -1
        public int LockedByUserId { get; set; }

        public string Name { get; set; }
        public string VirtualFileName { get; set; }
        public string PathOnServer { get; set; }

        public bool IsFolder { get; set; }
        public bool IsRootFolder { get; set; }
        public int VersionNumber { get; set; }
        public Status Status { get; set; } // Archived or Active
        public bool Locked { get; set; }

        public string MetaData { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
    }

    public class GroupMembership
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
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
        public int FileId { get; set; }
        public bool IsFolder { get; set; }
        public PermissionType Type { get; set; }
    }

    public enum Status
    {
        Active = 1,
        Archive = 2
    }

    public enum PermissionType
    {
        WRITE = 1,
        READ = 2
    }
}