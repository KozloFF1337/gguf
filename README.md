// Получаем список всех ID станций из объекта stations
const allStationIDs = Object.keys(stations);

// Находим станции, которых нет в рейтинге
const stationsWithRating = ratings.map(r => r.StationID);
const stationsWithoutData = allStationIDs.filter(id => !stationsWithRating.includes(id));

// Добавляем станции без данных в массив рейтинга с rating = null
stationsWithoutData.forEach(stationID => {
    ratings.push({
        StationID: stationID,
        rating: null
    });
});

// Сортируем: сначала станции с рейтингом (по убыванию), затем без рейтинга
ratings.sort((a, b) => {
    // Если у обоих null - не меняем порядок
    if (a.rating === null && b.rating === null) {
        return 0;
    }
    // Если у 'a' null - ставим в конец
    if (a.rating === null) {
        return 1;
    }
    // Если у 'b' null - ставим в конец
    if (b.rating === null) {
        return -1;
    }
    // Оба имеют рейтинг - сортируем по убыванию
    return b.rating - a.rating;
});

// Выбираем контейнер рейтинга
const ratingContainer = d3.select('.rating-container');

// Отрисовка карточек с проверкой наличия данных
ratings.forEach((item, index) => {
    const card = ratingContainer.append('div')
        .attr('class', 'rating-card');
    
    const ratingInfo = card.append('div')
        .attr('class', 'rating-info');
    
    // Для станций с данными показываем номер места
    if (item.rating !== null) {
        ratingInfo.append('span')
            .attr('class', 'rank-number')
            .text(`${index + 1}.`);
    }
    
    // Название станции
    ratingInfo.append('span')
        .attr('class', 'user-name')
        .text(stations[item.StationID]);
    
    // Только для станций с данными: показываем рейтинг и чекбокс
    if (item.rating !== null) {
        // Определяем класс для стиля рейтинга
        const rating = parseFloat(item.rating);
        let scoreClass = 'score-value-0';
        
        if (rating >= 1) scoreClass = 'score-value-5';
        else if (rating >= 0.5) scoreClass = 'score-value-4';
        else if (rating >= -0.5) scoreClass = 'score-value-3';
        else if (rating >= -1) scoreClass = 'score-value-2';
        else scoreClass = 'score-value-1';
        
        ratingInfo.append('span')
            .attr('class', scoreClass)
            .text(rating.toFixed(2));
        
        // Добавляем чекбокс
        const radioBtn = card.append('div')
            .attr('class', 'radio-btn');
        
        radioBtn.append('input')
            .attr('type', 'checkbox')
            .attr('id', `station-${item.StationID}`)
            .attr('name', 'station')
            .attr('value', item.StationID);
    }
    
    // Обработчик клика для перехода по ссылке (только если есть данные)
    if (item.rating !== null && stationLinks[item.StationID]) {
        card.on('click', function() {
            window.open(stationLinks[item.StationID], '_blank');
        });
    } else {
        // Убираем курсор pointer для станций без данных
        card.style('cursor', 'default');
    }
});
