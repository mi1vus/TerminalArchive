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
            _repository.UserName = User?.Identity?.Name;
            DbHelper.UpdateAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None"} };
            groups.AddRange(DbHelper.Groups);
            ViewBag.Groups = groups;
                //DbHelper.UserTerminalGroup(_repository.UserName);
                    //.Select(t => new SelectListItem {Value = t.Id.ToString(), Text = t.Name});
            return View(new Terminal());
        }

        [Authorize]
        [HttpPost]
        public ViewResult Add(Terminal terminal)
        {
            _repository.UserName = User?.Identity?.Name;
            DbHelper.UpdateAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(DbHelper.Groups);
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
            var terminalsModel = new TerminalDetailViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = _repository.Terminals.Count()
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
        public ViewResult Edit(int id, int page)
        {
            _repository.UserName = User?.Identity?.Name;
            DbHelper.UpdateAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(DbHelper.Groups);
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
            var terminalsModel = new TerminalDetailViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = _repository.Terminals.Count()
                },
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };

            return View(terminal);
        }

        [Authorize]
        [HttpPost]
        public ViewResult Edit(Terminal terminalMod, int id, int page)
        {
            _repository.UserName = User?.Identity?.Name;
            DbHelper.UpdateAllGroups(_repository.UserName);
            var groups = new List<Group> { new Group { Id = -1, Name = "None" } };
            groups.AddRange(DbHelper.Groups);
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
                var terminalsModel = new TerminalDetailViewModel
                {
                    PagingInfo = new PagingInfo
                    {
                        CurrentPage = page,
                        ItemsPerPage = PageSize,
                        TotalItems = _repository.Terminals.Count()
                    },
                    Terminal = new ViewTerminal(terminal)
                    {
                        GroupsIdsString = terminal.IdGroup.ToString(),
                        GroupsNamesString = terminal.Group?.Name
                    }
                };

                return View(terminal);
            }
        }
    }
}