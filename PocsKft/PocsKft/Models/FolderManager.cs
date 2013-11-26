using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    //Singleton
    public class FolderManager
    {
        private static volatile FolderManager instance;
        private static object syncRoot = new Object();
        private FolderManager() { }
        //public List<Group> Groups { get; set; }

        public static FolderManager FolderManager
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new FolderManager();
                }
                return instance;
            }
        }

        public void DeleteFolder(Folder g)
        {
            using (UsersContext ct = new UsersContext())
            {
                ct.Folders.Remove(g);
                ct.SaveChanges();
            }
        }

        public bool DeleteFolderById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Folder g = ct.Folders.Where(i => i.FolderId == id).FirstOrDefault();
                if (g != null)
                {
                    ct.Folders.Remove(g);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool CreateGroup(Group g)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group temp = ct.Groups.Add(g);
                ct.SaveChanges();
                if (temp != null)
                    return true;
                else
                    return false;
            }
        }

        public bool DeleteUserFromGroup(Group g, UserProfile user)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    UserProfile u = g.Users.Where(i => i.UserId == user.UserId).FirstOrDefault();
                    g.Users.Remove(u);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool DeleteUserFromGroupById(int id, UserProfile user)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.Where(i => i.GroupId == id).FirstOrDefault();
                if (g != null)
                {
                    UserProfile u = g.Users.Where(i => i.UserId == user.UserId).FirstOrDefault();
                    g.Users.Remove(u);
                    ct.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        public bool AddUserToGroup(Group g, UserProfile user)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    UserProfile u = g.Users.Where(i => i.UserId == user.UserId).FirstOrDefault();
                    g.Users.Add(u);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool AddUserToGroupById(int id, UserProfile user)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.Where(i => i.GroupId == id).FirstOrDefault();
                if (g != null)
                {
                    UserProfile u = g.Users.Where(i => i.UserId == user.UserId).FirstOrDefault();
                    g.Users.Add(u);
                    ct.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }
    }
}