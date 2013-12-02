using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PocsKft.Models;

namespace PocsKft.Controllers
{
    public class HomeController : Controller
    {
        // private UserManager userManager;

        public JsonResult List(string path)
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            int userId = UserManager.Instance.GetUserIdByName(HttpContext.User.Identity.Name);

            path = path.Substring(1);

            if (String.IsNullOrEmpty(path))
            {
                List<Folder> projects = FolderManager.Instance.GetProjects();
                List<ClientProject> projectsWithPermission = new List<ClientProject>();
                foreach (Folder f in projects)
                {
                    if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
                    {
                        projectsWithPermission.Add(new ClientProject {  
                            CreationDate = f.Metadata.createdDate,
                            Name = f.Name,
                            OwnerName = UserManager.Instance.GetUserNameById(f.CreatorId),
                            Right = "WRITE"
                        });
                    }
                }

                return Json(projectsWithPermission);

            }
            else {

                Folder folder = FolderManager.Instance.GetFolderByPath(path);

                List<Folder> children = FolderManager.Instance.ListChildrenFolders(folder.Id);
                List<Document> documents = FolderManager.Instance.ListDocumentsInFolder(folder.Id);

                List<ClientFile> documentsAndFoldersWithPermission = new List<ClientFile>();

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
                        });
                    }
                }
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
                            LockedByUserName =  UserManager.Instance.GetUserNameById(f.LockedByUserId),
                            Name = f.Name,
                            ParentFolderName = f.ParentFolderId,                            
                            VersionNumber = f.VersionNumber,
                            UserHasLock = userId == f.LockedByUserId ? true : false,
                            PathOnServer = f.PathOnServer
                        });
                    }
                }

                return Json(documentsAndFoldersWithPermission);
            }
        }

        public ActionResult Index(string path)
        {
            var headerAccepts = Request.Headers["Accept"];
            if (headerAccepts.ToLower().Contains("json"))
            {
                return List(path);
            }
            return View();
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

            IEnumerable<string> remFolderNames = folderNames.Take(folderNames.Length - 1);

            Regex regex = new Regex("^([a-zA-Z]:)?(\\\\[^<>:\"/\\\\|?*]+)+\\\\?$");

            string folderName = remFolderNames.Last();

            if (regex.IsMatch(folderName))
            {
                remFolderNames = remFolderNames.Take(remFolderNames.Count() - 1);

                Folder f =  FolderManager.Instance.GetFolderByPath(remFolderNames);

                if (f != null)
                {
                    if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
                    {
                        Folder newFolder = new Folder
                        {
                            Name = folderName
                            
                        };

                        int newFolderId = FolderManager.Instance.CreateFolder(newFolder, f.Id);

                        PermissionManager.Instance.GrantRightOnFolder(userId, newFolderId);
                    }
                }
            }

            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
