using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.DB;
using TerminalArchive.Domain.Models;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class TerminalController : Controller
    {
        private readonly ITerminalRepository _repository;
        public int PageSize = 10;

        public TerminalController()
        {
            _repository = new TerminalRepository {UserName = User?.Identity?.Name };
        }

        public TerminalController(ITerminalRepository repo)
        {
            _repository = repo;
            _repository.UserName = User?.Identity?.Name;
        }

        [Authorize]
        public ViewResult Add()
        {
            if(!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None"} };
            groups.AddRange(groupsAll.Values);
            ViewBag.Groups = groups;
                //DbHelper.GetUserGroups(_repository.UserName);
                    //.Select(t => new SelectListItem {Value = t.Id.ToString(), Text = t.Name});
            return View(new Terminal());
        }

        [Authorize]
        [HttpPost]
        public ViewResult Add(Terminal terminal)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll.Values);
            ViewBag.Groups = groups;

            if (ModelState.IsValid)
            {
                if (!DbHelper.AddTerminal(terminal.IdHasp, terminal.IdGroup, terminal.Name, terminal.Address, _repository.UserName))
                {
                    ModelState.AddModelError("Db", "Терминал не был добавлен! Повторите попытку или свяжитесь с администратором.");
                    return View(terminal);
                }
                return View("Saved");
            }
            else
            {
                return View(terminal);
            }
        }

        [Authorize]
        public ViewResult Details(int id, int page = 1)
        {
            _repository.UserName = User?.Identity?.Name;

            var groups = DbHelper.GetUserGroups(_repository.UserName, "Read");
            var terminal = _repository.GetTerminal(id, page, PageSize);
            if (groups == null || terminal == null || (groups.Any() && groups.All(g => g.Id != terminal.IdGroup)))
                return View("Unauthorize");

            //int page = 1;
            //var tGrpIds =
            //    terminal.Groups.Values.Any()
            //        ? terminal.Groups.Values.Select(t => t.Id.ToString())
            //            .Aggregate((current, next) => current + ", " + next)
            //        : " - ";
            //var tGrpNms =
            //    terminal.Groups.Values.Any()
            //        ? terminal.Groups.Values.Select(t => t.Name)
            //            .Aggregate((current, next) => current + ", " + next)
            //        : " - ";
            var maxPages = 0;
            int totalItems = DbHelper.OrdersCount(_repository.UserName, id);
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);

            var terminalsModel = new TerminalDetailViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page > maxPages ? maxPages: page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                },
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name 
                }
            };

            return View(terminalsModel);
        }

        [Authorize]
        public ViewResult Parameters(int id)
         {
            _repository.UserName = User?.Identity?.Name;

            var groups = DbHelper.GetUserGroups(_repository.UserName, "Read");
            var terminal = _repository.GetTerminal(id);
            if (groups == null || terminal == null || (groups.Any() && groups.All(g => g.Id != terminal.IdGroup)))
                return View("Unauthorize");

            var parameters = DbHelper.GetAllParameters(terminal.IdGroup);
            
            var terminalsModel = new TerminalParametersViewModel
            {
                Parameters = parameters.Select(p=>new Parameter
                {
                    Id = p.Id,
                    TId = terminal?.Id ?? 0,//terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.TId ?? 0,
                    Name = p.Name,
                    Path = p.Path,
                    Value = terminal?.Parameters?.FirstOrDefault(tp=>tp.Id == p.Id)?.Value,
                    SaveTime = terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.SaveTime ?? DateTime.MinValue,
                    LastEditTime = terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.LastEditTime ?? DateTime.MinValue,
                    Description = p.Description
                }),
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };

            ViewBag.CanEdit = 
                DbHelper.UserIsAdmin(User?.Identity?.Name)
                || DbHelper.UserInRole(User?.Identity?.Name,"Write", terminal.IdGroup)
                || DbHelper.UserInRole(User?.Identity?.Name, "Write", null);
            return View(terminalsModel);
        }

        [Authorize]
        [HttpPost]
        public ViewResult Parameters(int id, Terminal term)
        {
            _repository.UserName = User?.Identity?.Name;

            var groups = DbHelper.GetUserGroups(_repository.UserName, "Read");
            var terminal = _repository.GetTerminal(id);
            if (groups == null || terminal == null || (groups.Any() && groups.All(g => g.Id != terminal.IdGroup)))
                return View("Unauthorize");

            var result = false;

            var toUpdate = new Dictionary<Tuple<int, int>, TerminalParameter>();
            foreach (var k in Request.Form.AllKeys)
            {
                if (k.StartsWith("all"))
                {
                    var keyParts = k.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                    int idTerminal = int.Parse(keyParts[1]);
                    int idParameter = int.Parse(keyParts[2]);
                    var key = new Tuple<int, int>(idTerminal, idParameter);
                    int idGroup = int.Parse(keyParts[3]);
                    if (toUpdate.ContainsKey(key))
                    {
                        var newVal = toUpdate[key];
                        newVal.ToAllGroups = true;
                        toUpdate[key] = newVal;
                    }
                    else
                    {
                        var newPar = new TerminalParameter
                        {
                            IdTerminal = idTerminal,
                            IdParameter = idParameter,
                            IdGroupTerminal = idGroup,
                            ToAllGroups = true,
                        };
                        toUpdate.Add(key, newPar);
                    }
                }
                else if (k.StartsWith("val"))
                {
                    var keyParts = k.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                    int idTerminal = int.Parse(keyParts[1]);
                    int idParameter = int.Parse(keyParts[2]);
                    var key = new Tuple<int, int>(idTerminal, idParameter);
                    int idGroup = int.Parse(keyParts[3]);
                    if (toUpdate.ContainsKey(key))
                    {
                        var newVal = toUpdate[key];
                        newVal.Value = Request.Form[k];
                        toUpdate[key] = newVal;
                    }
                    else
                    {
                        var newPar = new TerminalParameter
                        {
                            IdTerminal = idTerminal,
                            IdParameter = idParameter,
                            IdGroupTerminal = idGroup,
                            Value = Request.Form[k]
                        };
                        toUpdate.Add(key, newPar);
                    }
                }
            }

            if (toUpdate.Values.Any(par => par.ToAllGroups))
            {
                var terminal1 = terminal;
                var terminalsToUpd = _repository.Terminals.Where(t => t.IdGroup == terminal1.IdGroup && t.Id != id);
                var srcParams = toUpdate.Values.Where(par => par.ToAllGroups).ToArray();
                foreach (var termToUpd in terminalsToUpd)
                    foreach (var parToUpd in srcParams.Select(tp => new TerminalParameter
                        {
                            IdParameter = tp.IdParameter,
                            IdTerminal = termToUpd.Id,
                            IdGroupTerminal = termToUpd.IdGroup,
                            Value = tp.Value,
                            ToAllGroups = tp.ToAllGroups,
                    })
                    )
                        toUpdate.Add(new Tuple<int, int>(parToUpd.IdTerminal, parToUpd.IdParameter), parToUpd);
                result = DbHelper.UpdateTerminalParameters(toUpdate.Values.Where(par => par.ToAllGroups), _repository.UserName);
            }
            else
                result = true;

            if (result && toUpdate.Values.Any(par => !par.ToAllGroups))
            {
                var terminal1 = terminal;
                result = DbHelper.UpdateTerminalParameters(
                    toUpdate.Values.Where(toUpdadePar => /*!string.IsNullOrWhiteSpace(toUpdadePar.Value) &&*/
                        !toUpdadePar.ToAllGroups &&
                        terminal1?.Parameters?.FirstOrDefault(
                            termParam => termParam.Id == toUpdadePar.IdParameter)?.Value !=
                        toUpdadePar.Value)
                    , _repository.UserName);
            }
            if (!result)
                ModelState.AddModelError("Db",
                    "Параметры терминала не были изменены! Повторите попытку или свяжитесь с администратором.");

            terminal = _repository.GetTerminal(id);
            var parameters = DbHelper.GetAllParameters(terminal.IdGroup);

            var terminalsModel = new TerminalParametersViewModel
            {
                Parameters = parameters.Select(p => new Parameter
                {
                    Id = p.Id,
                    TId = terminal?.Id ?? 0,//terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.TId ?? 0,
                    Name = p.Name,
                    Path = p.Path,
                    Value = terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.Value,
                    SaveTime = terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.SaveTime ?? DateTime.MinValue,
                    LastEditTime = terminal?.Parameters?.FirstOrDefault(tp => tp.Id == p.Id)?.LastEditTime ?? DateTime.MinValue,
                    Description = p.Description
                }),
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };
            ViewBag.CanEdit =
                DbHelper.UserIsAdmin(User?.Identity?.Name)
                || DbHelper.UserInRole(User?.Identity?.Name, "Write", terminal.IdGroup)
                || DbHelper.UserInRole(User?.Identity?.Name, "Write", null);

            return View(terminalsModel);
        }

        [Authorize]
        public ViewResult Edit(int id, int page = 0)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll.Values);
            ViewBag.Groups = groups;

            //int page = 1;
            var terminal = _repository.GetTerminal(id, page, PageSize);
            //var tGrpIds =
            //    terminal.Groups.Values.Any()
            //        ? terminal.Groups.Values.Select(t => t.Id.ToString())
            //            .Aggregate((current, next) => current + ", " + next)
            //        : " - ";
            //var tGrpNms =
            //    terminal.Groups.Values.Any()
            //        ? terminal.Groups.Values.Select(t => t.Name)
            //            .Aggregate((current, next) => current + ", " + next)
            //        : " - ";
            //var terminalsModel = new TerminalDetailViewModel
            //{
            //    PagingInfo = new PagingInfo
            //    {
            //        CurrentPage = page,
            //        ItemsPerPage = PageSize,
            //        TotalItems = _repository.Terminals.Count()
            //    },
            //    Terminal = new ViewTerminal(terminal)
            //    {
            //        GroupsIdsString = terminal.IdGroup.ToString(),
            //        GroupsNamesString = terminal.Group?.Name
            //    }
            //};

            return View(terminal);
        }

        [Authorize]
        [HttpPost]
        public ViewResult Edit(Terminal terminalMod, int id, int page = 0)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll.Values);
            ViewBag.Groups = groups;

            var terminal = _repository.GetTerminal(id, page, PageSize);
            if (ModelState.IsValid)
            {
                if (!DbHelper.EditTerminal(id, terminalMod.IdHasp, terminalMod.IdGroup, terminalMod.Name, terminalMod.Address, _repository.UserName))
                {
                    ModelState.AddModelError("Db", "Терминал не был отредактирован! Повторите попытку позже или свяжитесь с администратором.");
                    return View(terminal);
                }
                return View("Saved");
            }
            else
            {
                //var tGrpIds =
                //    terminal.Groups.Values.Any()
                //        ? terminal.Groups.Values.Select(t => t.Id.ToString())
                //            .Aggregate((current, next) => current + ", " + next)
                //        : " - ";
                //var tGrpNms =
                //    terminal.Groups.Values.Any()
                //        ? terminal.Groups.Values.Select(t => t.Name)
                //            .Aggregate((current, next) => current + ", " + next)
                //        : " - ";
                //var terminalsModel = new TerminalDetailViewModel
                //{
                //    PagingInfo = new PagingInfo
                //    {
                //        CurrentPage = page,
                //        ItemsPerPage = PageSize,
                //        TotalItems = _repository.Terminals.Count()
                //    },
                //    Terminal = new ViewTerminal(terminal)
                //    {
                //        GroupsIdsString = terminal.IdGroup.ToString(),
                //        GroupsNamesString = terminal.Group?.Name
                //    }
                //};

                return View(terminal);
            }
        }

        [Authorize]
        public ActionResult ListGroups()
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groups = DbHelper.GetAllGroups(_repository.UserName);

            return View(groups.Values);
        }
        [Authorize]
        public ActionResult AddOrEditGroup(int id = 0)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var allParameters = DbHelper.GetAllParameters(0);

            if (id != 0)
            {
                var groups = DbHelper.GetAllGroups(_repository.UserName);
                var group = groups.Values.Single(g => g.Id == id);
                group.AllParameters = allParameters ?? new List<Parameter>();
                return View(group);
            }
            else
                return View(new Group {AllParameters = allParameters ?? new List<Parameter>() });
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddOrEditGroup(int id = 0, Group group = null)
        {
            var allParameters = DbHelper.GetAllParameters(0);
            if (group == null) return View(new Group { AllParameters = allParameters ?? new List<Parameter>() });
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            group.AllParameters = allParameters ?? new List<Parameter>();

            if (!ModelState.IsValid) return View(group);

            var res = group.Id != 0
                ? DbHelper.EditGroup(group.Id, group.Name, _repository.UserName)
                : DbHelper.AddGroup(group.Name, _repository.UserName);
            if (!res)
            {
                ModelState.AddModelError("Db", "Группа не была добавлена! Повторите попытку или свяжитесь с администратором.");
                return View(group);
            }

            var groups = DbHelper.GetAllGroups(_repository.UserName);
            group = groups.Values.Single(g => g.Id == id);
            group.AllParameters = allParameters ?? new List<Parameter>();
            var result = false;

            if (allParameters != null && allParameters.Any())
            {
                var newChecked = Request.Form.AllKeys;
                var toDelete = new List<ParameterGroup>();
                var toAdd = new List<ParameterGroup>();
                foreach (var parameter in allParameters)
                {
                    var inNewChecked = newChecked.Any(k => k == $"chk_{parameter.Id}_{group.Id}");
                    var inOldChecked = group.Parameters.Any(p => p.Id == parameter.Id);

                    if (inNewChecked && inOldChecked)
                        continue;
                    if (inNewChecked)
                        toAdd.Add(new ParameterGroup
                        {
                            IdParameter = parameter.Id,
                            IdGroup = group.Id
                        });
                    else if (inOldChecked)
                        toDelete.Add(new ParameterGroup
                        {
                            IdParameter = parameter.Id,
                            IdGroup = group.Id
                        });
                }
                if (toAdd.Any() || toDelete.Any())
                    result = DbHelper.UpdateParameterGroups(toAdd, toDelete, _repository.UserName);
                else
                    result = true;
            }
            else
                return View("Saved");

            if (result)
            {
                //user = DbHelper.GetUser(_repository.UserName);
                //roles = DbHelper.GetAllRoles(_repository.UserName);
                //if (users == null || roles == null)
                //    return View(new UserRolesViewModel());

                //var modelNew = new UserViewModel
                //{
                //    User = users.Values,
                //    Roles = roles.Values
                //};

                return View("Saved");
            }

            ModelState.AddModelError("Db", "Параметры группы не были изменены! Повторите попытку или свяжитесь с администратором.");
            return View(group);
        }
    }
}