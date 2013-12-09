using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PocsKft.Models;
using PocsKft.Filters;
using System.Web.Security;

namespace PocsKft.Controllers
{
    [InitializeSimpleMembership]
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
                        var revertCandidate = Request.QueryString["revertTo"];
                        if (revertCandidate == null)
                        {
                            return DeleteResource(path);
                        }
                        else
                        {
                            return RevertResource(path, revertCandidate);
                        }
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
                    if (ex.Message.ToLower().Contains("right")) Response.StatusCode = 503;
                    else Response.StatusCode = 500;
                    return Json(ex.Message.Substring(0, Math.Min(70, ex.Message.Length)), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = 500;
                    return Json(ex.Message.Substring(0, Math.Min(70, ex.Message.Length)), JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpDelete]
        private ActionResult RevertResource(string path, string revertCandidate)
        {
            var targetVersion = int.Parse(revertCandidate);
            var success = Assistant.RevertFileTo(path, targetVersion);
            return Json(success ? "Reverting was successful." : "Reverting failed");
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
                        path = Assistant.HandleFileUpdate(fileJson, path);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            else
            {
                path = Assistant.HandleFileUpload(file, path);
            }
            return RedirectToAction("Index", new { path = path });
        }

        [HttpGet]
        public JsonResult Search()
        {
            var pathTerm = Request.QueryString["searchInPath"];
            var keyTerm = Request.QueryString["searchInKey"];
            var valueTerm = Request.QueryString["searchInValue"];

            var result = Assistant.HandleSearch(pathTerm, keyTerm, valueTerm);

            return Json(result, JsonRequestBehavior.AllowGet);
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
                File file = FileManager.Instance.GetFileByPath(path);

                var files = Assistant.ListFilesIn(file);

                entitiesToList.AddRange(files);
            }
            return Json(entitiesToList, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Groups(string path)
        {
            try
            {
                if (Request.RequestType == "PUT" && GroupManager.Instance.GetGroup(path) == null)
                {

                    var result = Assistant.CreateGroup(path);

                    Request.RequestType = "GET";
                    return RedirectToAction("Groups", "Home", new { path = result });
                }
                else
                {
                    var groupVM = new GroupsViewModel();
                    groupVM.Group = GroupManager.Instance.GetGroup(path);
                    var users = UserManager.Instance.GetAllUsers();
                    groupVM.Members = new List<UserProfile>(users.Where(x => GroupManager.Instance.IsUserInGroup(x.UserId, groupVM.Group.Id)));
                    return View(groupVM);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Equals(typeof(Exception)))
                {
                    if (ex.Message.ToLower().Contains("right")) Response.StatusCode = 503;
                    else Response.StatusCode = 500;
                    return Json(ex.Message.Substring(0, Math.Min(70, ex.Message.Length)), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = 500;
                    return Json(ex.Message.Substring(0, Math.Min(70, ex.Message.Length)), JsonRequestBehavior.AllowGet);
                }
            }

            return View("Index");
        }


        [HttpPut]
        public ActionResult CreateFolder(string path)
        {
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
            var parentPath = path.Substring(0, path.LastIndexOf('/') + 1);
            var folderName = path.Substring(path.LastIndexOf('/') + 1);

            //Regex regex = new Regex(@"^[0-9A-Za-Z_-]{3,20}$");


            File parent = FileManager.Instance.GetFileByPath(parentPath);
            if (parent != null)
            {
                Assistant.CreateFolder(folderName, parent);
            }
            else
            {
                Assistant.CreateProject(folderName);
            }


            return Json(true);
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
