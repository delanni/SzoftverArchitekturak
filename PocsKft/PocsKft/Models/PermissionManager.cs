using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class PermissionManager
    {
        private static volatile PermissionManager instance;
        private static object syncRoot = new Object();
        private PermissionManager() { }
        public static PermissionManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new PermissionManager();
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

        /// <summary>
        /// Van-e közvetlenül a User-re vagy valamely User-t tartalmazó csoportra bejegyzés a Permission
        /// tábléba.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool DoesUserHavePermissionOnDocumentOrFolder(int userId, int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                //magára a user-re
                if (ct.Permissions.Where(i => i.CommonAncestorId == documentId
                    && i.UserOrGroupId == userId).FirstOrDefault() != null)
                {
                    return true;
                }
                //valamely, a user-t tartalmazó csoport-ra
                else
                {
                    List<Group> list = GroupManager.Instance.GetGroupsOfUser(userId);
                    if (list != null)
                    {
                        foreach (Group g in list)
                        {
                            if (ct.Permissions.Where(i => i.CommonAncestorId == documentId
                                && i.UserOrGroupId == g.GroupId).FirstOrDefault() != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Ha még nem volt rajta Right, akkor felveszi.
        /// </summary>
        /// <param name="userOrGroupId">Adott User vagy Group Id-ja</param>
        /// <param name="documentId">Adott Document vagy Folder Id-ja</param>
        public void GrantRightOnDocument(int userOrGroupId, int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                    && i.CommonAncestorId == documentId) == null)
                {
                    ct.Permissions.Add(new Permission
                    {
                        CommonAncestorId = documentId,
                        UserOrGroupId = userOrGroupId,
                        IsFolder = false
                    });
                    ct.SaveChanges();
                }
            }
        }

        public void GrantRightOnFolder(int userOrGroupId, int folderId)
        {
            using (UsersContext ct = new UsersContext())
            {
                GrantRightOnAllChildren(ct, folderId, userOrGroupId);

                ct.SaveChanges();
            }
        }

        /// <summary>
        /// Minden tartalmazó Document-re és Almappára beír egy rekordot
        /// a Permissions táblába.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="folderId"></param>
        /// <param name="userOrGroupId"></param>
        public void GrantRightOnAllChildren(UsersContext ct, int folderId, int userOrGroupId)
        {
            Folder f = FolderManager.Instance.GetFolderById(folderId);

            //erre a Folder-re van-e már bejegyzés. Ha nincs -> új rekord.
            if (ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                && i.CommonAncestorId == folderId).FirstOrDefault() == null)
            {
                ct.Permissions.Add(new Permission
                {
                    CommonAncestorId = folderId,
                    UserOrGroupId = userOrGroupId,
                    IsFolder = true
                });
            }

            //Minden dokumentumra van-e már bejegyzés. Ha nincs -> új rekord.
            if (f.Documents != null)
            {
                foreach (Document temp in f.Documents)
                {
                    if (ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                        && i.CommonAncestorId == temp.Id) == null)
                    {
                        ct.Permissions.Add(new Permission
                        {
                            CommonAncestorId = temp.Id,
                            UserOrGroupId = userOrGroupId,
                            IsFolder = false
                        });
                    }
                }
            }
            if (f.Children != null)
            {
                foreach (Folder temp in f.Children)
                {
                    GrantRightOnAllChildren(ct, temp.Id, userOrGroupId);
                }
            }
        }

        public void RevokeRightOnDocument(int userOrGroupId, int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Permission p = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                    && i.CommonAncestorId == documentId).FirstOrDefault();

                if(p != null){
                    ct.Permissions.Remove(p);
                    ct.SaveChanges();
                }
            }
        }

        public void RevokeRightOnFolder(int userOrGroupId, int folderId)
        {
            using (UsersContext ct = new UsersContext())
            {
                RevokeRightOnAllChildren(ct, folderId, userOrGroupId);

                ct.SaveChanges();
            }
        }

        public void RevokeRightOnAllChildren(UsersContext ct, int folderId, int userOrGroupId)
        {
            Folder f = FolderManager.Instance.GetFolderById(folderId);

            //erre a Folder-re a Permission törlése
            Permission p = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                && i.CommonAncestorId == folderId).FirstOrDefault();

            if (p != null)
            {
                ct.Permissions.Remove(p);
            }

            //Minden dokumentumra -> Permission törlése
            if (f.Documents != null)
            {
                foreach (Document temp in f.Documents)
                {
                    Permission perm = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                        && i.CommonAncestorId == temp.Id).FirstOrDefault();

                    if (perm != null)
                    {
                        ct.Permissions.Remove(perm);
                    }
                }
            }
            if (f.Children != null)
            {
                foreach (Folder temp in f.Children)
                {
                    GrantRightOnAllChildren(ct, temp.Id, userOrGroupId);
                }
            }
        }
    }
}