using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class LockManager
    {
        private static volatile LockManager instance;
        private static object syncRoot = new Object();
        private LockManager() { }
        public static LockManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new LockManager();
                }
                return instance;
            }
        }


        public bool DoesUserHaveLockOnDocument(int userId, int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Lock l = ct.Locks.Where(i => i.UserId == userId && i.FolderOrDocumentId == documnetId)
            .FirstOrDefault();
                if (l != null)
                {
                    return true;
                }
                return false;
            }
        }

        public bool DoesAybodyButUserHaveLockOnDocument(int userId, int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Lock l = ct.Locks.Where(i => i.UserId != userId && i.FolderOrDocumentId == documnetId)
            .FirstOrDefault();
                if (l != null)
                {
                    return true;
                }
                return false;
            }
        }

        public bool DoesAybodyHaveLockOnDocument(int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Lock l = ct.Locks.Where(i => i.FolderOrDocumentId == documnetId)
            .FirstOrDefault();
                if (l != null)
                {
                    return true;
                }
                return false;
            }
        }

        public void AcquireLockOnDocument(int userId, int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (ct.Locks.Where(i => i.UserId == userId && i.FolderOrDocumentId == documnetId)
                    .FirstOrDefault() == null)
                {
                    ct.Locks.Add(new Lock
                    {
                        UserId = userId,
                        FolderOrDocumentId = documnetId
                    });

                    Document d = DocumentManager.Instance.GetDocumentById(userId);
                    d.Locked = true;
                    d.LockedByUserId = documnetId;
                    ct.Entry(d).State = EntityState.Modified;

                    ct.SaveChanges();
                }
            }
        }

        public void ReleaseLockOnDocument(int userId, int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Lock l = ct.Locks.Where(i => i.UserId == userId && i.FolderOrDocumentId == documnetId)
                            .FirstOrDefault();
                if (l != null)
                {
                    ct.Locks.Remove(l);

                    ct.SaveChanges();
                }
            }
        }
    }
}