using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.Domain.DB
{
    public class TerminalRepository : ITerminalRepository
    {
        public string UserName { get; set; }

        public IEnumerable<Terminal> Terminals => 
            DbHelper.UpdateTerminals(UserName, 1, int.MaxValue, true) ? DbHelper.Terminals.Values : null;

        public Terminal GetTerminal(int id, int orderPage, int orderPageSize)
        {
            if (DbHelper.Terminals.All(t => t.Key != id))
                DbHelper.UpdateTerminals(UserName, 1, int.MaxValue, true);

            if (DbHelper.Terminals.All(t => t.Key != id))
                return null;

            DbHelper.UpdateTerminalOrders(UserName, id, orderPage, orderPageSize);
            DbHelper.UpdateTerminalParameters(UserName, id);

            return DbHelper.Terminals[id];
        }
    }
}
