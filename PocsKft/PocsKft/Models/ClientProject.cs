using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class ClientProject
    {
        public ClientProject(File f, Guid UserId)
        {
            this.Name = f.Name;
            this.Description = f.Description;
            this.OwnerName = UserManager.Instance.GetUserNameById(UserId);
            this.MetaData = f.MetaData;
            this.CreationDate = f.CreatedDate;
            this.Right = PermissionManager.Instance.EvaluateRight(UserId, f.Id);
        }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Right { get; set; }
        public string MetaData { get; set; }

        public object toJSON()
        {
            /**
             * projectName : string
             * creationDate: date
             * ownerName:string
             * rights:enum("WRITE","READ")
             **/
            return new
            {
                projectName = Name,
                description = Description,
                creationDate = CreationDate,
                ownerName = OwnerName,
                rights = Right,
                filePath = "/",
                properties = MetaData ?? "[]",
                isProject = true
            };
        }

        public string Description { get; set; }
    }
}