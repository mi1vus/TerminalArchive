using System.Collections.Generic;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.Domain.Abstract
{
    public interface ITerminalRepository
    {
        string UserName { get; set; }
        IEnumerable<Terminal> Terminals { get; }
        Terminal GetTerminal(int id, int orderPage = 0, int orderPageSize = 0);
    }
}