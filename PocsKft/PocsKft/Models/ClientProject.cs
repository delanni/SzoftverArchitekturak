using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class ClientProject
    {
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Right { get; set; }

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
                creationDate = CreationDate,
                ownerName = OwnerName,
                rights = Right
            };
        }
    }
}