using System.Collections.Generic;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.Domain.Abstract
{
    public interface ITerminalRepository
    {
        string UserName { get; set; }
        IEnumerable<Terminal> Terminals { get; }
    }
}