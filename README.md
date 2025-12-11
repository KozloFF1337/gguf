// Группируем данные по StationID
const groupedData = {};
combinedData.forEach(item => {
    if (!groupedData[item.station]) {
        groupedData[item.station] = [];
    }
    groupedData[item.station].push(item);
});

// Создаем массив объектов для рейтинга
const ratings = Object.keys(groupedData).map(key => ({
    StationID: key,
    rating: calculateRating(groupedData[key]),
    hasData: true
}));

// Сортируем рейтинг по значению
ratings.sort((a, b) => b.rating - a.rating);

// ✅ ИСПРАВЛЕНО: Приводим типы к строкам для корректного сравнения
const stationsWithData = new Set(ratings.map(r => String(r.StationID)));
const allStationIDs = Object.keys(stations);

const stationsWithoutData = allStationIDs
    .filter(stationID => !stationsWithData.has(stationID))
    .map(stationID => ({
        StationID: stationID,
        rating: null,
        hasData: false
    }));

// Объединяем: сначала станции с данными, потом без данных
const allRatings = [...ratings, ...stationsWithoutData];
