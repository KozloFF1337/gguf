// ... (весь предыдущий код остается без изменений до создания рейтинга)

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
    hasData: true  // ✅ Маркер наличия данных
}));

// Сортируем рейтинг по значению
ratings.sort((a, b) => b.rating - a.rating);

// ✅ НОВОЕ: Находим станции без данных
const stationsWithData = new Set(ratings.map(r => r.StationID));
const allStationIDs = Object.keys(stations);

const stationsWithoutData = allStationIDs
    .filter(stationID => !stationsWithData.has(stationID))
    .map(stationID => ({
        StationID: stationID,
        rating: null,  // Нет рейтинга
        hasData: false  // Нет данных
    }));

// ✅ Объединяем: сначала станции с данными, потом без данных
const allRatings = [...ratings, ...stationsWithoutData];

// Выбираем контейнер рейтинга
const ratingContainer = d3.select('.rating-container');

function selectColorForRating(rt, hasData) {
    if (!hasData) return 'score-value-0'; // ✅ Серый для станций без данных
    if (rt < 0) return 'score-value-3'; // Красный
    if (rt >= 0) return 'score-value-4'; // Светло-зеленый
    return 'score-value-0'; // Серый по умолчанию
}

// Обновленные функции highlightStation и resetHighlight
function highlightStation(StationID) {
    // Затемняем все элементы и текст
    d3.selectAll(".node rect")
        .style("opacity", 0.3);
    d3.selectAll(".node text")
        .style("opacity", 0.3);

    // Подсвечиваем элементы выбранной станции
    d3.selectAll(".node")
        .filter(d => d.data && d.data.station == StationID)
        .select("rect")
        .style("opacity", 1);

    d3.selectAll(".node")
        .filter(d => d.data && d.data.station == StationID)
        .selectAll("text")
        .style("opacity", 1);
}

function resetHighlight() {
    // Возвращаем нормальную прозрачность всем элементам
    d3.selectAll(".node rect, .node text")
        .style("opacity", 1);
}

// ✅ ОБНОВЛЕНО: Генерируем карточки рейтинга с обработкой станций без данных
allRatings.forEach((rating, index) => {
    const cardDiv = ratingContainer.append('div')
        .attr('class', 'rating-card');
    
    // Левая часть - информация о станции
    const ratingInfo = cardDiv.append('div')
        .attr('class', 'rating-info')
        .on('click', function() {
            // ✅ Переход работает для всех станций (с данными и без)
            if (stationLinks[rating.StationID]) {
                window.open(stationLinks[rating.StationID], '_blank');
            }
        });
    
    ratingInfo.append('span')
        .attr('class', 'rank-number')
        .text(index + 1 + '.');
    
    ratingInfo.append('span')
        .attr('class', 'user-name')
        .text(stations[rating.StationID]);
    
    // ✅ ИЗМЕНЕНО: Отображаем "Нет данных" или рейтинг
    ratingInfo.append('span')
        .attr('class', selectColorForRating(rating.rating, rating.hasData))
        .text(rating.hasData ? `${rating.rating.toFixed(2)}%` : 'Нет данных');
    
    // Правая часть - радиокнопка (только для станций с данными)
    if (rating.hasData) {
        const radioDiv = cardDiv.append('div')
            .attr('class', 'radio-btn')
            .on('click', function(e) {
                e.stopPropagation();
                highlightStation(rating.StationID);
                // Помечаем выбранную радиокнопку
                d3.selectAll('input[name="station"]').property('checked', false);
                d3.select(this).select('input').property('checked', true);
            });
        
        radioDiv.append('input')
            .attr('type', 'radio')
            .attr('name', 'station')
            .attr('id', `station-${rating.StationID}`);
    }
}); 

// Сбрасываем выделение при клике вне карточек
document.addEventListener('click', function(e) {
    if (!e.target.closest('.rating-card')) {
        resetHighlight();
        d3.selectAll('input[name="station"]').property('checked', false);
    }
});

