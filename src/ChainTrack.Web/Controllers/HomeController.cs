using System.Diagnostics;
using ChainTrack.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChainTrack.Web.Controllers;

/// <summary>Početna stranica aplikacije (prezentacijski sloj).</summary>
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
