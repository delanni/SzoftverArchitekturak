using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class UserManager
    {
        private static volatile UserManager instance;
        private static object syncRoot = new Object();
        private UserManager() { }
        public static UserManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new UserManager();
                }
                return instance;
            }
        }

        public int GetUserIdByName(string name)
        {
            using (UsersContext ct = new UsersContext())
            {
                UserProfile user = 
                    ct.UserProfiles.Where(i => i.UserName.Equals(name) == true).FirstOrDefault();
                if (user != null)
                    return user.UserId;
                else
                    return -1;
            }
        }

        public UserProfile GetUserById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                UserProfile user =
                    ct.UserProfiles.Where(i => i.UserId == id).FirstOrDefault();
                return user;
            }
        }

        public string GetUserNameById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                UserProfile user =
                    ct.UserProfiles.Where(i => i.UserId == id).FirstOrDefault();
                if (user != null)
                    return user.UserName;
                else
                    return null;
            }
        }
    }
}