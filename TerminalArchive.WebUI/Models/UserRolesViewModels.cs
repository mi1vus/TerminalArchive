using System.Collections.Generic;
using TerminalArchive.Domain.Models;

namespace TerminalArchive.WebUI.Models
{
    public class UserViewModel
    {
        public User User { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
    public class UserRolesViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
    public class RoleRightsViewModel
    {
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<Right> Rights { get; set; }
    }
}