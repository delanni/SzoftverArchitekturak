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
            var parentFolderId = FileManager.Instance.GetFileByPath(path).Id;
            var uploadPath = master.Server.MapPath("~/App_Data/uploads");

            if (!PermissionManager.Instance.CanRead(UserId, parentFolderId)) throw new Exception("You have no right to create a file here");

            if (file != null && file.ContentLength > 0)
            {
                var originalFileName = Path.GetFileName(file.FileName);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                file.SaveAs(Path.Combine(uploadPath, virtualFileName));

                File document = new File()
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
                FileManager.Instance.CreateFile(document);
                PermissionManager.Instance.GrantRightOnDocument(UserId, document.Id, PermissionType.WRITE);
            }
        }

        internal void HandleFileUpdate(string fileJSON, string path)
        {
            var fileToUpdate = FileManager.Instance.GetFilesByPath(path);
            if (fileToUpdate != null)
                if (PermissionManager.Instance.CanRead(UserId, fileToUpdate.Id))
                {
                    FileManager.Instance.UpdateMeta(fileToUpdate.Id, fileJSON);
                }
                else
                {
                    throw new Exception("You have no rights to modify the file");
                }
            else
            {
                var folderToUpdate = FileManager.Instance.GetFileByPath(path);
                if (folderToUpdate != null)
                {
                    if (PermissionManager.Instance.CanRead(UserId, folderToUpdate.Id))
                    {
                        FileManager.Instance.UpdateMeta(folderToUpdate.Id, fileJSON);
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

            var document = FileManager.Instance.GetFilesByPath(path);
            if (document == null)
                throw new Exception("There is no such document");
            if (!PermissionManager.Instance.CanRead(UserId, document.Id))
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
            List<File> projects = FileManager.Instance.GetProjects();

            foreach (File f in projects)
            {
                if (PermissionManager.Instance.CanRead(UserId, f.Id))
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

        internal IEnumerable<object> ListFilesIn(File folder)
        {
            List<File> documents = FileManager.Instance.ListChildren(folder.Id);
            if (documents != null)
            {
                foreach (File f in documents)
                {
                    if (PermissionManager.Instance.CanRead(UserId, f.Id))
                    {
                        yield return (new ClientFile
                        {
                            CreatorId = f.CreatorId,
                            //Description = f.,
                            Id = f.Id,
                            IsFolder = f.IsFolder,
                            CreatedDate = f.CreatedDate,
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
            var fileToDelete = FileManager.Instance.GetFilesByPath(path);
            if (fileToDelete != null
                && PermissionManager.Instance.CanRead(UserId, fileToDelete.Id)
                && FileManager.Instance.DeleteFileById(fileToDelete.Id))
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
            File folderToDelete = FileManager.Instance.GetFileByPath(path);
            if (folderToDelete != null
                && PermissionManager.Instance.CanRead(UserId, folderToDelete.Id)
                && FileManager.Instance.DeleteFileById(folderToDelete.Id))
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
            var document = FileManager.Instance.GetFilesByPath(path);
            if (document == null) throw new Exception("Lockable file not found");
            if (PermissionManager.Instance.CanRead(UserId, document.Id) && !document.Locked){
                LockManager.Instance.AcquireLockOnDocument(UserId, document.Id);
                return true;
            } else {
                throw new Exception("You have no rights to lock the file.");
            }
        }

        internal bool HandleFileUnlock(string path)
        {
            var document = FileManager.Instance.GetFilesByPath(path);
            if (document == null) throw new Exception("Unlockable file not found");
            if (PermissionManager.Instance.CanRead(UserId, document.Id) && document.Locked)
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