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

            var terminalsModel = new TerminalsListViewModel
            {
                Terminals = new List<ViewTerminal>(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = 1,
                    ItemsPerPage = PageSize,
                    TotalItems = 0
                }
            };

            //if (!DbHelper.UserInRole(_repository.UserName, "Read", null))
            //    return View(terminalsModel);

            int maxPages;
            var terminals = _repository.Terminals as IList<Terminal> ?? _repository.Terminals.ToList();
            var totalItems = terminals.Count;
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);

            terminalsModel.Terminals =
            from terminal in terminals.OrderBy(t => t.Id).Skip((page - 1)*PageSize).Take(PageSize)
                select new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup > 0 ? terminal.IdGroup.ToString() : " - ",
                    GroupsNamesString = terminal.Group?.Name ?? " - "
                };
            terminalsModel.PagingInfo = new PagingInfo
            {
                CurrentPage = page > maxPages ? maxPages : page,
                ItemsPerPage = PageSize,
                TotalItems = totalItems
            };

            ViewBag.CurrentController = GetType().ToString();
            return View(terminalsModel);
        }
    }
}