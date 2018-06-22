using System.Collections.Generic;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.Domain.DB
{
    public class TerminalRepository : ITerminalRepository
    {
        public string UserName { get; set; }

        public IEnumerable<Terminal> Terminals => 
            DbHelper.GetTerminals(UserName, 1, int.MaxValue, true).Values;

        public Terminal GetTerminal(int id, int orderPage, int orderPageSize)
        {
            var terminal = DbHelper.GetTerminal(id, UserName);
            if (terminal == null)
                return null;

            terminal.Orders = DbHelper.GetTerminalOrders(UserName, id, orderPage, orderPageSize);
            terminal.Parameters = DbHelper.GetTerminalParameters(UserName, id);

            return terminal;
        }
    }
}
