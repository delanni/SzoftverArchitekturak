using PocsKft.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace PocsKft.Models
{
    public class HomeControllerAssistant
    {
        private HomeController master;

        private Guid? _userId;

        private Guid UserId
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
            this._userId = UserId;
        }

        internal string HandleFileUpload(HttpPostedFileBase file, string path)
        {
            string targetFileName = file.FileName;
            if (!path.EndsWith("/"))
            {
                targetFileName = path.Split('/').LastOrDefault();
                path = path.Substring(0, path.LastIndexOf('/') + 1);
            }

            var virtualFileName = Guid.NewGuid().ToString();
            var parentFolderId = FileManager.Instance.GetFileByPath(path).Id;
            var uploadPath = master.Server.MapPath("~/App_Data/uploads");

            if (!PermissionManager.Instance.CanRead(UserId, parentFolderId)) throw new Exception("You have no right to create a file here");

            if (file != null && file.ContentLength > 0)
            {
                var originalFileName = Path.GetFileName(targetFileName);
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
                    LockedByUserId = Guid.Empty,
                    Name = originalFileName,
                    ParentFolderId = parentFolderId,
                    PathOnServer = path,
                    Status = Status.Active,
                    PreviousVersionFileId = -1,
                    VersionNumber = 1,
                    VirtualFileName = virtualFileName
                };
                FileManager.Instance.CreateFile(document);
                PermissionManager.Instance.GrantRightOnFile(UserId, document.Id, PermissionType.WRITE);
            }

            return path.Substring(0, path.LastIndexOf('/') + 1); ;
        }

        internal string HandleFileUpdate(string fileJSON, string path)
        {
            var fileToUpdate = FileManager.Instance.GetFileByPath(path);
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
            return path;
        }

        internal dynamic FetchFile(string path)
        {
            var uploadPath = master.Server.MapPath("~/App_Data/uploads");

            var document = FileManager.Instance.GetFileByPath(path);
            if (document == null)
                throw new Exception("There is no such document");
            if (!PermissionManager.Instance.CanRead(UserId, document.Id))
                throw new Exception("You have no rights to download the file");

            var virtualFileName = document.VirtualFileName;
            var fileName = document.Name;
            var fileStream = System.IO.File.OpenRead(Path.Combine(uploadPath, virtualFileName));
            return new
            {
                fileStream = fileStream,
                fileName = fileName
            };
        }

        internal IEnumerable<object> ListProjects()
        {
            List<File> projects = FileManager.Instance.GetProjects();

            foreach (File f in projects)
            {
                string right = PermissionManager.Instance.EvaluateRight(UserId, f.Id);
                if (right != null)
                {
                    yield return (new ClientProject
                    {
                        CreationDate = f.CreatedDate,
                        Name = f.Name,
                        OwnerName = UserManager.Instance.GetUserNameById(f.CreatorId),
                        Right = right,
                        MetaData = f.MetaData
                    }.toJSON());
                }
            }
        }

        internal IEnumerable<object> ListFilesIn(File file)
        {
            List<File> documents = FileManager.Instance.ListChildren(file.Id);
            if (documents != null)
            {
                string folderReadRight = PermissionManager.Instance.EvaluateRight(UserId, file.Id);
                if (folderReadRight != null)
                {
                    foreach (File f in documents)
                    {
                        string right = PermissionManager.Instance.EvaluateRight(UserId, f.Id);
                        var versions = FileManager.Instance.GetVersionsForFile(f.Id);
                        if (right != null)
                        {
                            yield return (new ClientFile
                            {
                                CreatedDate = f.CreatedDate,
                                IsFolder = f.IsFolder,
                                LastModifiedDate = f.LastModifiedDate,
                                Locked = f.Locked,
                                MetaData = f.MetaData,
                                Name = f.Name,
                                PathOnServer = f.PathOnServer,
                                Right = right,
                                UserHasLock = f.LockedByUserId == UserId,
                                VersionNumber = f.VersionNumber,
                                Versions = versions
                            }.toJSON());
                        }
                    }
                }
            }
        }

        internal bool DeleteDocument(string path)
        {
            var fileToDelete = FileManager.Instance.GetFileByPath(path);
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
            var document = FileManager.Instance.GetFileByPath(path);
            if (document == null) throw new Exception("Lockable file not found");
            if (PermissionManager.Instance.CanRead(UserId, document.Id) && !document.Locked)
            {
                LockManager.Instance.AcquireLockOnDocument(UserId, document.Id);
                return true;
            }
            else
            {
                throw new Exception("You have no rights to lock the file.");
            }
        }

        internal bool HandleFileUnlock(string path)
        {
            var document = FileManager.Instance.GetFileByPath(path);
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


        internal void CreateFolder(string folderName, File parent)
        {
            if (PermissionManager.Instance.CanRead(UserId, parent.Id))
            {
                File newFolder = new File
                {
                    Name = folderName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    IsRootFolder = false,
                    IsFolder = true,
                    CreatorId = UserId,
                    ParentFolderId = parent.Id,
                    PathOnServer = (parent.PathOnServer + parent.Name + "/")
                };

                int newFolderId = FileManager.Instance.CreateFile(newFolder);

                if (master.HttpContext.User.Identity.IsAuthenticated)
                {
                    PermissionManager.Instance.GrantRightOnFolder(UserId, newFolderId, PermissionType.WRITE);
                }
                else
                {
                    PermissionManager.Instance.GrantRightOnFolder(GroupManager.EVERYBODY_ID, newFolderId, PermissionType.WRITE);
                }
            }
            else
            {
                throw new Exception("No rights for creating folder here");
            }
        }

        internal void CreateProject(string folderName)
        {
            if (PermissionManager.Instance.CanRead(UserId, 0))
            {
                File newFolder = new File
                {
                    Name = folderName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    IsRootFolder = true,
                    IsFolder = true,
                    CreatorId = UserId,
                    ParentFolderId = 0,
                    PathOnServer = "/",
                    MetaData = "[]"
                };

                int newFolderId = FileManager.Instance.CreateFile(newFolder);
                PermissionManager.Instance.GrantRightOnFolder(UserId, newFolderId, PermissionType.WRITE);
            }
            else
            {
                throw new Exception("No rights for creating folder here");
            }
        }

        internal bool RevertFileTo(string path, int targetVersion)
        {
            List<File> filesToDelete = new List<File>();
            File file = FileManager.Instance.GetFileByPath(path);
            while (file != null && file.VersionNumber != targetVersion)
            {
                filesToDelete.Add(file);
                file = FileManager.Instance.GetFileById(file.PreviousVersionFileId);
            }
            if (file != null)
            {
                using (var ctx = new UsersContext())
                {
                    file.Status = Status.Active;
                    ctx.Entry<File>(file).State = System.Data.EntityState.Modified;

                    ctx.SaveChanges();
                }
                LockManager.Instance.AcquireLockOnDocument(UserId, file.Id);
                foreach (var f in filesToDelete)
                {
                    FileManager.Instance.DeleteFileById(f.Id);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal IEnumerable<object> HandleSearch(string pathTerm, string keyTerm, string valueTerm)
        {
            List<File> results = new List<File>();
            if (!String.IsNullOrWhiteSpace(pathTerm))
                results.AddRange(FileManager.Instance.SearchFilesByName(pathTerm));
            if (!String.IsNullOrWhiteSpace(keyTerm+valueTerm))
                results.AddRange(FileManager.Instance.SearchMeta(keyTerm, valueTerm));
            var resultSet = results.Where(y=>PermissionManager.Instance.EvaluateRight(UserId, y.Id)!=null)
                .ToDictionary(x => x.Id);
            return resultSet.Select(x => new ClientFile(x.Value, UserId).toJSON());
        }
    }
}