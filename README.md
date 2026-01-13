// Функция для вычисления рейтинга
function calculateRating(group) {
    const sumUrtPercent = group.reduce((acc, curr) => {
        const value = curr.urt_percent !== undefined ? curr.urt_percent : 0;
        return acc + parseFloat(value) * curr.size * -1;
    }, 0);

    const sumKpdPercent = group.reduce((acc, curr) => {
        const value = curr.kpd_percent !== undefined ? curr.kpd_percent : 0;
        return acc + parseFloat(value) * curr.size;
    }, 0);

    const totalSum = group.reduce((acc, curr) => acc + curr.size, 0);

    // Определяем StationID из группы
    const stationId = group.length > 0 ? group[0].station : null;
    
    // Если галочка включена и это ТЭЦ - не умножаем на 2 (только котлы)
    const isTec = tecStations.includes(parseInt(stationId));
    const multiplier = (excludeTecTurbinesFlag && isTec) ? 1 : 2;

    // Проверка на случай, если totalSum равен 0
    return totalSum !== 0 ? (sumUrtPercent + sumKpdPercent) * multiplier / totalSum : 0;
}
