using System.Collections.Generic;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.WebUI.Models
{
    public class HistoryViewModel
    {
        public IEnumerable<History> History { get; set; }
        public ViewTerminal Terminal { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}