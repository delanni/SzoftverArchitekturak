using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PocsKft.Models;
using PocsKft.Models.PocsKft.Models;
using System.IO;

namespace PocsKft.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string path)
        {
            var type = Request.RequestType;
            if (type == "PUT")
            {
                return CreateFolder(path);
            }
            else if (type == "DELETE")
            {
                return DeleteFolder(path);
            }
            else
            {
                var headerAccepts = Request.Headers["Accept"];
                if (headerAccepts.ToLower().Contains("json"))
                {
                    var x = List(path);
                    return x;
                }
                return View();
            }
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string path)
        {
            var fileName = Guid.NewGuid().ToString();
            var parentFolderId = FolderManager.Instance.GetFolderByPath(path).Id;

            if (file != null && file.ContentLength > 0)
            {
                var oldFileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                if (!Directory.Exists(Server.MapPath("~/App_Data/uploads")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/uploads"));
                }
                var newPath = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(newPath);

                DocumentManager.DocumentManagerInstance.AddDocument(new Document()
                {
                    CreatedDate = DateTime.Now,
                    CreatorId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name),
                    IsFolder = false,
                    LastModifiedbyId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name),
                    LastModifiedDate = DateTime.Now,
                    Locked = false,
                    LockedByUserId = -1,
                    Name = oldFileName,
                    ParentFolderId = parentFolderId,
                    PathOnServer = path,
                    Status = Status.Active,
                    VersionNumber = 1,
                    VirtualFileName = fileName
                });
            }
            return RedirectToAction("Index", new { path = path });
        }

        public JsonResult List(string path)
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);

            if (String.IsNullOrEmpty(path))
            {
                List<Folder> projects = FolderManager.Instance.GetProjects();
                List<object> projectsWithPermission = new List<object>();
                foreach (Folder f in projects)
                {
                    if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
                    {
                        projectsWithPermission.Add(new ClientProject
                        {
                            //CreationDate = f.Metadata.createdDate,
                            Name = f.Name,
                            OwnerName = UserManager.Instance.GetUserNameById(f.CreatorId),
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
                        if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
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
                                PathOnServer = f.PathOnServer
                            }.toJSON());
                        }
                    }
                }
                if (documents != null)
                {
                    foreach (Document f in documents)
                    {
                        if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
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
                                PathOnServer = f.PathOnServer
                            }.toJSON());
                        }
                    }
                }

                return Json(documentsAndFoldersWithPermission, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpPut]
        public ActionResult CreateFolder(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);

            string[] folderNames = path.Split('/');

            string folderName = folderNames.Last();

            IEnumerable<string> remFolderNames = folderNames.Take(folderNames.Length - 1);

            //Regex regex = new Regex(@"^[0-9A-Za-Z_-]{3,20}$");

            if (true)//regex.IsMatch(folderName))
            {
                Folder f = FolderManager.Instance.GetFolderByPath(remFolderNames);

                if (f != null)
                {
                    if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
                    {
                        Folder newFolder = new Folder
                        {
                            Name = folderName,
                            CreatedDate = DateTime.Now,
                            LastModifiedDate = DateTime.Now,
                            IsFolder = true,
                            CreatorId = userId,
                            ParentFolderId = f.Id
                        };

                        int newFolderId = FolderManager.Instance.CreateFolder(newFolder);

                        PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId, PermissionType.WRITE);
                    }
                }
                else
                {
                    Folder newFolder = new Folder
                    {
                        Name = folderName,
                        IsRootFolder = true,
                        CreatedDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        CreatorId = userId,
                        IsFolder = true
                    };

                    int newFolderId = FolderManager.Instance.CreateFolder(newFolder);

                    PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId, PermissionType.WRITE);
                }
            }

            ViewBag.Message = "Your contact page.";

            return Json(true);
        }

        [HttpDelete]
        public ActionResult DeleteFolder(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
            if (path.EndsWith("/"))
            {
                int folderId = FolderManager.Instance.GetFolderByPath(path).Id;
                if (true /*has right*/)
                {
                    FolderManager.Instance.DeleteFolderById(folderId);
                    return Json(true);
                }
                else
                {
                    Response.StatusCode = 403;
                    return View("Error");
                }
            }
            else
            {
                var file = DocumentManager.DocumentManagerInstance.GetDocumentByPath(path);
                if (true /*has rights*/)
                {
                    if (DocumentManager.DocumentManagerInstance.DeleteDocumentById(file.Id))
                    {
                        return Json(true);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    Response.StatusCode = 403;
                    return View("Error");
                }
            }


        }
    }
}
