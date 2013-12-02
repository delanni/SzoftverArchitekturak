using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class ClientFile
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string CreatorName { get; set; }
        public bool IsFolder { get; set; }
        public int ParentFolderName { get; set; }
        public string PathOnServer { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLocked { get; set; }
        public string LockedByUserName { get; set; }
        public Status Status { get; set; }

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
                filePath = PathOnServer.EndsWith("/")?PathOnServer:PathOnServer+"/",
                lockStatus = IsLocked?UserHasLock?"UNDERCONTROL":"LOCKED":"UNLOCKED",
                rights = Right,
                properties = new List<object>()
            };
        }

        public static ClientFile createExample()
        {
            var example = new ClientFile();
            example.IsFolder = false;
            example.IsLocked = true;
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
            example.IsLocked = true;
            example.UserHasLock = true;
            example.Name = "alma";
            example.PathOnServer = "/asd/das";
            example.Right = "WRITE";
            return example;
        }
    }
}