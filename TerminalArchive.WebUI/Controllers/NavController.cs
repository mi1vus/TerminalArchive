using System.Collections.Generic;
using System.Web.Mvc;
using TerminalArchive.Domain.DB;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class NavController : Controller
    {
        [Authorize]
        public PartialViewResult Menu()
        {
            var admin = DbHelper.UserIsAdmin(User?.Identity?.Name);
            List<MenuInfo> menues = 
                new List<MenuInfo>
                {
                    new MenuInfo { Text = "Главная", Controller = "TerminalMonitoring", Action = "List" }
                };
            if (admin)
            {
                menues.Add(new MenuInfo {Text = "Новый терминал", Controller = "Terminal", Action = "Add"});
                menues.Add(new MenuInfo {Text = "Группы", Controller = "Terminal", Action = "ListGroups"});
                menues.Add(new MenuInfo {Text = "Новая группа", Controller = "Terminal", Action = "AddOrEditGroup"});
                menues.Add(new MenuInfo {Text = "Пользователи", Controller = "User", Action = "ListUser"});
                menues.Add(new MenuInfo {Text = "Новый пользователь", Controller = "User", Action = "AddOrEdit"});
                menues.Add(new MenuInfo {Text = "Роли пользователей", Controller = "User", Action = "UserRoles"});
                menues.Add(new MenuInfo {Text = "Роли", Controller = "User", Action = "ListRoles"});
                menues.Add(new MenuInfo {Text = "Новая роль пользователей",Controller = "User",Action = "AddOrEditRole"});
                menues.Add(new MenuInfo {Text = "Права ролей", Controller = "User", Action = "RoleRights"});
            }
            return PartialView(menues);
        }
    }
}