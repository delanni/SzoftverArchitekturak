﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Helpers;

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
                //ha van ugyanilyen nevű -> false
                if (ct.Documents.Any(x => x.ParentFolderId == g.ParentFolderId
                    && x.Name.Equals(g.Name)))
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

        public Document GetDocumentByPath(string path)
        {
            var folderPath = path.Substring(0, path.LastIndexOf('/')+1);
            var fileName = path.Substring(path.LastIndexOf('/')+1);
            int folderId = FolderManager.Instance.GetFolderByPath(folderPath).Id;
            using (UsersContext ct = new UsersContext())
            {
                var doc = ct.Documents.SingleOrDefault(x => x.ParentFolderId == folderId && x.Name == fileName);
                return doc;
            }
        }

        public void UpdateMeta(int fileId, string fileJson)
        {
            using (UsersContext ct = new UsersContext())
            {
                var fileToUpdate = ct.Documents.SingleOrDefault(x => x.Id == fileId);
                if (fileToUpdate == null) return;

                var metaData = ct.Metadatas.SingleOrDefault(x => x.Id == fileToUpdate.MetadataId);
                if (metaData == null) // Because the default value for an integer is 0
                {
                    metaData = ct.Metadatas.Add(new Metadata()
                    {
                        UserDefinedProperties = "{}"
                    });
                    ct.SaveChanges();
                    fileToUpdate.MetadataId = metaData.Id;
                    ct.Entry(fileToUpdate).State = EntityState.Modified;
                }
                var remoteObj = Json.Decode(fileJson);
                var properties = remoteObj.properties;
                var propsString = Json.Encode(properties);
                if (!String.IsNullOrEmpty(propsString))
                {
                    metaData.UserDefinedProperties = propsString;
                    ct.Entry(metaData).State = EntityState.Modified;
                }

                ct.SaveChanges();
            }
        }

        public Metadata GetMetadataFor(int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                var document = ct.Documents.SingleOrDefault(x => x.Id == documentId);
                if (document == null) return null;
                var metaData = ct.Metadatas.SingleOrDefault(x => x.Id == document.MetadataId);
                return metaData;
            }
        }


    }

}