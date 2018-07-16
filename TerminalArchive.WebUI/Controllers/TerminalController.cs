using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        public int PageSize = 2;

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
            groups.AddRange(groupsAll);
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
            groups.AddRange(groupsAll);
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
            var terminal = _repository.GetTerminal(id);
            var parameters = DbHelper.GetAllParameters();

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
                }),
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };


            return View(terminalsModel);
        }

        [Authorize]
        [HttpPost]
        public ViewResult Parameters(int id, Terminal term)
        {
            _repository.UserName = User?.Identity?.Name;
            var terminal = _repository.GetTerminal(id);
            var result = false;

            var toUpdate = new List<TerminalParameter>();
            bool allInGroup = false;
            foreach (var k in Request.Form.AllKeys)
            {
                if (k == "all_in_group")
                    allInGroup = true;
                else if(k.StartsWith("val"))
                {
                    var keyParts = k.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                    int idTerminal = int.Parse(keyParts[1]);
                    int idParameter = int.Parse(keyParts[2]);
                    var newPar = new TerminalParameter
                    {
                        IdTerminal = idTerminal,
                        IdParameter = idParameter,
                        Value = Request.Form[k]
                    };
                    toUpdate.Add(newPar);
                }
            }

            if (allInGroup)
            {
                var terminalIds = _repository.Terminals.Where(t=>t.IdGroup == terminal.IdGroup && t.Id != id).Select(t=>t.Id);
                var srcParams = toUpdate.ToArray();
                foreach (var tId in terminalIds)
                    toUpdate.AddRange(srcParams.Select(tp=>new TerminalParameter
                    {
                        IdParameter = tp.IdParameter,
                        IdTerminal = tId,
                        Value = tp.Value
                    }));
                result = DbHelper.UpdateTerminalParameters(toUpdate);
            }else
                result = DbHelper.UpdateTerminalParameters(
                    toUpdate.Where(toUpdadePar=>!string.IsNullOrWhiteSpace(toUpdadePar.Value)
                    && terminal?.Parameters?.FirstOrDefault(termParam => termParam.Id == toUpdadePar.IdParameter)?.Value != toUpdadePar.Value));

            if (!result)
                ModelState.AddModelError("Db",
                    "Параметры терминала не были изменены! Повторите попытку или свяжитесь с администратором.");

            terminal = _repository.GetTerminal(id);
            var parameters = DbHelper.GetAllParameters();

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
                }),
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };
            return View(terminalsModel);
        }

        [Authorize]
        public ViewResult Edit(int id, int page)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll);
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
        public ViewResult Edit(Terminal terminalMod, int id, int page)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;
            var groupsAll = DbHelper.GetAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(groupsAll);
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

            return View(groups);
        }
        [Authorize]
        public ActionResult AddOrEditGroup(int id = 0)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;

            if (id != 0)
            {
                var groups = DbHelper.GetAllGroups(_repository.UserName);
                return View(groups.Single(g => g.Id == id));
            }
            else
                return View(new Group());
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddOrEditGroup(int id = 0, Group group = null)
        {
            if (!DbHelper.UserIsAdmin(User?.Identity?.Name))
                return View("Unauthorize");

            _repository.UserName = User?.Identity?.Name;

            if (ModelState.IsValid && group != null)
            {
                var res = group.Id != 0
                    ? DbHelper.EditGroup(group.Id, group.Name, _repository.UserName)
                    : DbHelper.AddGroup(group.Name, _repository.UserName);
                if (!res)
                {
                    ModelState.AddModelError("Db", "Группа не была добавлена! Повторите попытку или свяжитесь с администратором.");
                    return View(group);
                }
                return View("Saved");
            }
            else
            {
                return View(group);
            }
        }


    }
}