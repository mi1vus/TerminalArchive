using System;
using System.Linq;
using System.Web.Mvc;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.DB;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class TerminalMonitoringController : Controller
    {
        private readonly ITerminalRepository _repository;
        public int PageSize = 2;

        public TerminalMonitoringController()
        {
            _repository = new TerminalRepository {UserName = User?.Identity?.Name };
        }

        public TerminalMonitoringController(ITerminalRepository repo)
        {
            _repository = repo;
            _repository.UserName = User?.Identity?.Name;
        }

        [Authorize]
        public ViewResult List(int page = 1)
        {
            _repository.UserName = User?.Identity?.Name;

            var maxPages = 0;
            int totalItems = _repository.Terminals.Count();
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);


            var terminalsModel = new TerminalsListViewModel
            {
                Terminals =
                    from terminal in _repository.Terminals.OrderBy(t => t.Id).Skip((page - 1)*PageSize).Take(PageSize)
                    //let tGrpIds =
                    //terminal.Groups.Values.Any()
                    //    ? terminal.Groups.Values.Select(t => t.Id.ToString())
                    //        .Aggregate((current, next) => current + ", " + next)
                    //    : " - "
                    //let tGrpNms =
                    //terminal.Groups.Values.Any()
                    //    ? terminal.Groups.Values.Select(t => t.Name).Aggregate((current, next) => current + ", " + next)
                    //    : " - "
                    select new ViewTerminal(terminal)
                    {
                        GroupsIdsString = terminal.IdGroup > 0? terminal.IdGroup.ToString() : " - ",
                        GroupsNamesString = terminal.Group?.Name ?? " - "
                    },
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page > maxPages ? maxPages : page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                }
            };
            ViewBag.CurrentController = GetType().ToString();
            return View(terminalsModel);
        }
    }
}