using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopDotNet.Services;

namespace ShopDotNet.Controllers.Admin;

public abstract class AdminBaseController : BaseController
{
    protected AdminBaseController(
        IDatabaseService db,
        ISettingsService settings,
        ICartService cart,
        IAuthService auth,
        ISecurityService security,
        IReviewService reviews,
        IWishlistService wishlist,
        IAddressService addresses,
        IAttributeService attributes)
        : base(db, settings, cart, auth, security, reviews, wishlist, addresses, attributes)
    {
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Call base to set up ViewData, but redirect logic is now in Middleware
        base.OnActionExecuting(context);
    }
}
