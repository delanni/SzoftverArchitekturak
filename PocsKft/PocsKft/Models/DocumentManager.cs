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
            public static string FILES_PATH = "D:\\var\\FILES";
            private DocumentManager() { }

            public static DocumentManager DocumentManagerInstance
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

            public Document GetDocumentById(int id)
            {
                using (UsersContext ct = new UsersContext())
                {
                    return ct.Documents.Where(i => i.Id == id).FirstOrDefault();
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
                Document g = GetDocumentById(id);
                using (UsersContext ct = new UsersContext())
                {
                    if (g != null)
                    {
                        ct.Documents.Remove(g);
                        ct.SaveChanges();
                        return true;
                    }
                }
                return false;
            }

            public void AddDocument(Document g)
            {
                using (UsersContext ct = new UsersContext())
                {
                    var siblings = ct.Documents.Where(x => x.ParentFolderId == g.ParentFolderId);
                    if (siblings.Any(w => w.Name == g.Name))
                    {

                    }
                    else
                    {
                        ct.Documents.Add(g);
                        ct.SaveChanges();
                    }
                }
            }

            public List<Document> SearchDocumentsByName(string name)
            {
                using (UsersContext ct = new UsersContext())
                {
                    var docs = ct.Documents.Where(i => i.Name.Contains(name));
                    return docs.ToList();
                }
            }

            public bool LockDocument(int id)
            {
                Document g = GetDocumentById(id);
                if (g.Locked == false)
                {
                    g.Locked = true;
                    return true;
                }
                else
                    return false;
            }

            public Document GetDocumentByPath(string path)
            {
                var folderPath = path.Substring(0, path.LastIndexOf('/'));
                var fileName = path.Substring(path.LastIndexOf('/'));
                int folderId = FolderManager.Instance.GetFolderByPath(folderPath).Id;
                using (UsersContext ct = new UsersContext())
                {
                    var doc = ct.Documents.SingleOrDefault(x => x.ParentFolderId == folderId && x.Name == fileName);
                    return doc;
                }
            }
        }
    }
}