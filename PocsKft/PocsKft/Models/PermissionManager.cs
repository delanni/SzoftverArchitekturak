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
                    {
                        instance = new PermissionManager();
                    }

                }
                return instance;
            }
        }

        /// <summary>
        /// Van-e közvetlenül a User-re vagy valamely User-t tartalmazó csoportra bejegyzés a Permission
        /// tábléba.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool CanRead(Guid userId, int documentId)
        {
            if (documentId == 0) return true; // a root-ban mindenki tud projektet csinalni
            using (UsersContext ct = new UsersContext())
            {
                //magára a user-re
                if (ct.Permissions.Any(i => i.FileId == documentId
                    && i.UserOrGroupId == userId))
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
                            if (ct.Permissions.Any(i => i.FileId == documentId
                                && i.UserOrGroupId == g.Id))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public bool CanWrite(Guid userId, int documentId)
        {
            if (documentId == 0) return true; // a root-ban mindenki tud projektet csinalni
            using (UsersContext ct = new UsersContext())
            {
                //magára a user-re
                if (ct.Permissions.Any(i => i.Type == PermissionType.WRITE
                    && i.FileId == documentId
                    && i.UserOrGroupId == userId))
                {
                    return true;
                }
                //valamely, a user-t tartalmazó csoport-ra
                else
                {
                    IEnumerable<Group> list = GroupManager.Instance.GetGroupsOfUser(userId);
                    if (list != null)
                    {
                        foreach (Group g in list)
                        {
                            if (ct.Permissions.Any(i => i.Type == PermissionType.WRITE
                                && i.FileId == documentId
                                && i.UserOrGroupId == g.Id))
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
        /// <param name="fileId">Adott File vagy Folder Id-ja</param>
        /// <param name="permissionType">A permission tipusa</param>
        public void GrantRightOnFile(Guid userOrGroupId, int fileId, PermissionType permissionType)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (!ct.Permissions.Any(i => i.UserOrGroupId == userOrGroupId && i.FileId == fileId))
                {
                    ct.Permissions.Add(new Permission
                    {
                        FileId = fileId,
                        UserOrGroupId = userOrGroupId,
                        IsFolder = false,
                        Type = permissionType
                    });
                    ct.SaveChanges();
                }
            }
        }

        public void GrantRightOnFolder(Guid userOrGroupId, int folderId, PermissionType Type, bool isRecursive = true)
        {
            GrantRightOnFile(userOrGroupId, folderId, Type);
            using (UsersContext ct = new UsersContext())
            {
                GrantRightOnAllChildren(ct, folderId, userOrGroupId, Type, isRecursive);
                ct.SaveChanges();
            }
        }

        /// <summary>
        /// Minden tartalmazó File-re és Almappára beír egy rekordot
        /// a Permissions táblába.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="folderId"></param>
        /// <param name="userOrGroupId"></param>
        public void GrantRightOnAllChildren(UsersContext ct, int folderId, Guid userOrGroupId, PermissionType Type, bool isRecursive)
        {
            File f = FileManager.Instance.GetFileById(folderId);

            //erre a Folder-re van-e már bejegyzés. Ha nincs -> új rekord.
            if (ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                && i.FileId == folderId).FirstOrDefault() == null)
            {
                ct.Permissions.Add(new Permission
                {
                    FileId = f.Id,
                    UserOrGroupId = userOrGroupId,
                    IsFolder = true
                });
            }

            // Ha rekurzív
            if (isRecursive)
            {

                //Minden dokumentumra van-e már bejegyzés. Ha nincs -> új rekord.
                List<File> docs = ct.Files.Where(i => i.ParentFolderId == folderId).ToList();
                if (docs != null)
                {
                    foreach (File temp in docs)
                    {
                        if (ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                            && i.FileId == temp.Id) == null)
                        {
                            ct.Permissions.Add(new Permission
                            {
                                FileId = temp.Id,
                                UserOrGroupId = userOrGroupId,
                                IsFolder = false,
                                Type = Type
                            });
                        }
                    }
                }
                //Minden gyerek-mappára van-e már bejegyzés. Ha nincs -> új rekord.
                List<File> folders = ct.Files.Where(i => i.ParentFolderId == folderId).ToList();
                if (folders != null)
                {
                    foreach (File temp in folders)
                    {
                        GrantRightOnAllChildren(ct, temp.Id, userOrGroupId, Type, isRecursive);
                    }
                }
            }
        }

        public void RevokeRightOnDocument(Guid userOrGroupId, int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Permission p = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                    && i.Id == documentId).FirstOrDefault();

                if (p != null)
                {
                    ct.Permissions.Remove(p);
                    ct.SaveChanges();
                }
            }
        }

        public void RevokeRightOnFolder(Guid userOrGroupId, int folderId)
        {
            using (UsersContext ct = new UsersContext())
            {
                RevokeRightOnAllChildren(ct, folderId, userOrGroupId);

                ct.SaveChanges();
            }
        }

        public void RevokeRightOnAllChildren(UsersContext ct, int folderId, Guid userOrGroupId)
        {
            File f = FileManager.Instance.GetFileById(folderId);

            //erre a Folder-re a Permission törlése
            Permission p = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                && i.Id == folderId).FirstOrDefault();

            if (p != null)
            {
                ct.Permissions.Remove(p);
            }

            //Minden dokumentumra -> Permission törlése
            List<File> docs = ct.Files.Where(i => i.ParentFolderId == folderId).ToList();
            if (docs != null)
            {
                foreach (File temp in docs)
                {
                    Permission perm = ct.Permissions.Where(i => i.UserOrGroupId == userOrGroupId
                        && i.Id == temp.Id).FirstOrDefault();

                    if (perm != null)
                    {
                        ct.Permissions.Remove(perm);
                    }
                    RevokeRightOnAllChildren(ct, temp.Id, userOrGroupId);
                }
            }
        }

        internal string EvaluateRight(Guid UserId, int fileId)
        {
            if (CanWrite(UserId, fileId)) return "WRITE";
            else if (CanRead(UserId, fileId)) return "READ";
            else return null;
        }
    }
}