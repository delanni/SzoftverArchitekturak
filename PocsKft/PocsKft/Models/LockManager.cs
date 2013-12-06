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


        // Az adott dokumentumon a User helyezett-e el Lock-ot ?
        // igen -> true
        // nem -> false
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

        // Az adott dokumentumon valaki aki nem a User helyezett-e el Lock-ot ?
        // igen -> true
        // nem -> false
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

        // Az adott dokumentumon van-e Lock?
        // igen -> true
        // nem -> false
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


        //Elhelyez egy lock-ot a 
        public void AcquireLockOnDocument(int userId, int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (ct.Locks.Where(i => i.UserId == userId && i.FolderOrDocumentId == documentId)
                    .FirstOrDefault() == null)
                    
                {
                    ct.Locks.Add(new Lock
                    {
                        UserId = userId,
                        FolderOrDocumentId = documentId
                    });

                    Document d = DocumentManager.Instance.GetDocumentById(documentId);
                    d.Locked = true;
                    d.LockedByUserId = userId;
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