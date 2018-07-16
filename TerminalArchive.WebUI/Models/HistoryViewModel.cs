using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.WebUI.Models
{
    public class HistoryViewModel
    {
        public IEnumerable<History> History { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}