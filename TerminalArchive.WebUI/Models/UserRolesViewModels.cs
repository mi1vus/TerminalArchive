using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.WebUI.Models
{
    public class UserRolesViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
}