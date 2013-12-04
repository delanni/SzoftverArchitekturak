using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PocsKft.Models;
using System.IO;

namespace PocsKft.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string path)
        {
            var type = Request.RequestType;
            try
            {
                switch (type)
                {
                    case "PUT":
                        return CreateFolder(path);
                        break;
                    case "DELETE":
                        return DeleteResource(path);
                        break;
                    case "GET":
                        var headerAccepts = Request.Headers["Accept"];
                        if (headerAccepts.ToLower().Contains("json"))
                        {
                            return List(path);
                        }
                        return View();
                        break;
                    default:
                        return View();
                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(Exception)))
                {
                    if (ex.Message.Contains("right")) Response.StatusCode = 503;
                    else Response.StatusCode = 500;
                    return Json(ex.Message);
                }
                else
                {
                    Response.StatusCode = 500;
                    return Json("An error ocurred, try again");
                }
            }
        }

        [HttpGet]
        public FileResult Download(string path)
        {
            if (path.EndsWith("/"))
            {
                throw new Exception("Do not try to download folders");
            }
            else
            {
                var document = DocumentManager.Instance.GetDocumentByPath(path);
                if (document == null) throw new Exception("There is no such document");
                if (!PermissionManager.Instance.HasRights(getUserId(), document.Id)) throw new Exception("You have no rights to download the file");
                var virtualFileName = document.VirtualFileName;
                var fileName = document.Name;
                var fileStream = System.IO.File.OpenRead(Path.Combine(Server.MapPath("~/App_Data/uploads"), virtualFileName));
                return File(fileStream, "application/octet-stream", fileName);
            }
        }

        private int getUserId() { return UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name); }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string path)
        {
            // case "POST":
            if (file == null)
            {
                try
                {
                    var fileJson = Request.Form["data"];
                    handleFileUpdate(fileJson, path);
                }
                catch (Exception e)
                {
                }
            }
            else
            {
                handleFileUpload(file, path);
            }
            return RedirectToAction("Index", new { path = path });
        }

        private void handleFileUpload(HttpPostedFileBase file, string path)
        {
            var virtualFileName = Guid.NewGuid().ToString();
            var userId = getUserId();
            var parentFolderId = FolderManager.Instance.GetFolderByPath(path).Id;

            if (!PermissionManager.Instance.HasRights(userId, parentFolderId)) throw new Exception("You have no right to create a file here");

            if (file != null && file.ContentLength > 0)
            {
                var originalFileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                if (!Directory.Exists(Server.MapPath("~/App_Data/uploads")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/uploads"));
                }
                var newPath = Path.Combine(Server.MapPath("~/App_Data/uploads"), virtualFileName);
                file.SaveAs(newPath);

                Document document = new Document()
                {
                    CreatedDate = DateTime.Now,
                    CreatorId = userId,
                    IsFolder = false,
                    LastModifiedbyId = userId,
                    LastModifiedDate = DateTime.Now,
                    Locked = false,
                    LockedByUserId = -1,
                    Name = originalFileName,
                    ParentFolderId = parentFolderId,
                    PathOnServer = path,
                    Status = Status.Active,
                    VersionNumber = 1,
                    VirtualFileName = virtualFileName
                };
                DocumentManager.Instance.AddDocument(document);
            }
        }

        private void handleFileUpdate(string fileJSON, string path)
        {
            var fileToUpdate = DocumentManager.Instance.GetDocumentByPath(path);
            if (fileToUpdate != null)
                if (PermissionManager.Instance.HasRights(getUserId(), fileToUpdate.Id))
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
                    if (PermissionManager.Instance.HasRights(getUserId(), folderToUpdate.Id))
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

        public JsonResult List(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);

            if (String.IsNullOrEmpty(path))
            {
                List<Folder> projects = FolderManager.Instance.GetProjects();
                List<object> projectsWithPermission = new List<object>();

                foreach (Folder f in projects)
                {
                    if (PermissionManager.Instance.HasRights(userId, f.Id))
                    {
                        projectsWithPermission.Add(new ClientProject
                        {
                            CreationDate = f.CreatedDate,
                            Name = f.Name,
                            OwnerName = UserManager.Instance.GetUserNameById(userId),
                            Right = "WRITE"
                        }.toJSON());
                    }
                }

                return Json(projectsWithPermission, JsonRequestBehavior.AllowGet);

            }
            else
            {

                Folder folder = FolderManager.Instance.GetFolderByPath(path);

                List<Folder> children = FolderManager.Instance.ListChildrenFolders(folder.Id);
                List<Document> documents = FolderManager.Instance.ListDocumentsInFolder(folder.Id);

                List<object> documentsAndFoldersWithPermission = new List<object>();

                if (children != null)
                {
                    foreach (Folder f in children)
                    {
                        if (PermissionManager.Instance.HasRights(userId, f.Id))
                        {
                            documentsAndFoldersWithPermission.Add(new ClientFile
                            {
                                CreatorId = f.CreatorId,
                                Id = f.Id,
                                IsFolder = true,
                                // IsLocked = false,
                                // LockedByUserName = ,
                                Name = f.Name,
                                ParentFolderId = f.ParentFolderId,
                                //VersionNumber = ,
                                //UserHasLock = ,
                                PathOnServer = f.PathOnServer,
                                MetaData = FolderManager.Instance.GetMetadataFor(f.Id)
                            }.toJSON());
                        }
                    }
                }
                if (documents != null)
                {
                    foreach (Document f in documents)
                    {
                        if (PermissionManager.Instance.HasRights(userId, f.Id))
                        {
                            documentsAndFoldersWithPermission.Add(new ClientFile
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
                                UserHasLock = f.LockedByUserId == userId,
                                PathOnServer = f.PathOnServer,
                                MetaData = DocumentManager.Instance.GetMetadataFor(f.Id)
                            }.toJSON());
                        }
                    }
                }

                return Json(documentsAndFoldersWithPermission, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpPut]
        public ActionResult CreateFolder(string path)
        {
            IEnumerable<string> parentNames = path.Split('/');
            string folderName = parentNames.Last();
            parentNames = parentNames.Take(parentNames.Count() - 1);

            //Regex regex = new Regex(@"^[0-9A-Za-Z_-]{3,20}$");

            if (true)//regex.IsMatch(folderName))
            {
                Folder parent = FolderManager.Instance.GetFolderByPath(parentNames);
                if (parent != null)
                {
                    createFolder(folderName, parent);
                }
                else
                {
                    createProject(folderName);
                }
            }

            return Json(true);
        }

        private void createFolder(string folderName, Folder parent)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
            if (PermissionManager.Instance.HasRights(userId, parent.Id))
            {
                Folder newFolder = new Folder
                {
                    Name = folderName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    IsRootFolder = false,
                    IsFolder = true,
                    CreatorId = userId,
                    ParentFolderId = parent.Id,
                    PathOnServer = (parent.PathOnServer + parent.Name + "/")
                };

                int newFolderId = FolderManager.Instance.CreateFolder(newFolder);
                PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId, PermissionType.WRITE);
            }
            else
            {
                throw new Exception("No rights for creating folder here");
            }
        }

        private void createProject(string folderName)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
            if (PermissionManager.Instance.HasRights(userId, 0))
            {
                Folder newFolder = new Folder
                {
                    Name = folderName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    IsRootFolder = true,
                    IsFolder = false,
                    CreatorId = userId,
                    ParentFolderId = 0,
                    PathOnServer = "/"
                };

                int newFolderId = FolderManager.Instance.CreateFolder(newFolder);
                PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId, PermissionType.WRITE);
            }
            else
            {
                throw new Exception("No rights for creating folder here");
            }
        }

        [HttpDelete]
        public ActionResult DeleteResource(string path)
        {

            if (path.EndsWith("/"))
            {
                return deleteFolder(path);
            }
            else
            {
                return deleteDocument(path);
            }
        }

        private ActionResult deleteDocument(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
            var fileToDelete = DocumentManager.Instance.GetDocumentByPath(path);
            if (fileToDelete != null
                && PermissionManager.Instance.HasRights(userId, fileToDelete.Id)
                && DocumentManager.Instance.DeleteDocumentById(fileToDelete.Id))
            {
                return Json(true);
            }
            else
            {
                throw new Exception("You have no right to delete that document");
            }
        }

        private ActionResult deleteFolder(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
            Folder folderToDelete = FolderManager.Instance.GetFolderByPath(path);
            if (folderToDelete != null
                && PermissionManager.Instance.HasRights(userId, folderToDelete.Id)
                && FolderManager.Instance.DeleteFolderById(folderToDelete.Id))
            {
                return Json(true);
            }
            else
            {
                throw new Exception("You have no right to delete that folder");
            }
        }
    }
}
