using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace PocsKft.Models
{
    public class ClientFile
    {
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public string PathOnServer { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string MetaData { get; set; }
        public int VersionNumber { get; set; }
        public bool Locked { get; set; }
        public string Right { get; set; }
        public bool UserHasLock { get; set; }

        public List<object> Versions { get; set; }

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
             * versions:[array{date:datestring, versionNumber:number}];
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
                properties = MetaData??"[]",
                versions = Versions
            };
        }
    }
}