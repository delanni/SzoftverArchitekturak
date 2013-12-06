using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace PocsKft.Models
{
    //Singleton
    public class FolderManager
    {
        private static volatile FolderManager instance;
        private static object syncRoot = new Object();
        private FolderManager() { }
        public static FolderManager Instance
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
                foreach (var kid in ct.Folders.Where(x=>x.ParentFolderId == g.Id)){
                    DeleteFolder(kid);
                }
                foreach (var kid in ct.Documents.Where(x=>x.ParentFolderId==g.Id)){
                    DocumentManager.Instance.DeleteDocumentById(kid.Id);
                }
                ct.Folders.Remove(g);
                ct.SaveChanges();
            }
        }

        public bool DeleteFolderById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                var folderToDelete = ct.Folders.SingleOrDefault(x => x.Id == id);
                if (folderToDelete != null)
                {
                    foreach (var childId in ct.Folders.Where(w => w.ParentFolderId == id).Select(y=>y.Id))
                    {
                        DeleteFolderById(childId);
                    }
                    ct.Folders.Remove(folderToDelete);
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public List<Folder> ListChildrenFolders(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Folders.Where(i => i.ParentFolderId == id).ToList();
            }
        }

        public List<Document> ListDocumentsInFolder(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Documents.Where(i => i.ParentFolderId == id 
                    && i.Status == Status.Active).ToList();
            }
        }

        public List<Folder> SearchFoldersByName(string name)
        {
            using (UsersContext ct = new UsersContext())
            {
                var folders = ct.Folders.Where(i => i.Name.Contains(name));
                return folders.ToList();
            }
        }

        public int CreateFolder(Folder f)
        {
            using (UsersContext ct = new UsersContext())
            {
                var siblings = ct.Folders.Where(x => x.ParentFolderId == f.ParentFolderId);
                if (siblings.Any(x=>x.Name == f.Name)){
                    throw new Exception("The folder already exists, try a different name.");
                }
                Folder temp = ct.Folders.Add(f);
                ct.SaveChanges();
                return temp.Id;
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

        public List<Folder> GetProjects()
        {
            List<Folder> list = null;
            using (UsersContext ct = new UsersContext())
            {
                list = ct.Folders.Where(i => i.IsRootFolder == true).ToList();
            }
            return list;
        }

        public Folder GetFolderByPath(string path)
        {
            if (!path.EndsWith("/")) path += "/";
            string[] folderNames = path.Split('/');

            IEnumerable<string> remFolderNames = folderNames.Take(folderNames.Length - 1);

            using (UsersContext ct = new UsersContext())
            {
                Folder f = ct.Folders.Where(i => i.IsRootFolder == true
                    && i.Name.Equals(remFolderNames.FirstOrDefault())).FirstOrDefault();

                if (f == null) return null;

                Folder temp = null;
                foreach (string s in remFolderNames.Skip(1))
                {
                    if (String.IsNullOrEmpty(s)) break;

                    temp = f;

                    f = ct.Folders.Where(i => i.IsRootFolder == false
                    && i.Name.Equals(s) && i.ParentFolderId == f.Id).FirstOrDefault();

                    if (f == null) return temp;
                }

                return f;
            }
        }

        public Folder GetFolderByPath(IEnumerable<string> path)
        {
            using (UsersContext ct = new UsersContext())
            {
                Folder f = ct.Folders.Where(i => i.IsRootFolder == true
                    && i.Name.Equals(path.FirstOrDefault())).FirstOrDefault();

                if (f == null) return null;

                foreach (string s in path.Skip(1))
                {
                    if (String.IsNullOrEmpty(s)) break;

                    f = ct.Folders.Where(i => i.IsRootFolder == false
                    && i.Name.Equals(s) && i.ParentFolderId == f.Id).FirstOrDefault();

                    if (f == null) return null;
                }

                return f;
            }
        }

        public void UpdateMeta(int folderId, string fileJson)
        {
            using (UsersContext ct = new UsersContext())
            {
                var folderToUpdate = ct.Folders.SingleOrDefault(x => x.Id == folderId);
                if (folderToUpdate == null) return;

                var remoteObj = Json.Decode(fileJson);
                var properties = remoteObj.properties;
                var propsString = Json.Encode(properties);
                if (!String.IsNullOrEmpty(propsString))
                {
                    folderToUpdate.MetaData = propsString;
                }
                ct.SaveChanges();
            }
        }

        public string GetMetadataFor(int folderId)
        {
            using (UsersContext ct = new UsersContext())
            {
                var folder = ct.Folders.SingleOrDefault(x => x.Id == folderId);
                if (folder == null) return null;
                var metaData = "[]";
                return metaData;
            }
        }
    }
}