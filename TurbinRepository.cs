using System.Collections.Generic;
using System.Threading.Tasks;
using Altair.Models;
using Microsoft.EntityFrameworkCore;

using Altair.Data;
public class TurbinRepository : ITurbinRepository
{
    private readonly TurbinDbContext _context;
    public TurbinRepository(TurbinDbContext context)
    {
        _context = context;
    }

    public async Task<List<Turbin>> GetTurbins(PeriodType periodType)
    {
        // Возвращаем данные с максимальной датой для данного типа периода
        // Фильтруем по PeriodValue > 0 (hours > 0)
        var maxDate = await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType && t.Date != null && t.PeriodValue > 0)
            .MaxAsync(t => (DateTime?)t.Date);

        if (maxDate == null)
        {
            return new List<Turbin>();
        }

        // Сравниваем по году, месяцу и дню чтобы игнорировать время
        return await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType
                && t.Date != null
                && t.Date.Value.Year == maxDate.Value.Year
                && t.Date.Value.Month == maxDate.Value.Month
                && t.Date.Value.Day == maxDate.Value.Day
                && t.PeriodValue > 0)
            .ToListAsync();
    }

    public async Task<List<Turbin>> GetTurbinsByDate(PeriodType periodType, DateTime date)
    {
        // Сравниваем только по году, месяцу и дню - игнорируем время
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        return await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType
                && t.Date != null
                && t.Date.Value.Year == year
                && t.Date.Value.Month == month
                && t.Date.Value.Day == day
                && t.PeriodValue > 0)
            .ToListAsync();
    }

    public async Task<List<DateTime>> GetAvailableDates(PeriodType periodType)
    {
        // Получаем все уникальные даты (только дата без времени)
        var dates = await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType && t.Date != null && t.PeriodValue > 0)
            .Select(t => t.Date!.Value.Date) // .Date убирает время
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        return dates;
    }
}
