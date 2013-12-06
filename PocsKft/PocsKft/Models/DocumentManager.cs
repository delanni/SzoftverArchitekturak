using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace PocsKft.Models
{
    //Singleton
    public class DocumentManager
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
                doc.MetaData = d.MetaData;
                ct.Documents.Add(doc);
                ct.SaveChanges();
            }
        }

        public bool AddDocument(Document document)
        {
            // TODO: miért van néhány property beállítva rajta, és néhány nem?
            using (UsersContext context = new UsersContext())

                if (context.Documents.Any(x => x.ParentFolderId == document.ParentFolderId
                   && x.Name == document.Name))
                {
                    return false;
                }
                else
                {
                    document.MetaData = "[]";
                    document.VersionNumber = 1;
                    document.PreviousVersionDocumentId = -1;
                    context.Documents.Add(document);
                    context.SaveChanges();

                    return true;
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

        public Document GetDocumentByPath(string path)
        {
            var folderPath = path.Substring(0, path.LastIndexOf('/') + 1);
            var fileName = path.Substring(path.LastIndexOf('/') + 1);
            int folderId = FolderManager.Instance.GetFolderByPath(folderPath).Id;
            using (UsersContext ct = new UsersContext())
            {
                var doc = ct.Documents.SingleOrDefault(x => x.ParentFolderId == folderId && x.Name == fileName);
                return doc;
            }
        }

        public void UpdateMeta(int documentId, string fileJson)
        {
            using (UsersContext ct = new UsersContext())
            {
                var fileToUpdate = ct.Documents.SingleOrDefault(x => x.Id == documentId);
                if (fileToUpdate == null) return;

                if (fileToUpdate.MetaData == null) fileToUpdate.MetaData = "{}";

                var remoteObj = Json.Decode(fileJson);
                var properties = remoteObj.properties;
                var propsString = Json.Encode(properties);
                if (!String.IsNullOrEmpty(propsString))
                {
                    fileToUpdate.MetaData = propsString;
                }

                ct.SaveChanges();
            }
        }

        public List<Document> SearchMeta(string jsonKey, string jsonValue)
        {
            using (UsersContext ct = new UsersContext())
            {
                List<Document> docments = new List<Document>();

                foreach (Document doc in ct.Documents)
                {
                    //dynamic data = Json.Decode(doc.MetaData);
                    JObject jObject = JObject.Parse(doc.MetaData);
                    JArray jArray = JArray.Parse(doc.MetaData);

                    if (String.IsNullOrEmpty(jsonKey) && String.IsNullOrEmpty(jsonValue)) return null;
                    else if (!String.IsNullOrEmpty(jsonKey) && !String.IsNullOrEmpty(jsonKey))
                    {
                        JToken jToken = jObject.GetValue(jsonKey);

                        if (jToken != null)
                        {
                            if (jToken.ToString().Equals(jsonValue))
                            {
                                docments.Add(doc);
                            }
                        }
                    }
                    else if (!String.IsNullOrEmpty(jsonKey)) //csak jsonKey
                    {
                        JToken jToken = jObject.GetValue(jsonKey);
                        if (jToken != null)
                            docments.Add(doc);
                    }
                    else //csak jsonValue
                    {
                        foreach( JToken jToken in jObject.Values() )
                        {
                            if (jToken != null)
                            {
                                if (jToken.ToString().Equals(jsonValue))
                                {
                                    docments.Add(doc);
                                }
                            }
                        }
                    }
                }
                return docments;
            }
        }


        public string GetMetadataFor(int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                var document = ct.Documents.SingleOrDefault(x => x.Id == documentId);
                if (document == null) return null;
                return document.MetaData;
            }
        }


    }

}