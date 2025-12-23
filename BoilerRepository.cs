using System.Collections.Generic;
using System.Threading.Tasks;
using Altair.Models;
using Microsoft.EntityFrameworkCore;

using Altair.Data;
public class BoilerRepository : IBoilerRepository
{
    private readonly BoilerDbContext _context;
    public BoilerRepository(BoilerDbContext context)
    {
        _context = context;
    }

    public async Task<List<Boiler>> GetBoilers(PeriodType periodType)
    {
        // Возвращаем данные с максимальной датой для данного типа периода
        // Фильтруем по PeriodValue > 0 (hours > 0)
        var maxDate = await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType && b.Date != null && b.PeriodValue > 0)
            .MaxAsync(b => (DateTime?)b.Date);

        if (maxDate == null)
        {
            return new List<Boiler>();
        }

        // Сравниваем по году, месяцу и дню чтобы игнорировать время
        return await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType
                && b.Date != null
                && b.Date.Value.Year == maxDate.Value.Year
                && b.Date.Value.Month == maxDate.Value.Month
                && b.Date.Value.Day == maxDate.Value.Day
                && b.PeriodValue > 0)
            .ToListAsync();
    }

    public async Task<List<Boiler>> GetBoilersByDate(PeriodType periodType, DateTime date)
    {
        // Сравниваем только по году, месяцу и дню - игнорируем время
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        return await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType
                && b.Date != null
                && b.Date.Value.Year == year
                && b.Date.Value.Month == month
                && b.Date.Value.Day == day
                && b.PeriodValue > 0)
            .ToListAsync();
    }

    public async Task<List<DateTime>> GetAvailableDates(PeriodType periodType)
    {
        // Получаем все уникальные даты (только дата без времени)
        var dates = await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType && b.Date != null && b.PeriodValue > 0)
            .Select(b => b.Date!.Value.Date) // .Date убирает время
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        return dates;
    }
}
