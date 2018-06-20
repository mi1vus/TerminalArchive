using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.WebUI.Models
{
    public class ViewTerminal : Terminal
    {
        public string GroupsIdsString { get; set; }
        public string GroupsNamesString { get; set; }

        public ViewTerminal(Terminal terminal)
        {
            Id = terminal.Id;
            Name = terminal.Name;
            Address = terminal.Address;
            IdHasp = terminal.IdHasp;
            IdGroup = terminal.IdGroup;
            Orders = terminal.Orders;
            Parameters = terminal.Parameters;
            Group = terminal.Group;
        }
    }

    public class TerminalsListViewModel
    {
        public IEnumerable<ViewTerminal> Terminals { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }

    public class TerminalDetailViewModel
    {
        public ViewTerminal Terminal { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}