using System.Collections.Generic;
using System.Linq;
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
        public ActionResult ListUser()
        {
            _repository.UserName = User?.Identity?.Name;
            var users = DbHelper.GetAllUsers(_repository.UserName);

            return View(users.Values);
        }

        // GET: User
        [Authorize]
        public ActionResult UserRoles()
        {
            _repository.UserName = User?.Identity?.Name;
            var users = DbHelper.GetAllUsers(_repository.UserName);
            var roles = DbHelper.GetAllRoles(_repository.UserName);
            if (users == null || roles == null)
                return View(new UserRolesViewModel());

            var model = new UserRolesViewModel
            {
                Users = users.Values,
                Roles = roles.Values
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UserRoles(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            var users = DbHelper.GetAllUsers(_repository.UserName);
            var roles = DbHelper.GetAllRoles(_repository.UserName);
            if (users == null || roles == null)
                return View(new UserRolesViewModel());

            var model = new UserRolesViewModel
            {
                Users = users.Values,
                Roles = roles.Values
            };


            //if (Request.Form["submitbutton"] == null || Request.Form["submitbutton"] != "Сохранить")
            //    return View("UserRoles", model);

            var result = false;
            if (users.Any() && roles.Any())
            {
                var newChecked = Request.Form.AllKeys;
                var toDelete = new List<UserRole>();
                var toAdd = new List<UserRole>();
                foreach (var user in users.Values)
                {
                    foreach (var role in roles.Values)
                    {
                        var inNewChecked = newChecked.Any(k => k == $"chk_{user.Id}_{role.Id}");
                        var inOldChecked = user.Roles.Any(r => r.Id == role.Id);

                        if (inNewChecked && inOldChecked)
                            continue;
                        if (inNewChecked)
                            toAdd.Add(new UserRole
                            {
                                IdUser = user.Id,
                                IdRole = role.Id
                            });
                        else if (inOldChecked)
                            toDelete.Add(new UserRole
                            {
                                IdUser = user.Id,
                                IdRole = role.Id
                            });
                    }
                }
                result = DbHelper.UpdateUserRoles(toAdd, toDelete, _repository.UserName);
            }
            if (result)
            {
                users = DbHelper.GetAllUsers(_repository.UserName);
                roles = DbHelper.GetAllRoles(_repository.UserName);
                if (users == null || roles == null)
                    return View(new UserRolesViewModel());

                var modelNew = new UserRolesViewModel
                {
                    Users = users.Values,
                    Roles = roles.Values
                };

                return View("UserRoles", modelNew);
            }
            else
            {
                ModelState.AddModelError("Db", "Роли пользователей не был изменены! Повторите попытку или свяжитесь с администратором.");
                return View("UserRoles", model);
            }
        }

        [Authorize]
        public ActionResult AddOrEdit(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            if (id != 0)
            {
                var users = DbHelper.GetAllUsers(_repository.UserName);
                return View(users[id]);
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

        [Authorize]
        public ActionResult ListRoles()
        {
            _repository.UserName = User?.Identity?.Name;
            var roles = DbHelper.GetAllRoles(_repository.UserName);

            return View(roles.Values);
        }
        [Authorize]
        public ActionResult AddOrEditRole(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll);
            ViewBag.Groups = groups;

            if (id != 0)
            {
                var roles = DbHelper.GetAllRoles(_repository.UserName);
                return View(roles[id]);
            }
            else
                return View(new Role());
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddOrEditRole(int id = 0, Role role = null)
        {
            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll);
            ViewBag.Groups = groups;

            if (ModelState.IsValid && role != null)
            {
                var res = role.Id != 0
                    ? DbHelper.EditRole(role.Id, role.Name, role.IdGroup <= 0 ? null : role.IdGroup, _repository.UserName)
                    : DbHelper.AddRole(role.Name, role.IdGroup <= 0 ? null : role.IdGroup, _repository.UserName);
                if (!res)
                {
                    ModelState.AddModelError("Db", "Роль не была добавлена! Повторите попытку или свяжитесь с администратором.");
                    return View(role);
                }
                return View("Saved");
            }
            else
            {
                return View(role);
            }
        }

        // GET: User
        [Authorize]
        public ActionResult RoleRights()
        {
            _repository.UserName = User?.Identity?.Name;
            var rights = DbHelper.GetAllRights(_repository.UserName);
            var roles = DbHelper.GetAllRoles(_repository.UserName);
            if (rights == null || roles == null)
                return View(new RoleRightsViewModel());

            var model = new RoleRightsViewModel
            {
                Rights = rights,
                Roles = roles.Values
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult RoleRights(int id = 0)
        {
            _repository.UserName = User?.Identity?.Name;
            var rights = DbHelper.GetAllRights(_repository.UserName);
            var roles = DbHelper.GetAllRoles(_repository.UserName);
            if (rights == null || roles == null)
                return View(new RoleRightsViewModel());

            var model = new RoleRightsViewModel
            {
                Rights = rights,
                Roles = roles.Values
            };

            //if (Request.Form["submitbutton"] == null || Request.Form["submitbutton"] != "Сохранить")
            //    return View("RoleRights", model);

            var result = false;
            if (rights.Any() && roles.Any())
            {
                var newChecked = Request.Form.AllKeys;
                var toDelete = new List<RoleRight>();
                var toAdd = new List<RoleRight>();
                foreach (var role in roles.Values)
                {
                    foreach (var right in rights)
                    {
                        var inNewChecked = newChecked.Any(k => k == $"chk_{role.Id}_{right.Id}");
                        var inOldChecked = role.Rights.Any(r => r.Id == right.Id);

                        if (inNewChecked && inOldChecked)
                            continue;
                        if (inNewChecked)
                            toAdd.Add(new RoleRight
                            {
                                IdRole = role.Id,
                                IdRight = right.Id
                            });
                        else if (inOldChecked)
                            toDelete.Add(new RoleRight
                            {
                                IdRole = role.Id,
                                IdRight = right.Id
                            });
                    }
                }
                result = DbHelper.UpdateRoleRights(toAdd, toDelete, _repository.UserName);
            }
            if (result)
            {
                rights = DbHelper.GetAllRights(_repository.UserName);
                roles = DbHelper.GetAllRoles(_repository.UserName);
                if (rights == null || roles == null)
                    return View(new RoleRightsViewModel());

                var modelNew = new RoleRightsViewModel
                {
                    Rights = rights,
                    Roles = roles.Values
                };

                return View("RoleRights", modelNew);
            }
            else
            {
                ModelState.AddModelError("Db", "Роли пользователей не был изменены! Повторите попытку или свяжитесь с администратором.");
                return View("RoleRights", model);
            }
        }


    }
}