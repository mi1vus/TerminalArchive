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

        public IEnumerable<Terminal> Terminals {
            get
            {
                if (DbHelper.UpdateTerminals(UserName, 1, int.MaxValue, true))
                    return DbHelper.Terminals.Values;
                else
                    return null;
            }
        }
    }
}
