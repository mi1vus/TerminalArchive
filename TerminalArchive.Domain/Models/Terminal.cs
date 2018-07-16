using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TerminalArchive.Domain.Models
{
    public class Terminal
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите имя терминала!")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите адрес терминала!")]
        [Display(Name = "Адрес")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите код терминала!")]
        [Display(Name = "Hasp уникальный номер")]
        public string IdHasp { get; set; }
        [Display(Name = "Принадлежность к группе терминалов")]
        public int IdGroup { get; set; }
        [ScaffoldColumn(false)]
        public Dictionary<int, Order> Orders { get; set; }
        [ScaffoldColumn(false)]
        public List<Parameter> Parameters { get; set; }
        [ScaffoldColumn(false)]
        public Group Group { get; set; }
    }

    public class TerminalGroup
    {
        public int IdTerminal { get; set; }
        public int IdGroup { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string Rnn { get; set; }
        public int IdState { get; set; }
        public string StateName { get; set; }
        public int IdTerminal { get; set; }
        public string TerminalName { get; set; }
        public List<AdditionalParameter> AdditionalParameters { get; set; }
        public int IdFuel { get; set; }
        public string FuelName { get; set; }
        public int IdPayment { get; set; }
        public string PaymentName { get; set; }
        public int IdPump { get; set; }
        public decimal PrePrice { get; set; }
        public decimal Price { get; set; }
        public decimal PreQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal PreSumm { get; set; }
        public decimal Summ { get; set; }
    }

    public class TerminalParameter
    {
        public int IdTerminal { get; set; }
        public int IdParameter { get; set; }
        public string Value { get; set; }
    }

    public class AdditionalParameter
    {
        public int Id { get; set; }
        public int IdOrder { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Parameter
    {
        [Required(ErrorMessage = "Не указан id параметра!")]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Не указан id терминала!")]
        [ScaffoldColumn(false)]
        public int TId { get; set; }
        //[Required(ErrorMessage = "Не указан id параметра терминала!")]
        //[ScaffoldColumn(false)]
        //public int TPId { get; set; }
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Путь")]
        public string Path { get; set; }
        [Display(Name = "Значение")]
        public string Value { get; set; }
        [ScaffoldColumn(false)]
        public DateTime LastEditTime { get; set; }
        [ScaffoldColumn(false)]
        public DateTime SaveTime { get; set; }
        [Display(Name = "Сохранен")]
        public bool Saved => SaveTime >= LastEditTime;
    }

    public class Group
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите название группы!")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
    }
}