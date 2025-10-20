using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Altair.Models;

namespace Altair.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataService _dataService;

        public HomeController(ILogger<HomeController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            var turbins = await _dataService.GetTurbins(PeriodType.Week);
            var boilers = await _dataService.GetBoilers(PeriodType.Week);
            var homePage = await _dataService.GetHomePageData();
            var model = new HomeIndexViewModel
            {
                Turbines = turbins,
                Boilers = boilers,
                HomePage = homePage
            };
            return View(model);
        }

        public async Task<IActionResult> Visualisation(PeriodType? selectedPeriod)
        {
            var period = selectedPeriod ?? PeriodType.Week; // По умолчанию неделя
            var turbins = await _dataService.GetTurbins(period);
            var boilers = await _dataService.GetBoilers(period);
            var model = new VisualisationViewModel
            {
                Turbins = turbins,
                Boilers = boilers,
                SelectedPeriod = period,
            };
            return View(model);
        }

        public IActionResult Contacts() => View();
        public IActionResult Params() => View();
        public IActionResult Optimizator() => View();
        public IActionResult QuicklyNotification() => View();
        public IActionResult NewService() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}