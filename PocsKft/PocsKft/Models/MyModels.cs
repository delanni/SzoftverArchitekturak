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
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid CreatorId { get; set; }
        public int ParentFolderId { get; set; }
        public Guid LastModifiedbyId { get; set; } //userId
        public int PreviousVersionFileId { get; set; } // ha ő az első verzió, akkor -1
        public Guid LockedByUserId { get; set; }

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
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string GroupName { get; set; }
    }

    public class Comment
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Content { get; set; }
        public DateTime createdDate { get; set; }
        public Guid WrittenbyId { get; set; } //userId
    }


    public class Lock
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int FolderOrDocumentId { get; set; }
    }


    public class Permission
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserOrGroupId { get; set; }
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