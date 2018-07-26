using System;
using System.Linq;
using System.Web.Mvc;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.DB;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ITerminalRepository _repository;
        public int PageSize = 10;

        public HistoryController()
        {
            _repository = new TerminalRepository { UserName = User?.Identity?.Name };
        }

        public bool AddHistory(
            string HaspId, string RRN,
            string Trace, string Msg, int? ErrorLevel, string Date,
            string User, string Pass
        )
        {
            return DbHelper.AddHistory(HaspId, RRN,Trace , Msg, ErrorLevel, Date, User, Pass);
        }

        [Authorize]
        public ViewResult List(int id, int page = 1)
        {
            _repository.UserName = User?.Identity?.Name;

            var groups = DbHelper.GetUserGroups(_repository.UserName, Constants.RightReadName);
            var terminal = _repository.GetTerminal(id);
            if (groups == null || terminal == null || (groups.Any() && groups.All(g => g.Id != terminal.IdGroup)))
                return View("Unauthorize");

            var history = DbHelper.GetHistory(_repository.UserName, id, page, PageSize);
            var maxPages = 0;
            int totalItems = DbHelper.HistoryCount(_repository.UserName, id);
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);

            var terminalsModel = new HistoryViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page > maxPages ? maxPages : page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                },
                History = history,
                Terminal = new ViewTerminal(terminal)
                {
                    GroupsIdsString = terminal.IdGroup.ToString(),
                    GroupsNamesString = terminal.Group?.Name
                }
            };

            return View(terminalsModel);
        }
    }
}
