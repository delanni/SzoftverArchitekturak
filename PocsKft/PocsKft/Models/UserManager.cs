using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;

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

        public Guid GetUserIdByName(string name)
        {
            if (String.IsNullOrEmpty(name) || name == "unknown") return Guid.NewGuid();
            using (var context = new UsersContext())
            {
                var usr = context.UserProfiles.SingleOrDefault(x => x.UserName == name);
                return usr==null?Guid.NewGuid():usr.UserId;
            }
        }

        public string GetUserNameById(Guid id)
        {
            using (var context = new UsersContext())
            {
                var usr = context.UserProfiles.SingleOrDefault(x => x.UserId == id);
                return usr==null?"unknown":usr.UserName;
            }
            //try
            //{
            //    var user = Membership.GetUser((Guid)id);
            //    if (user == null) return "unknown";
            //    return user.UserName;
            //}
            //catch (Exception e)
            //{
            //    return "unknown";
            //}
        }
    }
}