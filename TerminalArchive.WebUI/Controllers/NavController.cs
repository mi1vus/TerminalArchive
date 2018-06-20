using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class NavController : Controller
    {
        public PartialViewResult Menu()
        {
            IEnumerable<MenuInfo> menues = 
                new List<MenuInfo>
                {
                    new MenuInfo { Text = "Главная", Controller = "TerminalMonitoring", Action = "List" },
                    new MenuInfo{ Text = "Новый терминал", Controller = "Terminal", Action = "Add" },
                    new MenuInfo{ Text = "Пользователи", Controller = "User", Action = "List" },
                    new MenuInfo{ Text = "Новый пользователь", Controller = "User", Action = "AddOrEdit" },
                    new MenuInfo{ Text = "Роли пользователей", Controller = "User", Action = "UserRoles" }
                };
            return PartialView(menues);
        }
    }
}