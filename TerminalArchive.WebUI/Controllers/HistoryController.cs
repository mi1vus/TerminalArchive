using System;
using System.Linq;
using System.Web.Mvc;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.DB;
using TerminalArchive.Domain.Models;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ITerminalRepository _repository;
        public int PageSize = 2;

        public HistoryController()
        {
            _repository = new TerminalRepository { UserName = User?.Identity?.Name };
        }

        [HttpPost]
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
                History = history
            };

            return View(terminalsModel);
        }

    }
}
