using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
<<<<<<< HEAD
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Helpers;

    namespace PocsKft.Models
=======
    //Singleton
    public class DocumentManager
>>>>>>> 80a34c0689574039764360e5da83f0b709524635
    {
        private static volatile DocumentManager instance;
        private static object syncRoot = new Object();
        public static string FILES_PATH = "D:\\var\\FILES";
        private DocumentManager() { }

        public static DocumentManager Instance
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
<<<<<<< HEAD
                var folderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                var fileName = path.Substring(path.LastIndexOf('/') + 1);
                int folderId = FolderManager.Instance.GetFolderByPath(folderPath).Id;
                using (UsersContext ct = new UsersContext())
                {
                    var doc = ct.Documents.SingleOrDefault(x => x.ParentFolderId == folderId && x.Name == fileName);
                    return doc;
                }
=======
                var doc = ct.Documents.SingleOrDefault(x => x.ParentFolderId == folderId && x.Name == fileName);
                return doc;
>>>>>>> 80a34c0689574039764360e5da83f0b709524635
            }

            public void UpdateMeta(int fileId, string fileJson)
            {
                using (UsersContext ct = new UsersContext())
                {
                    var fileToUpdate = ct.Documents.SingleOrDefault(x => x.Id == fileId);
                    if (fileToUpdate == null) return;

                    var metaData = ct.Metadatas.SingleOrDefault(x => x.Id == fileToUpdate.MetadataId);
                    if (metaData == null)
                    {
                        metaData = ct.Metadatas.Add(new Metadata()
                        {
                            UserDefinedProperties = "{}"
                        });
                        ct.SaveChanges();
                        fileToUpdate.MetadataId = metaData.Id;
                    }
                    var remoteObj = Json.Decode(fileJson);
                    var properties = remoteObj.properties;
                    var propsString = Json.Encode(properties);
                    if (!String.IsNullOrEmpty(propsString))
                    {
                        metaData.UserDefinedProperties = propsString;
                    }

                    ct.SaveChanges();
                }
            }
        }
    }
}