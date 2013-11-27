using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    //Singleton
    public class FolderManager
    {
        private static volatile FolderManager instance;
        private static object syncRoot = new Object();
        private FolderManager() { }

        public static FolderManager FolderManagerInstance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new FolderManager();
                }
                return instance;
            }
        }

        public Folder GetFolderById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Folders.Where(i => i.Id == id).FirstOrDefault();
            }
        }

        public void DeleteFolder(Folder g)
        {
            using (UsersContext ct = new UsersContext())
            {
                ct.Folders.Remove(g);
                ct.SaveChanges();
            }
        }

        public bool DeleteFolderById(int id)
        {
            Folder g = GetFolderById(id);
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    ct.Folders.Remove(g);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public List<CommonAncestor> ListFolderItems(int id)
        {
            Folder g = GetFolderById(id);
            List<CommonAncestor> list = null;
            using (UsersContext ct = new UsersContext())
            {
                list = new  List<CommonAncestor>();
                foreach (CommonAncestor c in g.Children)
                {
                    list.Add(c);
                }
                foreach (CommonAncestor c in g.Documents)
                {
                    list.Add(c);
                }
            }
            return list;
        }

        public List<Folder> SearchFoldersByName(string name)
        {
            using (UsersContext ct = new UsersContext())
            {
                var folders = ct.Folders.Where(i => i.Name.Contains(name));
                return  folders.ToList();
            }
        }

        public void CreateFolder(Folder f, int parentfolderId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Folder parent = ct.Folders.Where(i => i.Id == parentfolderId).FirstOrDefault();
                parent.Children.Add(f);
                ct.SaveChanges();
            }
        }

        public void EditFolder(int id)
        {
            Folder g = GetFolderById(id);
            using (UsersContext ct = new UsersContext())
            {
                ct.Entry(g).State = EntityState.Modified;
                ct.SaveChanges();
            }
        }

        public bool AddDocumentToFolder(int folderId, Document document)
        {
            Folder g = GetFolderById(folderId);
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    if (g.Documents == null)
                        g.Documents = new List<Document>();
                    g.Documents.Add(document);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool RemoveDocumentFromFolder(int folderId, Document document)
        {
            Folder g = GetFolderById(folderId);
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    if (g.Documents == null)
                        g.Documents = new List<Document>();
                    g.Documents.Remove(document);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool AddDescriptionToFolder(int folderId, string description)
        {
            Folder g = GetFolderById(folderId);
            using (UsersContext ct = new UsersContext())
            {
                if (g != null)
                {
                    g.Description = description;
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

    }
}