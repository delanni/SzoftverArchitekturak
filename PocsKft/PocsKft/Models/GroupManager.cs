using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    //Singleton
    public class GroupManager
    {
        private static volatile GroupManager instance;
        private static object syncRoot = new Object();
        private GroupManager() { }
        //public List<Group> Groups { get; set; }

        public static GroupManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new GroupManager();
                }
                return instance;
            }
        }

        public UserProfile GetUserById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.UserProfiles.Where(i => i.UserId == id).FirstOrDefault();
            }
        }

        public void DeleteGroup(Group g)
        {
            using (UsersContext ct = new UsersContext())
            {
                ct.Groups.Remove(g);
                ct.SaveChanges();
            }
        }

        public bool DeleteGroupById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.Where(i => i.GroupId == id).FirstOrDefault();
                if (g != null)
                {
                    ct.Groups.Remove(g);
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

        public List<Group> GetGroupsOfUser(int userId)
        {
            UserProfile user = GetUserById(userId);
            
            List<Group> list = new List<Group>();
            using (UsersContext ct = new UsersContext())
            {
                return ct.Groups.Where(i => i.Users.Contains(user) == true).ToList();
            }

        }
    }
}