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
        public bool IsLockedByUser(Guid userId, int fileId)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Locks.Any(i => i.UserId == userId && i.FolderOrDocumentId == fileId);
            }
        }

        // Az adott dokumentumon valaki aki nem a User helyezett-e el Lock-ot ?
        // igen -> true
        // nem -> false
        public bool DoesAybodyButUserHaveLockOnDocument(Guid userId, int documnetId)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Locks.Any(i => i.UserId != userId && i.FolderOrDocumentId == documnetId);
            }
        }

        // Az adott dokumentumon van-e Lock?
        // igen -> true
        // nem -> false
        public bool IsFileLocked(int fileId)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Locks.Any(i => i.FolderOrDocumentId == fileId);
            }
        }


        //Elhelyez egy lock-ot a 
        public void AcquireLockOnDocument(Guid userId, int fileId)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (!ct.Locks.Any(i => i.UserId == userId && i.FolderOrDocumentId == fileId))
                {
                    ct.Locks.Add(new Lock
                    {
                        UserId = userId,
                        FolderOrDocumentId = fileId
                    });

                    File d = FileManager.Instance.GetFileById(fileId);
                    d.Locked = true;
                    d.LockedByUserId = userId;
                    ct.Entry(d).State = EntityState.Modified;

                    ct.SaveChanges();
                }
                else
                {
                    throw new Exception("The document is already locked");
                }
            }
        }

        public void ReleaseLockOnDocument(Guid userId, int fileId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Lock l = ct.Locks.SingleOrDefault(i => i.UserId == userId && i.FolderOrDocumentId == fileId);
                if (l != null)
                {
                    ct.Locks.Remove(l);

                    File d = FileManager.Instance.GetFileById(fileId);
                    d.Locked = false;
                    d.LockedByUserId = Guid.NewGuid();
                    ct.Entry(d).State = EntityState.Modified;

                    ct.SaveChanges();
                }
            }
        }
    }
}