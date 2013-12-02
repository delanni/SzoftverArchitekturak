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
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string path)
        {
            var fileName = Guid.NewGuid().ToString();
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the fielname
                var oldFileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                if (!Directory.Exists(Server.MapPath("~/App_Data/uploads")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/App_Data/uploads"));
                }
                var newPath = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(newPath);
            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("Index", new { path = path });
        }

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
                                CreatorName = UserManager.Instance.GetUserNameById(f.CreatorId),
                                Description = f.Description,
                                Id = f.Id,
                                IsFolder = true,
                                // IsLocked = false,
                                // LockedByUserName = ,
                                Name = f.Name,
                                ParentFolderName = f.ParentFolderId,
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
                                CreatorName = UserManager.Instance.GetUserNameById(f.CreatorId),
                                //Description = f.,
                                Id = f.Id,
                                IsFolder = false,
                                IsLocked = f.Locked,
                                LockedByUserName = UserManager.Instance.GetUserNameById(f.LockedByUserId),
                                Name = f.Name,
                                ParentFolderName = f.ParentFolderId,
                                VersionNumber = f.VersionNumber,
                                UserHasLock = userId == f.LockedByUserId ? true : false,
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
                            Children = null,
                            Documents = null,
                            IsFolder = true,
                            CreatorId = userId,
                            ParentFolderId = f.Id
                        };

                        int newFolderId = FolderManager.Instance.CreateFolder(newFolder, f.Id);

                        PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId);
                    }
                }
                else
                {
                    Folder newFolder = new Folder
                    {
                        Name = folderName,
                        IsRootFolder = true,
                        Children = null,
                        Documents = null,
                        CreatorId = userId,
                        IsFolder = true,
                        ParentFolderId = -1
                    };

                    int newFolderId = FolderManager.Instance.CreateRootFolder(newFolder);

                    PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId);
                }
            }

            ViewBag.Message = "Your contact page.";

            return Json(true);
        }

        [HttpDelete]
        public ActionResult DeleteFolder(string path)
        {
            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);
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
    }
}
