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

        public Group GetGroupById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.Where(i => i.Id == id).FirstOrDefault();
                return g;
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
            Group g = GetGRoupById(id);
            using (UsersContext ct = new UsersContext())
            {
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
                GroupMembership gm = ct.GroupMemberships.Where(i => i.UserId == user.UserId
                    && i.GroupId == g.Id).FirstOrDefault();
                if (gm != null)
                {
                    ct.GroupMemberships.Remove(gm);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool DeleteUserFromGroupById(int id, UserProfile user)
        {
            Group g = GetGRoupById(id);

            using (UsersContext ct = new UsersContext())
            {
                GroupMembership gm = ct.GroupMemberships.Where(i => i.UserId == user.UserId
                    && i.GroupId == g.Id).FirstOrDefault();
                if (gm != null)
                {
                    ct.GroupMemberships.Remove(gm);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool AddUserToGroup(Group g, UserProfile user)
        {
            using (UsersContext ct = new UsersContext())
            {
                GroupMembership gm = ct.GroupMemberships.Where(i => i.UserId == user.UserId
    && i.GroupId == g.Id).FirstOrDefault();

                if (gm != null)
                {

                    ct.GroupMemberships.Add(new GroupMembership
                    {
                        GroupId = g.Id,
                        UserId = user.UserId
                    });
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool AddUserToGroupById(int id, UserProfile user)
        {
            Group g = GetGRoupById(id);

            using (UsersContext ct = new UsersContext())
            {
                GroupMembership gm = ct.GroupMemberships.Where(i => i.UserId == user.UserId
    && i.GroupId == g.Id).FirstOrDefault();

                if (gm != null)
                {

                    ct.GroupMemberships.Add(new GroupMembership
                    {
                        GroupId = g.Id,
                        UserId = user.UserId
                    });
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public List<Group> GetGroupsOfUser(int userId)
        {
            using (UsersContext ct = new UsersContext())
            {
                List<Group> groups = new List<Group>();
                List<int> ints =  ct.GroupMemberships.Where(j => j.UserId == userId).Select(i => i.GroupId).ToList();
                foreach(int i in ints)
                {
                    groups.Add( ct.Groups.Where(j => j.Id == i).FirstOrDefault() );
                }
                return groups;
            }
        }
    }
}