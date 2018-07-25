using System;
using System.ComponentModel.DataAnnotations;

namespace TerminalArchive.Domain.Models
{
    public class History
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите время ошибки!")]
        [Display(Name = "Время")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите код терминала!")]
        [Display(Name = "Id терминала")]
        public int IdTerminal { get; set; }
        [Display(Name = "Терминал")]
        public string Terminal { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите код заказа!")]
        [Display(Name = "Id заказа")]
        public int IdOrder { get; set; }
        [Display(Name = "Заказ")]
        public string Order { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите код статуса!")]
        [Display(Name = "Id статуса")]
        public int IdState { get; set; }
        [Display(Name = "Состояние")]
        public string State { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите источник ошибки!")]
        [Display(Name = "Расположение")]
        public string Trace { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите текст сообщения!")]
        [Display(Name = "Текст")]
        public string Message { get; set; }
    }
}