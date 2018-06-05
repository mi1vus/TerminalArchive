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
    public class TerminalMonitoringController : Controller
    {
        private readonly ITerminalRepository _repository;
        public int PageSize = 2;

        public TerminalMonitoringController()
        {
            _repository = new TerminalRepository();
            _repository.UserName = "Admin";
        }

        public TerminalMonitoringController(ITerminalRepository repo)
        {
            _repository = repo;
            _repository.UserName = "Admin";
        }

        public ViewResult List(int page = 1)
        {
            var terminalsModel = new TerminalsListViewModel
            {
                Terminals =
                    from terminal in _repository.Terminals.OrderBy(t => t.Id).Skip((page - 1)*PageSize).Take(PageSize)
                    let tGrpIds =
                    terminal.Groups.Values.Any()
                        ? terminal.Groups.Values.Select(t => t.Id.ToString())
                            .Aggregate((current, next) => current + ", " + next)
                        : " - "
                    let tGrpNms =
                    terminal.Groups.Values.Any()
                        ? terminal.Groups.Values.Select(t => t.Name).Aggregate((current, next) => current + ", " + next)
                        : " - "
                    select new ViewTerminal(terminal)
                    {
                        GroupsIdsString = tGrpIds,
                        GroupsNamesString = tGrpNms
                    },
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = _repository.Terminals.Count()
                }
            };

            return View(terminalsModel);
        }
    }
}