using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    namespace PocsKft.Models
    {
        //Singleton
        public class DocumentManager
        {
            private static volatile DocumentManager instance;
            private static object syncRoot = new Object();
            private DocumentManager() { }

            public static DocumentManager DocumentManager
            {
                get
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DocumentManager();
                    }
                    return instance;
                }
            }

            public void DeleteDocument(Document g)
            {
                using (UsersContext ct = new UsersContext())
                {
                    ct.Documents.Remove(g);
                    ct.SaveChanges();
                }
            }

            public bool DeleteDocumentById(int id)
            {
                using (UsersContext ct = new UsersContext())
                {
                    Document g = ct.Documents.Where(i => i.DocumentId == id).FirstOrDefault();
                    if (g != null)
                    {
                        ct.Documents.Remove(g);
                        ct.SaveChanges();
                        return true;
                    }
                }
                return false;
            }

            public bool CreateDocument(Group g)
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
}