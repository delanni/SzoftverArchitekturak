using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace PocsKft.Models
{
    public class ClientFile
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
        public Metadata MetaData { get; set; }

        //Doc
        public int VersionNumber { get; set; }
        public int LockedByUser { get; set; }
        public Status Status { get; set; } // Archived or Active
        public DocumentType DocumentType { get; set; }
        public bool Locked { get; set; }

        public string Right { get; set; }
        public bool UserHasLock { get; set; }

        public object toJSON()
        {
            /**
             * JSON FORMAT:
             * isRealFile:boolean,
             * fileName:string,
             * filePath:string,
             * creationDate:datetime, ?? 
             * lastModificationDate:datetime, ??
             * projectName:string, ???
             * lastModifierName:string, ??
             * lockStatus:enum("LOCKED","UNLOCKED","UNDERCONTROL")
             * rights:enum("WRITE","READ");
             * properties:[array{string:object}];
             * comments:[array{name:string,date:string,message:string}];
             **/

            return new
            {
                isRealFile = !IsFolder,
                fileName= Name,
                filePath = PathOnServer==null?"":(PathOnServer.EndsWith("/") ? PathOnServer : PathOnServer + "/"),
                creationDate = CreatedDate,
                lastModificationDate = LastModifiedDate,
                lockStatus = Locked?UserHasLock?"UNDERCONTROL":"LOCKED":"UNLOCKED",
                rights = Right,
                properties = MetaData==null?"[]":MetaData.UserDefinedProperties
            };
        }

        public static ClientFile createExample()
        {
            var example = new ClientFile();
            example.IsFolder = false;
            example.Locked = true;
            example.UserHasLock = true;
            example.Name = "this.txt";
            example.PathOnServer = "/asd/das";
            example.Right = "WRITE";
            return example;
        }

        public static ClientFile createExampleFolder()
        {
            var example = new ClientFile();
            example.IsFolder = true;
            example.Locked = true;
            example.UserHasLock = true;
            example.Name = "alma";
            example.PathOnServer = "/asd/das";
            example.Right = "WRITE";
            return example;
        }
    }
}