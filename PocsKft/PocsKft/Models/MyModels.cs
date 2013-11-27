using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    [Table("CommonAncestor")]
    public abstract class CommonAncestor
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatorId { get; set; }
        public bool IsFolder { get; set; }
        public virtual Metadata Metadata { get; set; }
        public int ParentFolderId { get; set; }
        public string PathOnServer { get; set; }
    }

    [Table("Folder")]
    public class Folder : CommonAncestor
    {
        public virtual List<Folder> Children { get; set; }
        public virtual List<Document> Documents { get; set; }
        public bool IsRootFolder { get; set; }
        public string Description { get; set; }
    }

    [Table("Group")]
    public class Group
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public virtual Metadata Metadata { get; set; }
        public virtual List<UserProfile> Users { get; set; }
    }

    [Table("Document")]
    public class Document : CommonAncestor
    {
        public int VersionNumber { get; set; }
        public bool IsLocked { get; set; }
        public int LockedByUserId { get; set; }
        public Status Status { get; set; }
        public DocumentType DocumentType { get; set; }
        public virtual List<Comments> Comments { get; set; }
        public bool Locked { get; set; }
    }

    [Table("MetaData")]
    public class Metadata
    {
        public int Id { get; set; }
        public string description { get; set; }
        public DateTime createdDate { get; set; }
        public int lastModifiedby { get; set; } //userId
        public DateTime lastModifiedDate { get; set; }
    }

    [Table("Comments")]
    public class Comments
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime createdDate { get; set; }
        public int Writtenby { get; set; } //userId
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
}