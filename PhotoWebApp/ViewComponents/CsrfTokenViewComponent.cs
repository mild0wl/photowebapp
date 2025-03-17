using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace PhotoWebApp.ViewComponents
{
    public class CsrfTokenViewComponent : ViewComponent
    {
        private readonly IAntiforgery _antiforgery;

        public CsrfTokenViewComponent(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public IViewComponentResult Invoke()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return View((object)tokens.RequestToken);
        }
    }
}
