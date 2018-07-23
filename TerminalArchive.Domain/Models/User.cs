using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TerminalArchive.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите имя пользователя!")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите пароль пользователя!")]
        [Display(Name = "Пароль")]
        public string Pass { get; set; }
        [Display(Name = "Старый пароль")]
        public string OldPass { get; set; }
        [Display(Name = "Роли")]
        public List<Role> Roles { get; set; }
        public List<Role> AllRoles { get; set; }
    }

    public class UserRole
    {
        public int IdUser { get; set; }
        public int IdRole { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        [Display(Name = "Принадлежность к группе терминалов")]
        public int? IdGroup { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите имя роли!")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Display(Name = "Права")]
        public List<Right> Rights { get; set; }
    }
    public class Right
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите имя права!")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
    }
    public class RoleRight
    {
        public int IdRole { get; set; }
        public int IdRight { get; set; }
    }

}
