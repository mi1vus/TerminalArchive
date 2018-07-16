using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using TerminalArchive.Domain.DB;
using TerminalArchive.WebUI.Models;

namespace TerminalArchive.WebUI.Controllers
{
    public class AccountController : Controller
    {
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                if (DbHelper.IsAuthorizeUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false);
                    return Redirect(returnUrl ?? Url.Action("List", "TerminalMonitoring"));
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин или пароль");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            if (Request.Form["submitbutton"] != null && Request.Form["submitbutton"] == "Выйти")
            {
                FormsAuthentication.SignOut();
            }
            else
            if (Request.Form["submitbutton"] != null && Request.Form["submitbutton"] == "Сменить пароль")
            {
                var userId = DbHelper.GetUsersId(User?.Identity?.Name);
                return Redirect(Url.Action("AddOrEdit", "User", new {id = userId }));
            }


            var url = Request["ReturnUrl"];
            return Redirect(url ?? Url.Action("List", "TerminalMonitoring"));
        }

    }
}