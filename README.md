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

// ✅ НОВОЕ РЕШЕНИЕ: Создаем объект для быстрого поиска станций с рейтингом
const ratingsMap = {};
ratings.forEach(r => {
    ratingsMap[r.StationID] = true;
});

// ✅ Находим станции без данных - берем все станции и исключаем те, что есть в ratingsMap
const stationsWithoutData = Object.keys(stations)
    .filter(stationID => !ratingsMap[stationID])
    .map(stationID => ({
        StationID: stationID,
        rating: null,
        hasData: false
    }));

// Объединяем: сначала станции с данными, потом без данных
const allRatings = [...ratings, ...stationsWithoutData];
