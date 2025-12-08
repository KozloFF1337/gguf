// Форматируем данные в иерархическом виде
const hierarchicalData_turbin = {
    name: "Turbines",
    children: turbinsData.map(item => {
        // ✅ ИЗМЕНЕНО: Сначала пытаемся взять NominalURT из БД
        let nominalValue = item.NominalURT || 0;
        
        // ✅ Если значение из БД равно 0, берем из массива нормативов (fallback)
        if (nominalValue === 0) {
            const stationUrtValues = urtvalues[item.StationID];
            const turbinKey = parseInt(item.TurbinID);
            
            if (stationUrtValues && stationUrtValues[turbinKey] && stationUrtValues[turbinKey] > 0) {
                nominalValue = stationUrtValues[turbinKey];
            } else {
                // Если и в массиве нет данных, используем дефолтное значение
                nominalValue = 2300;
            }
        }
        
        return {
            urt: item.URT,
            size: item.Consumption,
            station: item.StationID,
            turbin: item.TurbinID,
            urt_percent: (((item.URT - nominalValue) / item.URT) * 100),
            urt_percent_normal: nominalValue
        };
    })
};
