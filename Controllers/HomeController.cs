using System.Diagnostics;
using AutoVerifyMVC.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using AutoVerifyMVC.Models;

namespace AutoVerifyMVC.Controllers;

public class HomeController : Controller
{
    private const int HomePageSize = 6;
    private readonly ICarRepository _carRepository;

    public HomeController(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _carRepository.GetPublicListingsAsync(null, null, 0, HomePageSize, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SearchCars(
        string? searchTerm, string? priceRange, CancellationToken cancellationToken)
    {
        var model = await _carRepository.GetPublicListingsAsync(
            searchTerm, priceRange, 0, HomePageSize, cancellationToken);
        return PartialView("_HomeCarCards", model);
    }

    [HttpGet]
    public async Task<IActionResult> LoadMoreCars(
        string? searchTerm, string? priceRange, int skip, CancellationToken cancellationToken)
    {
        if (skip < 0)
            return BadRequest();

        var model = await _carRepository.GetPublicListingsAsync(
            searchTerm, priceRange, skip, HomePageSize, cancellationToken);
        return PartialView("_HomeCarCards", model);
    }

    [HttpGet]
    public async Task<IActionResult> CarStatuses(CancellationToken cancellationToken)
    {
        var statuses = await _carRepository.GetPublicStatusesAsync(cancellationToken);
        return Json(statuses);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
