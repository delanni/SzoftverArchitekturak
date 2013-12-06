using PocsKft.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PocsKft.Models
{
    public class HomeControllerAssistant
    {
        private HomeController master;

        private int? _userId;

        private int UserId
        {
            get
            {
                if (!_userId.HasValue)
                {
                    _userId = UserManager.Instance.GetUserIdByName(master.HttpContext.User.Identity.Name);
                }
                return _userId.Value;
            }
            set
            {
                _userId = value;
            }
        }

        public HomeControllerAssistant(HomeController parent)
        {
            this.master = parent;
        }

        internal void HandleFileUpload(HttpPostedFileBase file, string path)
        {
            var virtualFileName = Guid.NewGuid().ToString();
            var parentFolderId = FolderManager.Instance.GetFolderByPath(path).Id;
            var uploadPath = master.Server.MapPath("~/App_Data/uploads");

            if (!PermissionManager.Instance.HasRights(UserId, parentFolderId)) throw new Exception("You have no right to create a file here");

            if (file != null && file.ContentLength > 0)
            {
                var originalFileName = Path.GetFileName(file.FileName);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                file.SaveAs(Path.Combine(uploadPath, virtualFileName));

                Document document = new Document()
                {
                    CreatedDate = DateTime.Now,
                    CreatorId = UserId,
                    LastModifiedbyId = UserId,
                    LastModifiedDate = DateTime.Now,
                    Locked = false,
                    LockedByUserId = 0,
                    Name = originalFileName,
                    ParentFolderId = parentFolderId,
                    PathOnServer = path,
                    Status = Status.Active,
                    VersionNumber = 1,
                    VirtualFileName = virtualFileName
                };
                DocumentManager.Instance.AddDocument(document);
                PermissionManager.Instance.GrantRightOnDocument(UserId, document.Id, PermissionType.WRITE);
            }
        }

        internal void HandleFileUpdate(string fileJSON, string path)
        {
            var fileToUpdate = DocumentManager.Instance.GetDocumentByPath(path);
            if (fileToUpdate != null)
                if (PermissionManager.Instance.HasRights(UserId, fileToUpdate.Id))
                {
                    DocumentManager.Instance.UpdateMeta(fileToUpdate.Id, fileJSON);
                }
                else
                {
                    throw new Exception("You have no rights to modify the file");
                }
            else
            {
                var folderToUpdate = FolderManager.Instance.GetFolderByPath(path);
                if (folderToUpdate != null)
                {
                    if (PermissionManager.Instance.HasRights(UserId, folderToUpdate.Id))
                    {
                        FolderManager.Instance.UpdateMeta(folderToUpdate.Id, fileJSON);
                    }
                    else
                    {
                        throw new Exception("You have no rights to modify the file");
                    }
                }
            }
        }

        internal dynamic FetchFile(string path)
        {
            var uploadPath = master.Server.MapPath("~/App_Data/uploads");

            var document = DocumentManager.Instance.GetDocumentByPath(path);
            if (document == null)
                throw new Exception("There is no such document");
            if (!PermissionManager.Instance.HasRights(UserId, document.Id))
                throw new Exception("You have no rights to download the file");

            var virtualFileName = document.VirtualFileName;
            var fileName = document.Name;
            var fileStream = System.IO.File.OpenRead(Path.Combine(uploadPath, virtualFileName));
            return new {
                fileStream= fileStream, fileName= fileName
            };
        }

        internal IEnumerable<object> ListProjects()
        {
            List<Folder> projects = FolderManager.Instance.GetProjects();

            foreach (Folder f in projects)
            {
                if (PermissionManager.Instance.HasRights(UserId, f.Id))
                {
                    yield return (new ClientProject
                    {
                        CreationDate = f.CreatedDate,
                        Name = f.Name,
                        OwnerName = UserManager.Instance.GetUserNameById(UserId),
                        Right = "WRITE"
                    }.toJSON());
                }
            }
        }

        internal IEnumerable<object> ListFoldersIn(Folder folder)
        {
            List<Folder> children = FolderManager.Instance.ListChildrenFolders(folder.Id);
            if (children != null)
            {
                foreach (Folder f in children)
                {
                    if (PermissionManager.Instance.HasRights(UserId, f.Id))
                    {
                        yield return (new ClientFile
                        {
                            CreatorId = f.CreatorId,
                            Id = f.Id,
                            IsFolder = true,
                            Name = f.Name,
                            ParentFolderId = f.ParentFolderId,
                            CreatedDate = f.CreatedDate,
                            LastModifiedDate = f.LastModifiedDate,
                            UserHasLock = false,
                            PathOnServer = f.PathOnServer,
                            MetaData = f.MetaData
                        }.toJSON());
                    }
                }
            }
        }

        internal IEnumerable<object> ListFilesIn(Folder folder)
        {
            List<Document> documents = FolderManager.Instance.ListDocumentsInFolder(folder.Id);
            if (documents != null)
            {
                foreach (Document f in documents)
                {
                    if (PermissionManager.Instance.HasRights(UserId, f.Id))
                    {
                        yield return (new ClientFile
                        {
                            CreatorId = f.CreatorId,
                            //Description = f.,
                            Id = f.Id,
                            IsFolder = false,
                            Locked = f.Locked,
                            LockedByUser = f.LockedByUserId,
                            Name = f.Name,
                            ParentFolderId = f.ParentFolderId,
                            VersionNumber = f.VersionNumber,
                            UserHasLock = f.LockedByUserId == UserId,
                            PathOnServer = f.PathOnServer,
                            MetaData = f.MetaData
                        }.toJSON());
                    }
                }
            }
        }

        internal bool DeleteDocument(string path)
        {
            var fileToDelete = DocumentManager.Instance.GetDocumentByPath(path);
            if (fileToDelete != null
                && PermissionManager.Instance.HasRights(UserId, fileToDelete.Id)
                && DocumentManager.Instance.DeleteDocumentById(fileToDelete.Id))
            {
                return true;
            }
            else
            {
                throw new Exception("You have no right to delete that document");
            }
        }

        internal bool DeleteFolder(string path)
        {
            Folder folderToDelete = FolderManager.Instance.GetFolderByPath(path);
            if (folderToDelete != null
                && PermissionManager.Instance.HasRights(UserId, folderToDelete.Id)
                && FolderManager.Instance.DeleteFolderById(folderToDelete.Id))
            {
                return true;
            }
            else
            {
                throw new Exception("You have no right to delete that folder");
            }
        }

        internal bool HandleFileLock(string path)
        {
            var document = DocumentManager.Instance.GetDocumentByPath(path);
            if (document == null) throw new Exception("Lockable file not found");
            if (PermissionManager.Instance.HasRights(UserId, document.Id) && !document.Locked){
                LockManager.Instance.AcquireLockOnDocument(UserId, document.Id);
                return true;
            } else {
                throw new Exception("You have no rights to lock the file.");
            }
        }

        internal bool HandleFileUnlock(string path)
        {
            var document = DocumentManager.Instance.GetDocumentByPath(path);
            if (document == null) throw new Exception("Unlockable file not found");
            if (PermissionManager.Instance.HasRights(UserId, document.Id) && document.Locked)
            {
                LockManager.Instance.ReleaseLockOnDocument(UserId, document.Id);
                return true;
            }
            else
            {
                throw new Exception("You have no rights to lock the file.");
            }
        }
    }
}