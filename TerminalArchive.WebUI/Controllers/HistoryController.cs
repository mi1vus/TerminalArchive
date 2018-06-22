using System.Web.Mvc;
using TerminalArchive.Domain.DB;

namespace TerminalArchive.WebUI.Controllers
{
    public class HistoryController : Controller
    {

        [HttpPost]
        public bool AddHistory(
            string HaspId, string RRN,
            string Trace, string Msg, int? ErrorLevel, string Date,
            string User, string Pass
        )
        {
            return DbHelper.AddHistory(HaspId, RRN,Trace , Msg, ErrorLevel, Date, User, Pass);
        }

    }
}
