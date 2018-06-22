using System;

namespace TerminalArchive.WebUI.Models
{
    public class PagingInfo
    {
        // Кол-во объектов
        public int TotalItems { get; set; }

        // Кол-во объектов на одной странице
        public int ItemsPerPage { get; set; }

        // Номер текущей страницы
        public int CurrentPage { get; set; }

        // Общее кол-во страниц
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}