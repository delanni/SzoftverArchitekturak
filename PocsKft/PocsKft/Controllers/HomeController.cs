using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PocsKft.Models;

namespace PocsKft.Controllers
{
    public class HomeController : Controller
    {
       // private UserManager userManager;

        public JsonResult Index(string path)
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            int userId = UserManager.UserManagerInstance.GetUserIdByName( HttpContext.User.Identity.Name );

            path = path.Substring(1);

            if(String.IsNullOrEmpty(path))
            {
                // PermissionManager.PermissionManagerInstance.GetHashCode();
                List<Folder> projects = FolderManager.Instance.GetProjects();
                List<Folder> projectsWithPermission = new List<Folder>();
                foreach(Folder f in projects)
                {
                    if (PermissionManager.Instance.DoesUserHavePermissionOnDocumentOrFolder(userId, f.Id))
                    {
                        projectsWithPermission.Add(f);
                    }
                }

                return Json(projectsWithPermission);

            }
            else{}

            string[] folderNames = path.Split('/');

            //if(folder)

            return null;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
