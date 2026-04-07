using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShopDotNet.Controllers.Admin;

public abstract class AdminBaseController : BaseController
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var user = CurrentUser;
        if (user == null)
        {
            context.Result = Redirect("/login");
            return;
        }
        if (!user.IsAdmin)
        {
            context.Result = Redirect("/");
        }
    }
}
