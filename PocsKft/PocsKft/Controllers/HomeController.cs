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

        private HomeControllerAssistant _assistant;
        HomeControllerAssistant Assistant
        {
            get
            {
                if (_assistant == null)
                    _assistant = new HomeControllerAssistant(this);
                return _assistant;
            }
            set { this._assistant = value; }
        }

        public ActionResult Index(string path)
        {
            var type = Request.RequestType;
            try
            {
                switch (type)
                {
                    case "PUT":
                        return CreateFolder(path);
                    case "DELETE":
                        return DeleteResource(path);
                    case "GET":
                        var headerAccepts = Request.Headers["Accept"];
                        if (headerAccepts.ToLower().Contains("json"))
                        {
                            return List(path);
                        }
                        return View();
                    default:
                        return View();
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(Exception)))
                {
                    if (ex.Message.Contains("right")) Response.StatusCode = 503;
                    else Response.StatusCode = 500;
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = 500;
                    return Json("An error ocurred, try again", JsonRequestBehavior.AllowGet);
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
                dynamic fileInfo = Assistant.FetchFile(path);
                return File(fileInfo.fileStream, "application/octet-stream", fileInfo.fileName);
            }
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string path)
        {
            // case "POST":
            if (file == null)
            {
                if (Request.Form["unlock"] != null)
                {
                    Assistant.HandleFileUnlock(path);
                }
                else if (Request.Form["lock"] != null)
                {
                    Assistant.HandleFileLock(path);
                }
                else
                {
                    try
                    {
                        var fileJson = Request.Form["data"];
                        Assistant.HandleFileUpdate(fileJson, path);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            else
            {
                Assistant.HandleFileUpload(file, path);
            }
            return RedirectToAction("Index", new { path = path });
        }

        [HttpGet]
        public JsonResult List(string path)
        {
            List<object> entitiesToList = new List<object>();

            if (String.IsNullOrEmpty(path))
            {
                var projects = Assistant.ListProjects();
                entitiesToList.AddRange(projects);
            }
            else
            {
                Folder folder = FolderManager.Instance.GetFolderByPath(path);

                var folders = Assistant.ListFoldersIn(folder);
                var files = Assistant.ListFilesIn(folder);

                entitiesToList.AddRange(folders);
                entitiesToList.AddRange(files);

            }
            return Json(entitiesToList, JsonRequestBehavior.AllowGet);

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
                return Json(Assistant.DeleteFolder(path) ? "Deletion successful" : "Deletion failed.");
            }
            else
            {
                return Json(Assistant.DeleteDocument(path) ? "Deletion successful" : "Deletion failed.");
            }
        }


        public ActionResult About()
        {
            return View();
        }
    }
}
