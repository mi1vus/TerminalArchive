using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TerminalArchive.Domain.Abstract;
using TerminalArchive.Domain.Models;
using TerminalArchive.WebUI.Controllers;
using TerminalArchive.WebUI.HtmlHelpers;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            // Организация (arrange)
            Mock<ITerminalRepository> mock = new Mock<ITerminalRepository>();
            mock.Setup(m => m.Terminals).Returns(new List<Terminal>
            {
                new Terminal { Id = 1, Name = "Терм1", Address = "Аддр1", IdHasp = "1", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 2, Name = "Терм2", Address = "Аддр2", IdHasp = "2", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 3, Name = "Терм3", Address = "Аддр3", IdHasp = "3", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 4, Name = "Терм4", Address = "Аддр4", IdHasp = "4", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 5, Name = "Терм5", Address = "Аддр5", IdHasp = "5", Groups = new Dictionary<int, Group>()}
            });
            TerminalMonitoringController controller = new TerminalMonitoringController(mock.Object);
            controller.PageSize = 3;

            // Действие (act)
            TerminalsListViewModel result = (TerminalsListViewModel)controller.List(2).Model;

            // Утверждение (assert)
            List<ViewTerminal> terminals = result.Terminals.ToList();
            Assert.IsTrue(terminals.Count == 2);
            Assert.AreEqual(terminals[0].Name, "Терм4");
            Assert.AreEqual(terminals[1].Name, "Терм5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {

            // Организация - определение вспомогательного метода HTML - это необходимо
            // для применения расширяющего метода
            HtmlHelper myHelper = null;

            // Организация - создание объекта PagingInfo
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            // Организация - настройка делегата с помощью лямбда-выражения
            Func<int, string> pageUrlDelegate = i => "Page" + i;

            // Действие
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            // Утверждение
            Assert.AreEqual(@"<a class=""btn btn-default"" href=""Page1"">1</a>"
                + @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>"
                + @"<a class=""btn btn-default"" href=""Page3"">3</a>",
                result.ToString());
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            // Организация (arrange)
            Mock<ITerminalRepository> mock = new Mock<ITerminalRepository>();
            mock.Setup(m => m.Terminals).Returns(new List<Terminal>
            {
                new Terminal { Id = 1, Name = "Терм1", Address = "Аддр1", IdHasp = "1", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 2, Name = "Терм2", Address = "Аддр2", IdHasp = "2", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 3, Name = "Терм3", Address = "Аддр3", IdHasp = "3", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 4, Name = "Терм4", Address = "Аддр4", IdHasp = "4", Groups = new Dictionary<int, Group>()},
                new Terminal { Id = 5, Name = "Терм5", Address = "Аддр5", IdHasp = "5", Groups = new Dictionary<int, Group>()}
            });
            TerminalMonitoringController controller = new TerminalMonitoringController(mock.Object);
            controller.PageSize = 3;

            // Действие (act)
            TerminalsListViewModel result = (TerminalsListViewModel)controller.List(2).Model;

            // Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }
    }
}