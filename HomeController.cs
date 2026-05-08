using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Altair.Models;
using Altair.Services;

namespace Altair.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataService _dataService;
        private readonly ITechParamsService _techParamsService;
        private readonly IBoilerRatingParamsService _boilerRatingParamsService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IDataService dataService, ITechParamsService techParamsService, IBoilerRatingParamsService boilerRatingParamsService, IConfiguration configuration)
        {
            _logger = logger;
            _dataService = dataService;
            _techParamsService = techParamsService;
            _boilerRatingParamsService = boilerRatingParamsService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(PeriodType? selectedPeriod, DateTime? selectedDate)
        {
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            // По умолчанию месяц вместо дня
            var period = selectedPeriod ?? PeriodType.Month;

            List<Turbin> turbins;
            List<Boiler> boilers;

            // Если дата не указана и это первое открытие - получаем последний доступный месяц
            if (!selectedDate.HasValue && !selectedPeriod.HasValue)
            {
                var availableMonths = await _dataService.GetAvailableMonths();
                if (availableMonths.Any())
                {
                    selectedDate = availableMonths.First(); // Первый = самый последний (отсортировано по убыванию)
                }
            }

            if (selectedDate.HasValue)
            {
                turbins = await _dataService.GetTurbinsByDate(period, selectedDate.Value);
                boilers = await _dataService.GetBoilersByDate(period, selectedDate.Value);
            }
            else
            {
                turbins = await _dataService.GetTurbins(period);
                boilers = await _dataService.GetBoilers(period);
            }

            double reservesRub;
            Dictionary<int, double> reservesRubByStation;
            if (selectedDate.HasValue)
            {
                reservesRub = await _dataService.GetReservesRubByDate(period, selectedDate.Value);
                reservesRubByStation = await _dataService.GetReservesRubByStation(period, selectedDate.Value);
            }
            else
            {
                reservesRub = await _dataService.GetReservesRub(period);
                reservesRubByStation = await _dataService.GetReservesRubByStation(period, null);
            }

            // Цена ТУТ по станциям — передаётся на фронт для расчёта рублей как ТУТ × цена.
            // Пустой словарь для Year — годовые рубли считаются как SUM месячных из БД.
            var priceByStation = await _dataService.GetPriceByStation(period, selectedDate);

            // Для годового периода: рубли вычисляются из raw_turbins_monthly / raw_boilers_monthly напрямую.
            // Год берётся из выбранной даты, а если дата не указана — из последних загруженных данных турбин,
            // чтобы гарантировать совпадение с отображаемым периодом.
            int selectedYear = selectedDate.HasValue
                ? selectedDate.Value.Year
                : (turbins.Any() && turbins[0].Date.HasValue ? turbins[0].Date!.Value.Year : DateTime.Now.Year);
            var (yearlyRubByStationFromMonthly, yearlyTurbRubByStation) = await _dataService.GetYearlyRubByStationFromMonthly(selectedYear);
            var (yearlyTutByStation, yearlyTurbTutByStation) = await _dataService.GetYearlyTutByStationFromMonthly(selectedYear);

            // Данные для графика по месяцам (выбранный год и предыдущий).
            // Используем selectedYear, чтобы месячный график совпадал с годовым итогом на KPI-карточке.
            var (turbinesCurrent, boilersCurrent, totalCurrent, tecTurbinesCurrent) = await _dataService.GetMonthlyReservesRubByYear(selectedYear);
            var (turbinesPrev, boilersPrev, totalPrev, tecTurbinesPrev) = await _dataService.GetMonthlyReservesRubByYear(selectedYear - 1);
            var monthlyByStation     = await _dataService.GetMonthlyRubByStationByYear(selectedYear);
            var monthlyByStationPrev = await _dataService.GetMonthlyRubByStationByYear(selectedYear - 1);
            var monthlyTutByStation     = await _dataService.GetMonthlyTutByStationByYear(selectedYear);
            var monthlyTutByStationPrev = await _dataService.GetMonthlyTutByStationByYear(selectedYear - 1);

            var model = new HomeIndexViewModel
            {
                Turbines = turbins,
                Boilers = boilers,
                SelectedPeriod = period,
                SelectedDate = selectedDate,
                ReservesRub = reservesRub,
                ReservesRubByStation = reservesRubByStation,
                PriceByStation = priceByStation,
                YearlyRubByStationFromMonthly = yearlyRubByStationFromMonthly,
                YearlyTurbRubByStation = yearlyTurbRubByStation,
                MonthlyTurbinesRub = turbinesCurrent,
                MonthlyBoilersRub = boilersCurrent,
                MonthlyTotalRub = totalCurrent,
                MonthlyTurbinesRubPrev = turbinesPrev,
                MonthlyBoilersRubPrev = boilersPrev,
                MonthlyTotalRubPrev = totalPrev,
                MonthlyTecTurbinesRub = tecTurbinesCurrent,
                MonthlyTecTurbinesRubPrev = tecTurbinesPrev,
                MonthlyRubByStation     = monthlyByStation,
                MonthlyRubByStationPrev = monthlyByStationPrev,
                YearlyTutByStation = yearlyTutByStation,
                YearlyTurbTutByStation = yearlyTurbTutByStation,
                MonthlyTutByStation = monthlyTutByStation,
                MonthlyTutByStationPrev = monthlyTutByStationPrev
            };
            ViewBag.QLossInDevelopment = _configuration.GetValue<bool>("FeatureFlags:QLossInDevelopment");
            return View(model);
        }

        public async Task<IActionResult> Visualisation(PeriodType? selectedPeriod, DateTime? selectedDate)
        {
            // По умолчанию месяц вместо дня
            var period = selectedPeriod ?? PeriodType.Month;

            List<Turbin> turbins;
            List<Boiler> boilers;

            // Если дата не указана и это первое открытие - получаем последний доступный месяц
            if (!selectedDate.HasValue && !selectedPeriod.HasValue)
            {
                var availableMonths = await _dataService.GetAvailableMonths();
                if (availableMonths.Any())
                {
                    selectedDate = availableMonths.First(); // Первый = самый последний (отсортировано по убыванию)
                }
            }

            if (selectedDate.HasValue)
            {
                turbins = await _dataService.GetTurbinsByDate(period, selectedDate.Value);
                boilers = await _dataService.GetBoilersByDate(period, selectedDate.Value);
            }
            else
            {
                turbins = await _dataService.GetTurbins(period);
                boilers = await _dataService.GetBoilers(period);
            }

            var model = new VisualisationViewModel
            {
                Turbins = turbins,
                Boilers = boilers,
                SelectedPeriod = period,
                SelectedDate = selectedDate,
                TechParams = _techParamsService.GetData(),
                BoilerRatingParamsHistory = _boilerRatingParamsService.GetHistory()
            };
            return View(model);
        }

        public IActionResult Contacts() => View();
        public IActionResult Params() => View();

        public IActionResult RepairAnalysis()
        {
            var records = new List<RepairAnalysisRecord>();

            // Котлы — КПД
            foreach (var stationId in NormativeValues.KpdMeta.Keys.OrderBy(k => k))
            {
                string stationName = NormativeValues.StationNames.TryGetValue(stationId, out var sn) ? sn : stationId.ToString();
                foreach (var kvp in NormativeValues.KpdMeta[stationId].OrderBy(k => k.Key))
                {
                    string code = kvp.Key;
                    var (date, note) = kvp.Value;
                    double normValue = NormativeValues.KpdValues.TryGetValue(stationId, out var kd) && kd.TryGetValue(code, out var v) ? v : 0;
                    records.Add(new RepairAnalysisRecord
                    {
                        StationName = stationName,
                        EquipmentType = "Котёл",
                        EquipmentCode = code,
                        NormValue = normValue,
                        Unit = "%",
                        RepairType = ExtractRepairType(note),
                        RepairDate = date
                    });
                }
            }

            // Турбины — УРТ
            foreach (var stationId in NormativeValues.UrtMeta.Keys.OrderBy(k => k))
            {
                string stationName = NormativeValues.StationNames.TryGetValue(stationId, out var sn) ? sn : stationId.ToString();
                foreach (var kvp in NormativeValues.UrtMeta[stationId].OrderBy(k => k.Key))
                {
                    string code = kvp.Key;
                    var (date, note) = kvp.Value;
                    double normValue = NormativeValues.UrtValues.TryGetValue(stationId, out var ud) && ud.TryGetValue(code, out var v) ? v : 0;
                    records.Add(new RepairAnalysisRecord
                    {
                        StationName = stationName,
                        EquipmentType = "Турбина",
                        EquipmentCode = code,
                        NormValue = normValue,
                        Unit = "ккал/кВт·ч",
                        RepairType = ExtractRepairType(note),
                        RepairDate = date
                    });
                }
            }

            // Нумеруем после сортировки
            for (int i = 0; i < records.Count; i++)
                records[i].RowNumber = i + 1;

            return View(records);
        }

        private static string ExtractRepairType(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return "-";
            if (note.Contains("КР")) return "КР";
            if (note.Contains("СР")) return "СР";
            if (note.Contains("ТР")) return "ТР";
            return "-";
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentJson()
        {
            var (turbines, boilers) = await _dataService.GetDistinctEquipment();

            var turbinList = turbines
                .Select(t => new { stationId = t.StationID, turbinId = t.TurbinID })
                .ToList();

            var boilerList = boilers
                .Select(b => new { stationId = b.StationID, boilerId = b.BoilerID })
                .ToList();

            return Json(new { turbines = turbinList, boilers = boilerList });
        }
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