using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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

            public void AddNewVersionFromDocument(Document doc, int documentId)
            {
                using (UsersContext ct = new UsersContext())
                {
                    Document d = GetDocumentById(documentId);
                    d.Status = Status.Archive;
                    
                    doc.VersionNumber = d.VersionNumber + 1;
                    doc.PreviousVersionDocumentId = d.Id;

                    ct.Entry(d).State = EntityState.Modified;
                    doc.Status = Status.Active;
                    //Document temp = 
                    Metadata m = ct.Metadatas.Add(new Metadata
                    {
<<<<<<< HEAD
<<<<<<< HEAD
                        UserDefinedProperties = ""
                    });
                    doc.MetadataId = m.Id;
                    ct.Documents.Add(doc);
                    ct.SaveChanges();
                }
            }

            public bool AddDocument(Document g)
            {
                using (UsersContext ct = new UsersContext())
                {
                    var siblings = ct.Documents.Where(x => x.ParentFolderId == g.ParentFolderId
                        && x.Name.Equals(g.Name));

                    //ha van ugyanilyen nevű -> false
                    if (siblings != null)
                    {
                        return false;
=======
                        throw new Exception("The file already exists, try locking then updating the file");
>>>>>>> 531f6bc4fa0d130016176853ad1b051bdaa92ef0
=======
                        throw new Exception("The file already exists, try locking then updating the file");
>>>>>>> 531f6bc4fa0d130016176853ad1b051bdaa92ef0
                    }
                    else
                    {
                        Metadata m = ct.Metadatas.Add(new Metadata
                        {
                            UserDefinedProperties = ""
                        });
                        g.MetadataId = m.Id;
                        g.VersionNumber = 1;
                        g.PreviousVersionDocumentId = -1;
                        ct.Documents.Add(g);
                        ct.SaveChanges();
                        return true;
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