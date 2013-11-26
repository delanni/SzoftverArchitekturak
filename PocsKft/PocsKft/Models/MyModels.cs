using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    [Table("Folder")]
    public class Folder
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public HashSet<Folder> Children { get; set; }
        public HashSet<Document> Documents { get; set; }
        public int parentFolderId { get; set; }
        public bool rootFolder { get; set; }
        public Metadata Metadata { get; set; }
        public Folder() { }
    }

    [Table("Group")]
    public class Group
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public Metadata Metadata { get; set; }
        public List<UserProfile> Users { get; set; }
        public Group() { }
    }

    [Table("Document")]
    public class Document
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }
        public int DocumentName { get; set; }
        public int CreatorId { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLocked { get; set; }
        public int LockedByUserId { get; set; }
        public Status Status { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    [Table("MetaData")]
    public class Metadata
    {
        public string description { get; set; }
        public DateTime createdDate { get; set; }
        public int lastModifiedby { get; set; } //userId
        public DateTime lastModifiedDate { get; set; }
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