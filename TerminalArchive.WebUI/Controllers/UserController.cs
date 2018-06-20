using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.DB;
using TerminalArchive.Domain.Models;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class UserController : Controller
    {
        private readonly ITerminalRepository _repository;

        public UserController()
        {
            _repository = new TerminalRepository { UserName = User?.Identity?.Name };
        }

        public UserController(ITerminalRepository repo)
        {
            _repository = repo;
            _repository.UserName = User?.Identity?.Name;
        }

        [Authorize]
        public ActionResult List()
        {
            _repository.UserName = User?.Identity?.Name;
            DbHelper.UpdateAllUsers(_repository.UserName);

            return View(DbHelper.Users.Values);
        }

        // GET: User
        [Authorize]
        public ActionResult UserRoles()
        {
            _repository.UserName = User?.Identity?.Name;
            if (!DbHelper.UpdateAllUsers(_repository.UserName) ||
                !DbHelper.UpdateAllRoles(_repository.UserName))
                View(new UserRolesViewModel());

            var model = new UserRolesViewModel
            {
                Users = DbHelper.Users.Values,
                Roles = DbHelper.Roles.Values
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UserRoles(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            if (!DbHelper.UpdateAllUsers(_repository.UserName) ||
                !DbHelper.UpdateAllRoles(_repository.UserName))
                View("UserRoles");

            var model = new UserRolesViewModel
            {
                Users = DbHelper.Users.Values,
                Roles = DbHelper.Roles.Values
            };

            if (Request.Form["submitbutton"] != null && Request.Form["submitbutton"] == "Save")
            {
                var result = false;
                if (DbHelper.Users.Any() && DbHelper.Roles.Any())
                {
                    var new_checked = Request.Form.AllKeys;
                    List<UserRole> to_delete = new List<UserRole>();
                    List<UserRole> to_add = new List<UserRole>();
                    foreach (var user in DbHelper.Users.Values)
                    {
                        foreach (var role in DbHelper.Roles.Values)
                        {
                            bool in_new_checked = new_checked.Any(k => k == $"chk_{user.Id}_{role.Id}");
                            bool in_old_checked = user.Roles.Any(r => r.Id == role.Id);

                            if (in_new_checked && in_old_checked)
                                continue;
                            else if (in_new_checked && !in_old_checked)
                                to_add.Add(new UserRole
                                {
                                    IdUser = user.Id,
                                    IdRole = role.Id
                                });
                            else if (!in_new_checked && in_old_checked)
                                to_delete.Add(new UserRole
                                {
                                    IdUser = user.Id,
                                    IdRole = role.Id
                                });
                        }
                    }
                    result = DbHelper.UpdateUserRoles(to_add, to_delete, _repository.UserName);
                }
                if (result)
                {
                    if (!DbHelper.UpdateAllUsers(_repository.UserName) ||
                        !DbHelper.UpdateAllRoles(_repository.UserName))
                        View("UserRoles");

                    var modelNew = new UserRolesViewModel
                    {
                        Users = DbHelper.Users.Values,
                        Roles = DbHelper.Roles.Values
                    };
                    return View("UserRoles", modelNew);
                }
                else
                {
                    ModelState.AddModelError("Db", "Роли пользователей не был изменены! Повторите попытку или свяжитесь с администратором.");
                    return View("UserRoles", model);
                }
            }

            return View("UserRoles", model);
        }

        [Authorize]
        public ActionResult AddOrEdit(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            if (id != 0)
            {
                DbHelper.UpdateAllUsers(_repository.UserName);
                return View(DbHelper.Users[id]);
            }
            else
                return View(new User());
        }
        [Authorize]
        [HttpPost]
        public ActionResult AddOrEdit(int id = 0, User user = null)
        {
            _repository.UserName = User?.Identity?.Name;
            if (ModelState.IsValid && user != null)
            {
                var res = user.Id != 0
                    ? DbHelper.EditUser(user.Id, user.Name, user.OldPass, user.Pass, _repository.UserName)
                    : DbHelper.AddUser(user.Name, user.Pass, _repository.UserName);
                if (!res)
                {
                    ModelState.AddModelError("Db", "Пользователь не был добавлен! Повторите попытку или свяжитесь с администратором.");
                    return View(user);
                }
                return View("Saved");
            }
            else
            {
                return View(user);
            }
        }
    }
}