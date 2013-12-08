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
    public class FileManager
    {
        private static volatile FileManager instance;
        private static object syncRoot = new Object();
        public static string FILES_PATH = "D:\\var\\FILES";
        private FileManager() { }

        public static FileManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new FileManager();
                }
                return instance;
            }
        }

        public File GetFileById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Files.Where(i => i.Id == id).FirstOrDefault();
            }
        }

        private bool DeleteFile(File file)
        {
            using (UsersContext ct = new UsersContext())
            {
                if (file.IsFolder)
                {
                    foreach (var kid in ct.Files.Where(x => x.ParentFolderId == file.Id))
                    {
                        DeleteFile(kid);
                    }
                }
                try
                {
                    ct.Entry<File>(file).State = EntityState.Deleted;
                    ct.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public bool DeleteFileById(int fileId)
        {
            File file = GetFileById(fileId);
            return DeleteFile(file);
        }

        public int AddNewVersionForFile(File file, int oldFileId)
        {
            File insertedFile;
            using (UsersContext ct = new UsersContext())
            {
                File oldFile = GetFileById(oldFileId);
                oldFile.Status = Status.Archive;
                ct.Entry(oldFile).State = EntityState.Modified;

                file.CreatedDate = oldFile.CreatedDate;
                file.VersionNumber = oldFile.VersionNumber + 1;
                file.PreviousVersionFileId = oldFile.Id;
                file.Status = Status.Active;
                file.MetaData = oldFile.MetaData;
                insertedFile = ct.Files.Add(file);
                ct.SaveChanges();

            }
            LockManager.Instance.ReleaseLockOnDocument(file.CreatorId, oldFileId);
            LockManager.Instance.AcquireLockOnDocument(file.CreatorId, insertedFile.Id);
            return insertedFile.Id;
        }

        public int CreateFile(File file)
        {
            // TODO: miért van néhány property beállítva rajta, és néhány nem?
            using (UsersContext context = new UsersContext())
            {
                var evilTwin = context.Files.SingleOrDefault(x =>
                    x.IsFolder == file.IsFolder // the same type
                    && x.ParentFolderId == file.ParentFolderId  // in the same folder
                    && x.Name == file.Name // going by the same name
                    && (x.IsFolder || x.Status == Status.Active) // if file, then the active one only
                    );
                if (evilTwin != null)
                {
                    if (!file.IsFolder)
                    {
                        if (LockManager.Instance.IsLockedByUser(file.CreatorId, evilTwin.Id))
                        {
                            return AddNewVersionForFile(file, evilTwin.Id);
                        }
                        else
                        {
                            throw new Exception("You must first acquire lock before updating.");
                        }
                    }
                    else { throw new Exception("The file already exists, try a different name."); }
                }
                else
                {
                    if (!file.PathOnServer.StartsWith("/")) file.PathOnServer = "/" + file.PathOnServer;
                    file.MetaData = "[]";
                    file.VersionNumber = 1;
                    file.PreviousVersionFileId = -1;
                    var returnedFile = context.Files.Add(file);
                    context.SaveChanges();

                    return returnedFile.Id;
                }
            }
        }

        public List<File> SearchFilesByName(string name)
        {
            using (UsersContext ct = new UsersContext())
            {
                var files = ct.Files.Where(i => i.Name.Contains(name));
                return files.ToList();
            }
        }

        public void UpdateMeta(int fileId, string fileJson)
        {
            using (UsersContext ct = new UsersContext())
            {
                var fileToUpdate = ct.Files.SingleOrDefault(x => x.Id == fileId);
                if (fileToUpdate == null) return;

                if (fileToUpdate.MetaData == null) fileToUpdate.MetaData = "[]";

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

        public List<File> SearchMeta(string jsonKey, string jsonValue)
        {
            using (UsersContext ct = new UsersContext())
            {
                List<File> files = new List<File>();


                foreach (File doc in ct.Files)
                {
                    JArray jArray = JArray.Parse(doc.MetaData);
                    for (var i = 0; i < jArray.Count; i++)
                    {
                        JToken propName = jArray[i]["propName"];
                        string propNameString = propName.ToString();
                        JToken propValue = jArray[i]["propValue"];
                        string propValueString = propValue.ToString();

                        if (propName.Contains(jsonKey) || propValue.Contains(jsonValue))
                        {
                            files.Add(doc);
                        }
                    }

                }
                return files;
            }
        }


        public string GetMetadataFor(int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                var document = ct.Files.SingleOrDefault(x => x.Id == documentId);
                if (document == null) return null;
                return document.MetaData;
            }
        }

        public List<File> ListChildren(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                return ct.Files.Where(i => i.ParentFolderId == id && (i.IsFolder || i.Status == Status.Active)).ToList();
            }
        }

        public List<File> ListArchivesFor(int id)
        {
            List<File> versions = new List<File>();
            var file = GetFileById(id);
            if (file == null) return versions;
            if (file.PreviousVersionFileId != -1)
            {
                versions.AddRange(ListArchivesFor(file.PreviousVersionFileId));
            }
            versions.Add(file);
            return versions;
        }

        public bool AddDescriptionToFile(int fileId, string description)
        {
            File file = GetFileById(fileId);
            using (UsersContext ct = new UsersContext())
            {
                if (file != null)
                {
                    file.Description = description;
                    ct.Entry<File>(file).State = EntityState.Modified;
                    ct.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public List<File> GetProjects()
        {
            List<File> list = null;
            using (UsersContext ct = new UsersContext())
            {
                list = ct.Files.Where(i => i.IsRootFolder == true).ToList();
            }
            return list;
        }

        public File GetFileByPath(string path)
        {
            if (!path.StartsWith("/")) path = "/" + path;
            using (UsersContext ct = new UsersContext())
            {
                var file = ct.Files.SingleOrDefault(x =>
                    (x.IsFolder && path == (x.PathOnServer + x.Name + "/")) ||
                    (!x.IsFolder && x.Status == Status.Active && path == (x.PathOnServer + x.Name)));
                if (file == null)
                {
                    file = ct.Files.SingleOrDefault(x =>
                    (x.IsFolder && path == (x.PathOnServer + x.Name + "/")) ||
                    (!x.IsFolder && x.Status == Status.Active && path == ("/" + x.PathOnServer + x.Name)));
                }
                return file;
            }
        }

    }

}