using System.Web.Mvc;
using TerminalArchive.Domain.DB;

namespace TerminalArchive.WebUI.Controllers
{
    public class HistoryController : Controller
    {
        public HistoryController()
        {
        }

        [HttpPost]
        public bool AddHistory(
            string HaspId, string RRN,
            string Trace, string Msg, int? ErrorLevel,
            string User, string Pass
        )
        {
            return DbHelper.AddHistory(HaspId, RRN,Trace , Msg, ErrorLevel, User, Pass);
        }

    }
}
