using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace PocsKft.Models
{
    //Singleton
    public class GroupManager
    {
        public const string EVERYBODY = "Everybody";
        public static Guid EVERYBODY_ID = Guid.Empty;
        private static volatile GroupManager instance;
        private static object syncRoot = new Object();
        private GroupManager()
        {
            using (UsersContext context = new UsersContext())
            {
                if (!context.Groups.Any(x => x.GroupName == EVERYBODY))
                {
                    CreateGroup(new Group()
                    {
                        GroupName = EVERYBODY
                    });
                }
                EVERYBODY_ID = GetGroup(EVERYBODY).Id;
            }
        }

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

        public Group GetGroup(Guid id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.FirstOrDefault(i => i.Id == id);
                return g;
            }
        }
        public Group GetGroup(string groupName)
        {
            using (UsersContext ct = new UsersContext())
            {
                Group g = ct.Groups.FirstOrDefault(i => i.GroupName == groupName);
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
        public void DeleteGroup(Guid id)
        {
            Group g = GetGroup(id);
            DeleteGroup(g);

        }

        public Guid CreateGroup(Group g)
        {
            if (Roles.RoleExists(g.GroupName))
            {
                throw new Exception("Role " + g.GroupName + "  already exists");
            }
            else
            {
                Roles.CreateRole(g.GroupName);
                using (UsersContext ct = new UsersContext())
                {
                    Group temp = ct.Groups.Add(g);
                    ct.SaveChanges();
                    return temp.Id;
                }
            }
        }

        public void DeleteUserFromGroup(Group g, string user)
        {
            Roles.RemoveUserFromRole(user, g.GroupName);

        }
        public void DeleteUserFromGroup(Guid id, string user)
        {
            Group g = GetGroup(id);
            Roles.RemoveUserFromRole(user, g.GroupName);
        }

        public void AddUserToGroup(Group g, string user)
        {
            Roles.AddUserToRole(user, g.GroupName);
        }
        public void AddUserToGroup(Guid id, string user)
        {
            Group g = GetGroup(id);

            AddUserToGroup(g, user);
        }

        public List<Group> GetGroupsOfUser(Guid userId)
        {
            var userName = UserManager.Instance.GetUserNameById(userId);
            List<Group> groups = new List<Group>();
            groups.Add(GetGroup(GroupManager.EVERYBODY));
            try
            {
                var roles = Roles.Provider.GetRolesForUser(userName);
                groups.AddRange(roles.Select(x => GetGroup(x)));
                return groups;
            }
            catch (InvalidOperationException exception)
            {
                return groups;
            }
        }

        public bool IsUserInGroup(Guid userId, Guid groupId)
        {
            string userName = UserManager.Instance.GetUserNameById(userId);
            string groupName = GetGroup(groupId).GroupName;

            return IsUserInGroup(userName, groupName);
        }
        public bool IsUserInGroup(string userName, string groupName)
        {
            return Roles.GetRolesForUser(userName).Contains(groupName);
        }
    

    }
}