public async Task<List<Turbin>> GetTurbinsByDate(PeriodType periodType, DateTime date)
    {
        // Конвертируем дату в только дату (без времени) для сравнения
        var dateOnly = date.Date;

        // Фильтруем по PeriodValue > 0 (hours > 0)
        // Используем сравнение по дате без учёта времени
        return await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType
                && t.Date.HasValue
                && t.Date.Value.Year == dateOnly.Year
                && t.Date.Value.Month == dateOnly.Month
                && t.Date.Value.Day == dateOnly.Day
                && t.PeriodValue > 0)
            .ToListAsync();
    }

    public async Task<List<Boiler>> GetBoilersByDate(PeriodType periodType, DateTime date)
    {
        // Конвертируем дату в только дату (без времени) для сравнения
        var dateOnly = date.Date;

        // Фильтруем по PeriodValue > 0 (hours > 0)
        // Используем сравнение по дате без учёта времени
        return await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType
                && b.Date.HasValue
                && b.Date.Value.Year == dateOnly.Year
                && b.Date.Value.Month == dateOnly.Month
                && b.Date.Value.Day == dateOnly.Day
                && b.PeriodValue > 0)
            .ToListAsync();
    }
