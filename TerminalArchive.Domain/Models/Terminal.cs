using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TerminalArchive.Domain.Models
{
    public class Terminal
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите имя терминала!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите адрес терминала!")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите код терминала!")]
        public string IdHasp { get; set; }
        public Dictionary<int, Order> Orders { get; set; }
        public List<Parameter> Parameters { get; set; }
        public Dictionary<int, Group> Groups { get; set; }
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

    public class AdditionalParameter
    {
        public int Id { get; set; }
        public int IdOrder { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Parameter
    {
        public int Id { get; set; }
        public int TId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        public DateTime LastEditTime { get; set; }
        public DateTime SaveTime { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}