// ✅ НОВОЕ: Переменная для отслеживания выбранной станции
let selectedStationID = null;

// Генерируем карточки рейтинга с обработкой станций без данных
allRatings.forEach((rating, index) => {
    const cardDiv = ratingContainer.append('div')
        .attr('class', 'rating-card');
    
    const ratingInfo = cardDiv.append('div')
        .attr('class', 'rating-info')
        .on('click', function() {
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
    
    ratingInfo.append('span')
        .attr('class', selectColorForRating(rating.rating, rating.hasData))
        .text(rating.hasData ? `${rating.rating.toFixed(2)}%` : 'Нет данных');
    
    // Правая часть - радиокнопка (только для станций с данными)
    if (rating.hasData) {
        const radioDiv = cardDiv.append('div')
            .attr('class', 'radio-btn')
            .on('click', function(e) {
                e.stopPropagation();
                
                // ✅ НОВОЕ: Проверяем повторный клик
                if (selectedStationID === rating.StationID) {
                    // Повторный клик - снимаем выделение
                    selectedStationID = null;
                    d3.selectAll('input[name="station"]').property('checked', false);
                    resetHighlight();
                } else {
                    // Новая станция - выделяем
                    selectedStationID = rating.StationID;
                    highlightStation(rating.StationID);
                    d3.selectAll('input[name="station"]').property('checked', false);
                    d3.select(this).select('input').property('checked', true);
                }
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
        selectedStationID = null; // ✅ ДОБАВЛЕНО: Сброс переменной
        resetHighlight();
        d3.selectAll('input[name="station"]').property('checked', false);
    }
});
