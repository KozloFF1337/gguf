public async Task<List<DateTime>> GetAvailableDates(PeriodType periodType)
    {
        // Получаем все записи и извлекаем уникальные даты на стороне клиента
        // Используем только поле Date напрямую, без .Date свойства в SQL
        var turbins = await _context.Set<Turbin>()
            .Where(t => t.PeriodType == periodType && t.Date != null && t.PeriodValue > 0)
            .Select(t => t.Date!.Value)
            .ToListAsync();

        // Нормализуем даты на стороне клиента, убирая время
        var dates = turbins
            .Select(d => d.Date) // DateTime.Date - убирает время
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        return dates;
    }

public async Task<List<DateTime>> GetAvailableDates(PeriodType periodType)
    {
        // Получаем все записи и извлекаем уникальные даты на стороне клиента
        // Используем только поле Date напрямую, без .Date свойства в SQL
        var boilers = await _context.Set<Boiler>()
            .Where(b => b.PeriodType == periodType && b.Date != null && b.PeriodValue > 0)
            .Select(b => b.Date!.Value)
            .ToListAsync();

        // Нормализуем даты на стороне клиента, убирая время
        var dates = boilers
            .Select(d => d.Date) // DateTime.Date - убирает время
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        return dates;
    }
