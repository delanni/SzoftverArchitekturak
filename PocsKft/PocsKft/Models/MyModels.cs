using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class Folder
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public HashSet<Folder> children { get; set; }
        public int parentFolderId { get; set; }
        public bool rootFolder { get; set; }
        public Metadata Metadata { get; set; }
        public Folder() { }
    }

    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public Metadata Metadata { get; set; }
        public Group() { }
    }

    public class Metadata
    {
        public string description { get; set; }
        public DateTime createdDate { get; set; }
        public int lastModifiedby { get; set; } //userId
        public DateTime lastModifiedDate { get; set; }
    }

    //Singleton
    public class GroupManagerClass
    {
        private static volatile GroupManagerClass instance;
        private static object syncRoot = new Object();
        private GroupManagerClass() { }
        public HashSet<Group> Groups { get; set; }

        public static GroupManagerClass GroupManager
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null) 
                        instance = new GroupManagerClass();
                }
                return instance;
            }
        }

        public bool DeleteUserFromGroup(Group g)
        {
            return Groups.Remove(g);
        }

        public bool DeleteUserFromGroup(int id)
        {
            Group g = Groups.Where(i => i.GroupId == id).FirstOrDefault();
            if (g != null)
                return Groups.Remove(g);
            else
                return false;
        }

        public bool AddUserToGroup(Group g)
        {
            return Groups.Add(g);
        }
    }

}