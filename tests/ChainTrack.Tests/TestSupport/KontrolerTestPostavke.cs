using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace ChainTrack.Tests.TestSupport;

/// <summary>
/// Priprema instancu kontrolera za jedinično testiranje izvan ASP.NET cjevovoda
/// (postavlja ControllerContext, ViewData/ViewBag te TempData).
/// </summary>
public static class KontrolerTestPostavke
{
    public static T Pripremi<T>(T kontroler) where T : Controller
    {
        var httpContext = new DefaultHttpContext();

        kontroler.ControllerContext = new ControllerContext { HttpContext = httpContext };
        kontroler.ViewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(), kontroler.ModelState);
        kontroler.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return kontroler;
    }
}
