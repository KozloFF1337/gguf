@using Newtonsoft.Json;
@using Altair.Services;

@{
    ViewData["Title"] = "Альтаир Рейтинг";
}

@model Altair.Models.VisualisationViewModel

<head>
    <meta charset="UTF-8">
    <title>Treemap Visualization</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.8.5/d3.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/xlsx-js-style@1.2.0/dist/xlsx.bundle.js"></script>
    <style>
        .rating-container {
        position: fixed;
        top: 60px;
        right: 0;
        bottom: 45px;
        width: 200px;
        padding: 8px;
        padding-top: 0;
        background-color: #222;
        color: white;
        border-radius: 8px;
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
        font-family: sans-serif;
        line-height: 1.2;
        overflow: hidden;
        z-index: 100;
        }
    body {
        background-color: black;
        color: white;
        font-family: sans-serif;
        }
    .rating-card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.32em;
    padding: 0.45em;
    background-color: #333;
    border-radius: 4px;
    box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2);
    transition: transform 0.3s ease;
    cursor: pointer;
    }
    .rating-info {
        display: flex;
        align-items: center;
        flex-grow: 1;
        overflow: hidden;
        white-space: nowrap;
    }
    .radio-btn {
        margin-left: 0.5em;
        cursor: pointer;
        transform: scale(1);
        transform-origin: center;
    }
    .radio-btn input {
        cursor: pointer;
        width: 1em;
        height: 1em;
    }
    .toggle-all-btn {
        background: #444;
        border: 1px solid #666;
        border-radius: 4px;
        color: #ddd;
        padding: 0.1em 0.4em;
        font-size: 0.9em;
        cursor: pointer;
        transition: all 0.2s ease;
    }
    .toggle-all-btn:hover {
        background: #555;
        border-color: #888;
    }
    .rating-card:nth-child(even) {
        background-color: #2a2a2a;
    }
    .rank-number {
        font-weight: bold;
        color: white;
        font-size: 1em;
        margin-right: 0.5em;
    }
    .user-name {
        font-style: normal;
        font-size: 1em;
        color: #dddddd;
    }
    .score-value-0 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #5d5d5d;
    }
    .score-value-1 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #8B0000;
    }
    .score-value-2 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #c5172c;
    }
    .score-value-3 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #FF0000;
    }
    .score-value-4 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #009F00;
    }
    .score-value-5 {
        margin-left: 0.25em;
        font-size: 1em;
        font-weight: bold;
        color: #006400;
    }    
    .tooltip {
    position: absolute;
    background-color: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 8px;
    z-index: 1000;
    opacity: 0;
    transition: opacity 0.01s ease-in-out;
    pointer-events: none;
    }
    #sidebar {
        grid-column-start: span 1;
        align-self: start;
        width: calc(10% + 20px);
        margin-left: 10px;
    }
    .rating-card:hover {
        transform: scale(1.02);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
        background-color: #444 !important;
    }

    .main-container {
    display: flex;
    }
    .graph-container {
            width: 100%;    
            text-align: center; /* Центрируем содержимое */    
            box-sizing: border-box;    
            margin-left: 4px;
            }

    h2 {
            color: #dddddd;
            font-size: 30px;
            margin-top: 0;
            margin-bottom: 4px;
            display: inline-block; /* Чтобы text-align:center работал правильно */
            }

    .node rect {
        stroke-width: 1px;
        fill-opacity: 1;
    }
    
    text.centered-text {
        font-size: 16px;
        dominant-baseline: middle;
        pointer-events: none;
        text-anchor: middle;
    }
    select {
    width: 90px;
    height: 40px;
    border: 1px solid #444;
    border-radius: 5px;
    background-color: #333;
    color: #dddddd;
    font-size: 16px;
    text-align: center;
    padding: 0 0px;
    cursor: pointer;
    transition: background-color 0.3s ease-in-out, color 0.3s ease-in-out;
    }

    select:hover {
        background-color: #444;
        color: #fd817e;
    }

    select:focus {
        outline: none;
        box-shadow: 0 0 5px rgba(253, 129, 126, 0.5);
    }

    </style>
</head>
<body>
    <!-- Вкладки период и дата — фиксированная панель поверх всего -->
    <div id="period-bar-backdrop" style="position: fixed; top: 60px; left: 0; right: 0; z-index: 50; background-color: #000; height: calc(7vh); box-sizing: border-box;"></div>
    <div id="period-bar" style="position: fixed; top: 65px; left: 0; right: 210px; z-index: 51; display: flex; justify-content: center; align-items: center; gap: 15px; padding: 10px; background-color: #222; border-radius: 10px; box-sizing: border-box;">
        <div style="display: flex; gap: 10px; align-items: center;">
            <span style="color: #dddddd; font-size: 16px; font-weight: bold;">Период:</span>
            <select id="periodSelect" onchange="onPeriodChange()">
                <option value="day">День</option>
                <option value="month">Месяц</option>
                <option value="year">Год</option>
            </select>
        </div>
        <div style="display: flex; gap: 10px; align-items: center;">
            <span style="color: #dddddd; font-size: 16px; font-weight: bold;">Дата:</span>
            <select id="dateSelect" onchange="reloadPage()" style="min-width: 150px;"></select>
        </div>
    </div>

    <div style="margin-top: 60px; margin-right: 210px;">
        <div style="flex: 1; display: flex; flex-direction: column;">
            <div class="graph-container" id="treemap-left" style="text-align: center;">
                <h2 style="margin-bottom: 4px; display: inline-block;">Котлы</h2>
            </div>
            <div class="graph-container" id="treemap-right" style="text-align: center;">
                <h2 style="margin-bottom: 4px; display: inline-block;">Турбины</h2>
            </div>
        </div>
        <div id="kpd-scatter-row" style="display: flex; gap: 10px; margin-top: 20px; align-items: flex-start;">
            <!-- Рейтинг котлов слева -->
            <div id="boiler-rating-panel" style="width: 228px; flex-shrink: 0; background-color: #222; border-radius: 10px; padding: 8px; display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box;">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; margin-top: 3px; flex-shrink: 0;">
                    <span style="color: #ccc; font-size: 14px; font-weight: 600;">Рейтинг котлов:</span>
                    <button onclick="exportBoilerRating()" title="Выгрузить в Excel" style="background: none; border: 1px solid #555; border-radius: 4px; color: #aaa; font-size: 11px; padding: 2px 6px; cursor: pointer; line-height: 1.4;">xlsx</button>
                </div>
                <div id="boiler-rating-list" style="overflow-y: auto; overflow-x: hidden; flex: 1;"></div>
            </div>
            <!-- График -->
            <div id="kpd-scatter-container" style="flex: 1; background-color: #222; border-radius: 10px; padding: 15px 15px 5px 15px; min-width: 0;">
                <div style="color: #ccc; font-size: 15px; font-weight: 600; margin-bottom: 10px;">КПД сравнительный анализ</div>
                <svg id="kpd-scatter-svg" style="display: block; width: 100%;"></svg>
            </div>
        </div>
        <div id="urt-scatter-row" style="display: flex; gap: 10px; margin-top: 20px; align-items: flex-start;">
            <!-- Рейтинг турбин слева -->
            <div id="turbine-rating-panel" style="width: 228px; flex-shrink: 0; background-color: #222; border-radius: 10px; padding: 8px; display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box;">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; margin-top: 3px; flex-shrink: 0;">
                    <span style="color: #ccc; font-size: 14px; font-weight: 600;">Рейтинг турбин:</span>
                    <button onclick="exportTurbineRating()" title="Выгрузить в Excel" style="background: none; border: 1px solid #555; border-radius: 4px; color: #aaa; font-size: 11px; padding: 2px 6px; cursor: pointer; line-height: 1.4;">xlsx</button>
                </div>
                <div id="turbine-rating-list" style="overflow-y: auto; overflow-x: hidden; flex: 1;"></div>
            </div>
            <!-- График -->
            <div id="urt-scatter-container" style="flex: 1; background-color: #222; border-radius: 10px; padding: 15px 15px 5px 15px; min-width: 0;">
                <div style="color: #ccc; font-size: 15px; font-weight: 600; margin-bottom: 10px;">УРТ сравнительный анализ</div>
                <svg id="urt-scatter-svg" style="display: block; width: 100%;"></svg>
            </div>
        </div>
    </div>
    <div class="rating-container" id="rating-container-vis">
        <div class="rating-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; margin-top: 5px;">
            <h2 style="margin: 0; font-size: 14px;">Рейтинг:</h2>
            <button id="toggle-all-vis" class="toggle-all-btn" title="Выбрать/Снять все" style="font-size: 14px; padding: 2px 8px;">☑</button>
        </div>
        <div class="rating-cards-wrapper" id="rating-cards-vis"></div>
    </div>

    <div id="tooltip" class="tooltip"></div>

    <script>
    // Текущее состояние из модели
    const currentPeriodType = @((int)Model.SelectedPeriod);
    const currentSelectedDate = '@(Model.SelectedDate?.ToString("yyyy-MM-dd") ?? "")';

    const periodSelect = document.getElementById('periodSelect');
    const dateSelect = document.getElementById('dateSelect');

    // Устанавливаем текущий тип периода
    if (currentPeriodType === 0) periodSelect.value = 'day';
    else if (currentPeriodType === 1) periodSelect.value = 'month';
    else if (currentPeriodType === 2) periodSelect.value = 'year';

    // Загружаем доступные даты при загрузке страницы
    document.addEventListener('DOMContentLoaded', () => loadAvailableDates());

    async function loadAvailableDates() {
        const period = periodSelect.value;
        let url = '';

        if (period === 'day') {
            url = '/api/Data/available-days';
        } else if (period === 'month') {
            url = '/api/Data/available-months';
        } else {
            url = '/api/Data/available-years';
        }

        try {
            const response = await fetch(url);
            const data = await response.json();

            dateSelect.innerHTML = '';

            if (period === 'year') {
                // Для года показываем только годы с данными из API
                if (data.years && data.years.length > 0) {
                    data.years.forEach(y => {
                        const opt = document.createElement('option');
                        opt.value = y.date;
                        opt.textContent = y.displayText;
                        dateSelect.appendChild(opt);
                    });
                }
            } else {
                if (data.dates) {
                    data.dates.forEach(d => {
                        const option = document.createElement('option');
                        option.value = d.date;
                        option.textContent = d.displayText;
                        dateSelect.appendChild(option);
                    });
                }
            }

            if (currentSelectedDate && dateSelect.querySelector(`option[value="${currentSelectedDate}"]`)) {
                dateSelect.value = currentSelectedDate;
            }
        } catch (error) {
            console.error('Ошибка загрузки дат:', error);
        }
    }

    function onPeriodChange() {
        loadAvailableDates().then(() => {
            if (dateSelect.options.length > 0) {
                reloadPage();
            }
        });
    }

    function reloadPage() {
        const selectedPeriod = periodSelect.value;
        const selectedDate = dateSelect.value;
        // Сохраняем в localStorage для синхронизации между вкладками
        localStorage.setItem('selectedPeriod', selectedPeriod);
        if (selectedDate) {
            localStorage.setItem('selectedDate', selectedDate);
        } else {
            localStorage.removeItem('selectedDate');
        }
        let url = `@Url.Action("Visualisation", "Home")?selectedPeriod=${selectedPeriod}`;
        if (selectedDate) {
            url += `&selectedDate=${selectedDate}`;
        }
        window.location.href = url;
    }

    </script>

    <script>
        const stations = {
            25: `РефГРЭС`,   // для Рефтинской
            9: `ТуГРЭС`,    // для Томь-Усинской
            15: `БелГРЭС`,   // для Беловской
            1: `НазГРЭС`,    // для Назаровской
            24: `КрГРЭС-2`,   // для Красноярской-2
            26: `ПрГРЭС`,    // для Приморской
            14: `КемТЭЦ`,
            3: `НкТЭЦ`,
            6: `БарТЭЦ-2`,
            7: `БарТЭЦ-3`,
            22: `БиТЭЦ`,
            4: `КрТЭЦ-1`,
            2: `КрТЭЦ-2`,
            12: `КрТЭЦ-3`,
            13: `КанТЭЦ`,
            8: `АбТЭЦ`,
            10: `МинТЭЦ`,
            17: `НТЭЦ-2`,
            18: `НТЭЦ-3`,
            19: `НТЭЦ-4`,
            20: `НТЭЦ-5`,
            21: `БбТЭЦ`,
            5: `КемГРЭС`
        };

        // Станции типа ТЭЦ + КемГРЭС (для исключения турбин из рейтинга)
        const tecStations = [14, 3, 6, 7, 22, 4, 2, 12, 13, 8, 10, 17, 18, 19, 20, 21, 5];

        // Читаем настройку исключения турбин ТЭЦ из localStorage (по умолчанию true)
        const excludeTecFromUrl = localStorage.getItem('excludeTecTurbines') !== 'false';
        const excludeTecTurbinesFlag = excludeTecFromUrl;

        // Читаем состояние выбора оборудования из localStorage
        const _eqStateRaw = (() => {
            try { return JSON.parse(localStorage.getItem('equipmentSelectionState') || '{}'); }
            catch(e) { return {}; }
        })();
        const eqTurbines = _eqStateRaw.turbines || {};
        const eqBoilers  = _eqStateRaw.boilers  || {};

        const stationLinks = {
            25: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%A0%D0%B5%D1%84%D0%93%D0%A0%D0%AD%D0%A1',   // для Рефтинской
            9: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%A2%D0%A3%D0%93%D0%A0%D0%AD%D0%A1',    // для Томь-Усинской
            15: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%91%D0%B5%D0%BB%D0%93%D0%A0%D0%AD%D0%A1',   // для Беловской
            1: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9D%D0%93%D0%A0%D0%AD%D0%A1',    // для Назаровской
            24: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D0%93%D0%A0%D0%AD%D0%A1-2',   // для Красноярской-2
            26: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9F%D0%A0%D0%93%D0%A0%D0%AD%D0%A1',    // для Приморской
            14: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D0%B5%D0%BC%D0%A2%D0%AD%D0%A6',
            3: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9D%D0%9A%D0%A2%D0%AD%D0%A6',
            6: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%90%D0%BB%D1%82%D0%B0%D0%B9%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%91%D0%B0%D1%80%D0%A2%D0%AD%D0%A6-2',
            7: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%90%D0%BB%D1%82%D0%B0%D0%B9%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%91%D0%B0%D1%80%D0%A2%D0%AD%D0%A6-3',
            22: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%90%D0%BB%D1%82%D0%B0%D0%B9%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%91%D0%B8%D0%B9%D0%A2%D0%AD%D0%A6',
            4: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D1%80%D0%A2%D0%AD%D0%A6-1',
            2: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D1%80%D0%A2%D0%AD%D0%A6-2',
            12: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D1%80%D0%A2%D0%AD%D0%A6-3',
            13: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D0%B0%D0%BD%D0%A2%D0%AD%D0%A6',
            8: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%90%D0%B1%D0%A2%D0%AD%D0%A6',
            10: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D1%8F%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9C%D0%B8%D0%BD%D0%A2%D0%AD%D0%A6',
            17: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9D%D0%BE%D0%B2%D0%BE%D1%81%D0%B8%D0%B1%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9D%D0%A2%D0%AD%D0%A6-2',
            18: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9D%D0%BE%D0%B2%D0%BE%D1%81%D0%B8%D0%B1%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9D%D0%A2%D0%AD%D0%A6-3',
            19: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9D%D0%BE%D0%B2%D0%BE%D1%81%D0%B8%D0%B1%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9D%D0%A2%D0%AD%D0%A6-4',
            20: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9D%D0%BE%D0%B2%D0%BE%D1%81%D0%B8%D0%B1%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9D%D0%BE%D0%B2%D0%A2%D0%AD%D0%A6-5',
            21: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9D%D0%BE%D0%B2%D0%BE%D1%81%D0%B8%D0%B1%D0%B8%D1%80%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%91%D0%B1%D0%A2%D0%AD%D0%A6',
            5: 'https://powerbi-rs.suek.ru/reports/powerbi/Analytics/%D0%9F%D0%B5%D1%80%D0%B5%D0%B6%D0%BE%D0%B3%D0%B8/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0/%D0%9A%D1%83%D0%B7%D0%B1%D0%B0%D1%81%D1%81%D0%BA%D0%B8%D0%B9%20%D1%84%D0%B8%D0%BB%D0%B8%D0%B0%D0%BB/%D0%9A%D0%BE%D0%BD%D1%82%D1%80%D0%BE%D0%BB%D1%8C%20%D1%80%D0%B5%D0%B6%D0%B8%D0%BC%D0%B0%20%D0%9A%D0%B5%D0%BC%D0%93%D0%A0%D0%AD%D0%A1'
        };

        // ============ НОРМАТИВНЫЕ ЗНАЧЕНИЯ КПД И УРТ ============
        // Загружаются динамически из файла normative_config.xlsx через сервис NormativeValues
        // Для изменения значений используйте страницу Параметры -> Нормативные значения КПД и УРТ

        const kpdvalues = @Html.Raw(NormativeValues.GetKpdValuesAsJson());

        const urtvalues = @Html.Raw(NormativeValues.GetUrtValuesAsJson());

        // Метаданные КПД: дата КР/СР и примечание для каждого котла
        const kpdmeta = @Html.Raw(NormativeValues.GetKpdMetaAsJson());

        // Метаданные УРТ: дата КР/СР и примечание для каждой турбины
        const urtmeta = @Html.Raw(NormativeValues.GetUrtMetaAsJson());

        // ============ КОНЕЦ БЛОКА НОРМАТИВНЫХ ЗНАЧЕНИЙ ============

        // История файлов рейтинга котлов: [0]=последний, [1]=предыдущий, [2]=самый старый
        const techParamsHistory = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(
            Model.BoilerRatingParamsHistory.Select(d =>
                d.GresRecords.Concat(d.TecRecords).ToList()
            ).ToList()
        ));

        // Берём данные из C# и превращаем их в пригодный для D3.js формат
    const turbinsData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Turbins));
    const boilersData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Boilers));
        // Форматируем данные в иерархическом виде
    const hierarchicalData_turbin = {
        name: "Turbines",
        children: turbinsData.map(item => {
            // NominalURT из БД: если АСТЭП передал nominal_urt > 0, то уже = nominal_urt + variation
            // Если не передал — хранится 0, надо взять из normative_config и добавить variation
            let nominalValue = item.NominalURT || 0;
            const variation = item.Variation || 0;

            // NominalURT < 500 → АСТЭП не дал nominal_urt (реальный УРТ всегда > 700 г/кВт·ч)
            // Берём базу из normative_config и прибавляем variation
            if (nominalValue < 500) {
                const stationUrtValues = urtvalues[item.StationID];
                const turbinKey = parseInt(item.TurbinID);
                let baseURT = 0;
                if (stationUrtValues && stationUrtValues[turbinKey] && stationUrtValues[turbinKey] > 0) {
                    baseURT = stationUrtValues[turbinKey];
                } else {
                    baseURT = 2300;
                }
                nominalValue = baseURT + variation;
            }

            // Если включено исключение турбин ТЭЦ и станция - ТЭЦ, обнуляем рейтинг
            const isTecStation = tecStations.includes(item.StationID);
            const equipKeyT = `${item.StationID}_${item.TurbinID}`;
            const isEquipEnabled = eqTurbines[equipKeyT] !== false;
            const shouldExclude = !isEquipEnabled || (excludeTecFromUrl && isTecStation);

            // Метаданные: дата КР/СР и примечание
            const stationUrtMeta = urtmeta[item.StationID];
            let urtMetaEntry = null;
            if (stationUrtMeta) {
                const normalizeKey = k => k.replace(/^0+(\d)/, '$1');
                const turbinNorm = normalizeKey(String(item.TurbinID));
                if (stationUrtMeta[item.TurbinID]) {
                    urtMetaEntry = stationUrtMeta[item.TurbinID];
                } else {
                    for (const [cfgKey, cfgVal] of Object.entries(stationUrtMeta)) {
                        if (normalizeKey(cfgKey) === turbinNorm) {
                            urtMetaEntry = cfgVal;
                            break;
                        }
                    }
                }
            }
            const urt_date = urtMetaEntry ? urtMetaEntry.date : '';
            const urt_note = urtMetaEntry ? urtMetaEntry.note : '';

            return {
                urt: item.URT,
                size: shouldExclude ? 0 : item.Consumption, // Обнуляем вес для ТЭЦ турбин
                station: item.StationID,
                turbin: item.TurbinID,
                urt_percent: shouldExclude ? 0 : (((item.URT - nominalValue) / nominalValue) * 100),
                urt_percent_normal: nominalValue,
                excluded: shouldExclude, // Флаг для визуализации
                urt_date: urt_date,
                urt_note: urt_note
            };
        })
    };

        
        const hierarchicalData_boiler = {
    name: "Boilers",
    children: boilersData.map(item => {
        const stationID = item.StationID;
        const boilerID = item.BoilerID;
        
        // Логирование для отладки
        console.log('Processing boiler:', {
            stationID: stationID,
            boilerID: boilerID,
            stationName: stations[stationID] || 'Unknown station',
            hasKpdValues: kpdvalues.hasOwnProperty(stationID),
            kpdValuesForStation: kpdvalues[stationID] ? Object.keys(kpdvalues[stationID]) : 'No kpdvalues for station',
            hasBoilerInKpd: kpdvalues[stationID] ? kpdvalues[stationID].hasOwnProperty(boilerID) : false
        });

        // Проверяем существование данных
        if (!kpdvalues[stationID]) {
            console.error(`❌ No kpdvalues found for station ID: ${stationID} (${stations[stationID] || 'Unknown'})`);
            console.error(`Available stations in kpdvalues:`, Object.keys(kpdvalues));
        } else if (!kpdvalues[stationID].hasOwnProperty(boilerID)) {
            console.warn(`⚠️ No kpd value found for boiler ${boilerID} at station ${stationID} (${stations[stationID] || 'Unknown'})`);
            console.warn(`Available boilers for station ${stationID}:`, Object.keys(kpdvalues[stationID]));
        }

        const kpdValue = kpdvalues[stationID] ? kpdvalues[stationID][boilerID] : undefined;
        const baseKPD = kpdValue > 0 ? kpdValue : 91;
        // Поправки применяются к нормативу: adjustedNorm = baseKPD - KPD_correction
        // KPD хранится сырым, KPD_correction = humidity + ash - (temp_fact - temp_nominal) * temp_koef
        const kpdCorrection = item.KPD_correction || 0;
        const adjustedNorm = baseKPD - kpdCorrection;
        const kpd_percent = (((item.KPD - adjustedNorm) / adjustedNorm) * 100);
        const kpd_percent_normal = adjustedNorm;

        // Метаданные: дата КР/СР и примечание
        // Нормализуем ключ: убираем ведущие нули для сравнения
        const stationMeta = kpdmeta[stationID];
        let meta = null;
        if (stationMeta) {
            // normalize: убираем ведущие нули из числовой части ("01А" → "1А", "08" → "8")
            const normalizeKey = k => k.replace(/^0+(\d)/, '$1');
            const boilerNorm = normalizeKey(boilerID);
            // Прямой поиск или поиск по нормализованному ключу
            if (stationMeta[boilerID]) {
                meta = stationMeta[boilerID];
            } else {
                // Перебираем ключи конфига, нормализуем и сравниваем
                for (const [cfgKey, cfgVal] of Object.entries(stationMeta)) {
                    if (normalizeKey(cfgKey) === boilerNorm) {
                        meta = cfgVal;
                        break;
                    }
                }
            }
        }
        const kpd_date = meta ? meta.date : '';
        const kpd_note = meta ? meta.note : '';

        const equipKeyB = `${item.StationID}_${item.BoilerID}`;
        const isBoilerEnabled = eqBoilers[equipKeyB] !== false;

        return {
            kpd: item.KPD,
            size: isBoilerEnabled ? item.Production : 0,   // Исключённые котлы обнуляем
            consumption: item.Consumption,   // Для tooltip сохраняем расход топлива
            station: item.StationID,
            boiler: item.BoilerID,
            kpd_percent: isBoilerEnabled ? kpd_percent : 0,
            kpd_percent_normal: kpd_percent_normal,
            kpd_date: kpd_date,
            kpd_note: kpd_note
        };
    })
};

        // Объединяем данные для treemap (нужно для подсветки станций)
        const combinedData = hierarchicalData_turbin.children.concat(hierarchicalData_boiler.children);

// Функция для вычисления рейтинга (такая же как на Index)
function calculateRating(group) {
    const stationId = group.length > 0 ? group[0].station : null;
    const isTec = tecStations.includes(parseInt(stationId));
    const excludeTurbines = excludeTecTurbinesFlag && isTec;

    // Котлы: взвешенный средний kpd_percent
    const boilers = group.filter(d => d.kpd_percent !== undefined);
    const boilerWeightSum = boilers.reduce((acc, d) => acc + d.size, 0);
    const boilerRating = boilerWeightSum !== 0
        ? boilers.reduce((acc, d) => acc + d.kpd_percent * d.size, 0) / boilerWeightSum
        : 0;

    // Турбины: взвешенный средний urt_percent
    const turbines = excludeTurbines ? [] : group.filter(d => d.urt_percent !== undefined);
    const turbineWeightSum = turbines.reduce((acc, d) => acc + d.size, 0);
    const turbineRating = turbineWeightSum !== 0
        ? turbines.reduce((acc, d) => acc + d.urt_percent * d.size, 0) / turbineWeightSum
        : 0;

    // Итоговая формула: ((100 + boilerRating) / (100 + turbineRating) - 1) * 100
    return ((100 + boilerRating) / (100 + turbineRating) - 1) * 100;
}

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

// Создаем объект для быстрого поиска станций с рейтингом
const ratingsMap = {};
ratings.forEach(r => {
    ratingsMap[r.StationID] = true;
});

// Находим станции без данных
const stationsWithoutData = Object.keys(stations)
    .filter(stationID => !ratingsMap[stationID])
    .map(stationID => ({
        StationID: stationID,
        rating: null,
        hasData: false
    }));

// Объединяем: сначала станции с данными, потом без данных
const allRatings = [...ratings, ...stationsWithoutData.filter(s => !s.hasData || s.hasData === false)];

// Сохраняем рейтинг в localStorage для синхронизации
localStorage.setItem('altairRatings', JSON.stringify(allRatings));



// Выбираем контейнер рейтинга (карточки добавляем в wrapper)
const ratingContainer = d3.select('#rating-cards-vis');

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
    
    // Правая часть - чекбокс (только для станций с данными)
    if (rating.hasData) {
        const checkboxDiv = cardDiv.append('div')
            .attr('class', 'radio-btn')
            .on('click', function(e) {
                e.stopPropagation();
                // Обновляем подсветку на основе выбранных станций
                updateHighlightFromCheckboxes();
                // Сохраняем состояние чекбоксов в localStorage
                saveCheckboxState();
                drawKpdScatter();
                drawUrtScatter();
            });

        // Загружаем состояние из localStorage
        const savedState = JSON.parse(localStorage.getItem('stationCheckboxState') || '{}');
        const isChecked = savedState[rating.StationID] !== undefined ? savedState[rating.StationID] : true;

        checkboxDiv.append('input')
            .attr('type', 'checkbox')
            .attr('name', 'station-vis')
            .attr('data-station-id', rating.StationID)
            .attr('id', `station-vis-${rating.StationID}`)
            .property('checked', isChecked);
    }
});

// Функция сохранения состояния чекбоксов
function saveCheckboxState() {
    const state = {};
    d3.selectAll('input[name="station-vis"]').each(function() {
        const stationId = d3.select(this).attr('data-station-id');
        state[stationId] = d3.select(this).property('checked');
    });
    localStorage.setItem('stationCheckboxState', JSON.stringify(state));
}

// Функция подсветки на основе выбранных чекбоксов
function updateHighlightFromCheckboxes() {
    const checkedStations = [];
    d3.selectAll('input[name="station-vis"]:checked').each(function() {
        checkedStations.push(+d3.select(this).attr('data-station-id'));
    });

    if (checkedStations.length === 0) {
        // Если ничего не выбрано - всё затемнено
        d3.selectAll(".node rect, .node text").style("opacity", 0.3);
    } else {
        // Затемняем все
        d3.selectAll(".node rect").style("opacity", 0.3);
        d3.selectAll(".node text").style("opacity", 0.3);

        // Подсвечиваем выбранные станции
        checkedStations.forEach(stationId => {
            d3.selectAll(".node")
                .filter(d => d.data && d.data.station == stationId)
                .select("rect")
                .style("opacity", 1);

            d3.selectAll(".node")
                .filter(d => d.data && d.data.station == stationId)
                .selectAll("text")
                .style("opacity", 1);
        });
    }
}

// Обновленные функции highlightStation и resetHighlight
function highlightStation(StationID) {
    // Затемняем все элементы и текст
    d3.selectAll(".node rect").style("opacity", 0.3);
    d3.selectAll(".node text").style("opacity", 0.3);

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
    d3.selectAll(".node rect, .node text").style("opacity", 1);
}

// Кнопка "Выбрать/Снять все" для Visualisation
// Определяем начальное состояние на основе сохранённых чекбоксов
const savedStateInit = JSON.parse(localStorage.getItem('stationCheckboxState') || '{}');
let allCheckedVis = Object.keys(savedStateInit).length === 0 || Object.values(savedStateInit).every(v => v === true);
document.getElementById('toggle-all-vis').textContent = allCheckedVis ? '☑' : '☐';

// Применяем начальную подсветку на основе сохранённого состояния
setTimeout(() => {
    updateHighlightFromCheckboxes();
}, 100);

document.getElementById('toggle-all-vis').addEventListener('click', function() {
    allCheckedVis = !allCheckedVis;
    d3.selectAll('input[name="station-vis"]').property('checked', allCheckedVis);
    this.textContent = allCheckedVis ? '☑' : '☐';
    saveCheckboxState();
    drawKpdScatter();
    drawUrtScatter();
    if (allCheckedVis) {
        resetHighlight();
    } else {
        d3.selectAll(".node rect, .node text").style("opacity", 0.3);
    }
});

        // Общая ширина окна (margin-right: 210px уже задан на контейнере)
        const totalWidth = document.body.clientWidth - 210;
        const graphWidth = totalWidth; // Ширина графика
        const height = window.innerHeight * 0.85;

        
        
        const tooltip = d3.select("#tooltip");

        // Корректировка координат tooltip с учётом CSS zoom на <html>
        function getZoom() {
            const z = parseFloat(document.documentElement.style.zoom);
            return isNaN(z) || z === 0 ? 1 : z;
        }
        function tipX(pageX, offset) { return (pageX / getZoom() + (offset || 0)) + "px"; }
        function tipY(pageY, offset) { return (pageY / getZoom() + (offset || 0)) + "px"; }

        // Переменная таймера для скрытия подсказки (общая для обоих treemap)
        let hideTimeoutTurbin;
        let hideTimeoutBoiler;

        // Функция для рисования одного treemap
            function drawTreemap_turbin(containerSelector, data) {
                const svg = d3.select(containerSelector).append("svg")
                            .attr("width", graphWidth)
                            .attr("height", height);

                // Hierarchy и обработка treemap
                const root = d3.hierarchy(data)
                            .sum(d => d.size)
                            .sort((a, b) => b.value - a.value);

                d3.treemap()
                .size([graphWidth, height])
                .padding(2)
                (root);

                // Создаем группы для всех узлов
                const cell = svg.selectAll(".node")
                                .data(root.leaves())
                                .enter().append("g")
                                .attr("transform", d => `translate(${d.x0}, ${d.y0})`)
                                .classed("node", true);

                // Прямоугольники
                cell.append("rect")
                .attr("width", d => d.x1 - d.x0)
                .attr("height", d => d.y1 - d.y0)
                .attr("class", "node-rect")
                .attr("fill", d => selectColorForURT(d.data.urt_percent !== undefined ? d.data.urt_percent : 100))
                .on("mouseenter", showTooltip) // Показываем подсказку при наведении
                .on("mousemove", moveTooltip) // Обновляем позицию подсказки
                .on("mouseleave", hideTooltip); // Скрываем подсказку при уходе мыши

                cell.filter(d => (d.x1 - d.x0) >= 38 && (d.y1 - d.y0) >= 40) // Только если ширина достаточна
                .each(function(d) {
                    const g = d3.select(this);
                    const rectWidth = d.x1 - d.x0; // Ширина прямоугольника
                    const rectHeight = d.y1 - d.y0; // Высота прямоугольника
                    const urtPercent = d.data.urt_percent !== undefined ? d.data.urt_percent : 0;
                    const textContent = `${stations[d.data.station]} ТА${d.data.turbin} ${urtPercent.toFixed(2)}%`;

                    // Вычислим оптимальное количество символов в строке
                    let fontSize = 16; // Начальный размер шрифта
                    let maxCharsPerLine = Math.floor(rectWidth / fontSize); // Максимальное количество символов в строке
                    let lines = wrapText(textContent, maxCharsPerLine);
                    let reducedForWidth = false; // Флаг для отслеживания уменьшения из-за ширины

                    // Уменьшаем шрифт или количество символов, пока текст или отдельные слова не поместятся в прямоугольник
                    while (fontSize > 6 && maxCharsPerLine > 1) {
                        const totalHeight = lines.length * (fontSize + 4); // Общая высота текста с учетом межстрочного интервала
                        const wordTooWide = lines.some(line => {
                            const words = line.split(' ');
                            return words.some(word => word.length * fontSize > rectWidth);
                        });

                        if (totalHeight <= rectHeight && !wordTooWide) {
                            break; // Если текст и слова помещаются, выходим из цикла
                        }

                        if (wordTooWide && fontSize > 6) {
                            fontSize--; // Уменьшаем размер шрифта, чтобы слово поместилось
                            reducedForWidth = true; // Отмечаем, что уменьшили из-за ширины
                        } else if (maxCharsPerLine > 1) {
                            maxCharsPerLine--; // Уменьшаем количество символов в строке
                        } else {
                            break; // Если достигнуты минимальные значения, выходим из цикла
                        }

                        lines = wrapText(textContent, maxCharsPerLine); // Пересчитываем строки
                    }

                    // Если высота меньше 60, уменьшаем шрифт на 2 пункта
                    if (rectHeight < 60 && fontSize > 9) {
                        fontSize -= 4;
                    }

                    // Если шрифт был уменьшен из-за ширины, увеличиваем его на 2 пункта
                    if (reducedForWidth && fontSize < 16) {
                        fontSize += 2;
                    }

                    // Центруем текст по вертикали
                    const lineHeight = fontSize + 4; // Средняя высота строки
                    const numLines = lines.length;
                    const yCenterOffset = ((numLines * lineHeight) - rectHeight) / 2;

                    // Добавляем текстовые строки
                    lines.forEach((line, index) => {
                        g.append("text")
                            .attr("class", "centered-text node-text")
                            .attr("x", (d.x1 - d.x0) / 2) // Х-центр блока
                            .attr("y", (index * lineHeight) - yCenterOffset + lineHeight / 2) // Центрирование по вертикали
                            .style("font-size", `${fontSize}px`) // Размер шрифта
                            .style("font-family", "Arial") // Шрифт
                            .style("font-weight", "bold") // Жирный шрифт
                            .style("fill", "#dddddd") // Белый цвет текста
                            .style("text-anchor", "middle") // Выравнивание по центру
                            .text(line);
                    });
                });

            // Вспомогательная функция для разрыва строки
            function wrapText(text, maxCharsPerLine) {
                const words = text.split(' ');
                const wrappedLines = [];
                let currentLine = '';

                words.forEach(word => {
                    if ((currentLine + word).length > maxCharsPerLine && currentLine !== '') {
                        wrappedLines.push(currentLine.trim());
                        currentLine = word + ' ';
                    } else {
                        currentLine += word + ' ';
                    }
                });

                if (currentLine.trim()) {
                    wrappedLines.push(currentLine.trim());
                }

                return wrappedLines;
            }

                    // Функции обработки подсказки
                    function showTooltip(event, d) {
                        const urt = d.data.urt !== undefined ? d.data.urt : 0;
                        const urtNormal = d.data.urt_percent_normal !== undefined ? d.data.urt_percent_normal : 0;
                        const size = d.data.size !== undefined ? d.data.size : 0;

                        // Формируем строку даты КР/СР
                        const urtDate = d.data.urt_date || '';
                        const urtNote = d.data.urt_note || '';
                        let dateLabel = '';
                        if (urtDate && urtDate !== '-') {
                            const isSR = urtNote.toLowerCase().includes('ср');
                            dateLabel = `<br/>Дата ${isSR ? 'СР' : 'КР'}: ${urtDate}`;
                        }

                        const content = `
                            Станция: ${stations[d.data.station]}<br/>
                            Турбина: ${d.data.turbin}<br/>
                            УРТ: ${urt.toFixed(2)}<br/>
                            Норма УРТ: ${urtNormal.toFixed(2)}${dateLabel}<br/>
                            Расход тепла: ${size.toFixed(2)}
                        `;

                        clearTimeout(hideTimeoutTurbin);

                        tooltip.html(content)
                            .style("opacity", 1)
                            .style("top",  tipY(event.pageY, -tooltip.node().offsetHeight))
                            .style("left", tipX(event.pageX, -150));
                    }

                    function moveTooltip(event) {
                        tooltip.style("top",  tipY(event.pageY, -tooltip.node().offsetHeight))
                               .style("left", tipX(event.pageX, -150));
                    }

                    function hideTooltip() {
                        hideTimeoutTurbin = setTimeout(() => {
                            tooltip.style("opacity", 0);
                        }, 50);
                    }
                    // Функция выбора цвета в зависимости от KPD
                    function selectColorForURT(urtValue) {
                        if (urtValue > 4) return '#8B0000'; // Красный
                        if (urtValue >= 2 && urtValue < 4) return '#BF0000'; // Красный
                        if (urtValue >= 0 && urtValue < 2) return '#c5172c'; // Оранжевый
                        if (urtValue >= -2 && urtValue < 0) return '#009F00'; // Светло-зеленый
                        if (urtValue < -2) return '#006400'; // Темно-зеленый
                        return '#DDDDDD'; // Серый по умолчанию
                    }
                }

            function drawTreemap_boiler(containerSelector, data) {
            const svg = d3.select(containerSelector).append("svg")
                           .attr("width", graphWidth)
                           .attr("height", height);

            // Hierarchy и обработка treemap
            const root = d3.hierarchy(data)
                           .sum(d => d.size)
                           .sort((a, b) => b.value - a.value);

            d3.treemap()
               .size([graphWidth, height])
               .padding(2)
               (root);

            // Создаем группы для всех узлов
            const cell = svg.selectAll(".node")
                             .data(root.leaves())
                             .enter().append("g")
                             .attr("transform", d => `translate(${d.x0}, ${d.y0})`)
                             .classed("node", true);
            // Прямоугольники
            cell.append("rect")
               .attr("width", d => d.x1 - d.x0)
               .attr("height", d => d.y1 - d.y0)
               .attr("class", "node-rect")
               .attr("fill", d => selectColorForKPD(d.data.kpd_percent !== undefined ? d.data.kpd_percent : 0))
               .on("mouseenter", showTooltip) // Показываем подсказку при наведении
               .on("mousemove", moveTooltip) // Обновляем позицию подсказки
               .on("mouseleave", hideTooltip); // Скрываем подсказку при уходе мыши
            // Добавляем текст, соблюдая ограничения и переносы
            cell.filter(d => (d.x1 - d.x0) >= 38 && (d.y1 - d.y0) >= 40) // Только если ширина достаточна
                .each(function(d) {
                    const g = d3.select(this);
                    const rectWidth = d.x1 - d.x0; // Ширина прямоугольника
                    const rectHeight = d.y1 - d.y0; // Высота прямоугольника
                    const kpdPercent = d.data.kpd_percent !== undefined ? d.data.kpd_percent : 0;
                    const textContent = `${stations[d.data.station]} КА${d.data.boiler} ${kpdPercent.toFixed(2)}%`;

                    // Вычислим оптимальное количество символов в строке
                    let fontSize = 16; // Начальный размер шрифта
                    let maxCharsPerLine = Math.floor(rectWidth / fontSize); // Максимальное количество символов в строке
                    let lines = wrapText(textContent, maxCharsPerLine);
                    let reducedForWidth = false; // Флаг для отслеживания уменьшения из-за ширины

                    // Уменьшаем шрифт или количество символов, пока текст или отдельные слова не поместятся в прямоугольник
                    while (fontSize > 6 && maxCharsPerLine > 1) {
                        const totalHeight = lines.length * (fontSize + 4); // Общая высота текста с учетом межстрочного интервала
                        const wordTooWide = lines.some(line => {
                            const words = line.split(' ');
                            return words.some(word => word.length * fontSize > rectWidth);
                        });

                        if (totalHeight <= rectHeight && !wordTooWide) {
                            break; // Если текст и слова помещаются, выходим из цикла
                        }

                        if (wordTooWide && fontSize > 6) {
                            fontSize--; // Уменьшаем размер шрифта, чтобы слово поместилось
                            reducedForWidth = true; // Отмечаем, что уменьшили из-за ширины
                        } else if (maxCharsPerLine > 1) {
                            maxCharsPerLine--; // Уменьшаем количество символов в строке
                        } else {
                            break; // Если достигнуты минимальные значения, выходим из цикла
                        }

                        lines = wrapText(textContent, maxCharsPerLine); // Пересчитываем строки
                    }

                    // Если высота меньше 60, уменьшаем шрифт на 2 пункта
                    if (rectHeight < 60 && fontSize > 9) {
                        fontSize -= 4;
                    }

                    // Если шрифт был уменьшен из-за ширины, увеличиваем его на 2 пункта
                    if (reducedForWidth && fontSize < 16) {
                        fontSize += 2;
                    }

                    // Центруем текст по вертикали
                    const lineHeight = fontSize + 4; // Средняя высота строки
                    const numLines = lines.length;
                    const yCenterOffset = ((numLines * lineHeight) - rectHeight) / 2;

                    // Добавляем текстовые строки
                    lines.forEach((line, index) => {
                        g.append("text")
                            .attr("class", "centered-text node-text")
                            .attr("x", (d.x1 - d.x0) / 2) // Х-центр блока
                            .attr("y", (index * lineHeight) - yCenterOffset + lineHeight / 2) // Центрирование по вертикали
                            .style("font-size", `${fontSize}px`) // Размер шрифта
                            .style("font-family", "Arial") // Шрифт
                            .style("font-weight", "bold") // Жирный шрифт
                            .style("fill", "#dddddd") // Белый цвет текста
                            .style("text-anchor", "middle") // Выравнивание по центру
                            .text(line);
                    });
                });

            // Вспомогательная функция для разрыва строки
            function wrapText(text, maxCharsPerLine) {
                const words = text.split(' ');
                const wrappedLines = [];
                let currentLine = '';

                words.forEach(word => {
                    if ((currentLine + word).length > maxCharsPerLine && currentLine !== '') {
                        wrappedLines.push(currentLine.trim());
                        currentLine = word + ' ';
                    } else {
                        currentLine += word + ' ';
                    }
                });

                if (currentLine.trim()) {
                    wrappedLines.push(currentLine.trim());
                }

                return wrappedLines;
            }

                            // Функции для всплывающих подсказок
                function showTooltip(event, d) {
                    const kpd = d.data.kpd !== undefined ? d.data.kpd : 0;
                    const kpdNormal = d.data.kpd_percent_normal !== undefined ? d.data.kpd_percent_normal : 0;
                    const consumption = d.data.consumption !== undefined ? d.data.consumption : 0;

                    // Формируем строку даты КР/СР
                    const kpdDate = d.data.kpd_date || '';
                    const kpdNote = d.data.kpd_note || '';
                    let dateLabel = '';
                    if (kpdDate && kpdDate !== '-') {
                        // Определяем тип по примечанию: СР → "Дата СР", иначе "Дата КР"
                        const isSR = kpdNote.toLowerCase().includes('ср');
                        dateLabel = `<br/>${isSR ? 'Дата СР' : 'Дата КР'}: ${kpdDate}`;
                    }

                    const content = `
                        Станция: ${stations[d.data.station]}<br/>
                        Котёл: ${d.data.boiler}<br/>
                        КПД: ${kpd.toFixed(2)}<br/>
                        КПД КР/СР: ${kpdNormal.toFixed(2)}${dateLabel}<br/>
                        Расход топлива: ${consumption.toFixed(2)}
                    `;
                    clearTimeout(hideTimeoutBoiler);
                    tooltip.html(content)
                        .style("opacity", 1)
                        .style("top",  tipY(event.pageY, -tooltip.node().offsetHeight - 10))
                        .style("left", tipX(event.pageX, -150));
                }

                function moveTooltip(event) {
                    tooltip.style("top",  tipY(event.pageY, -tooltip.node().offsetHeight - 10))
                           .style("left", tipX(event.pageX, -150));
                }

                function hideTooltip() {
                    hideTimeoutBoiler = setTimeout(() => {
                        tooltip.style("opacity", 0)
                    },500);
                }
                // Функция выбора цвета в зависимости от KPD
                function selectColorForKPD(kpdValue) {
                    if (kpdValue < -2) return '#8B0000'; // Красный
                    if (kpdValue >= -2 && kpdValue < -1) return '#BF0000'; // Красный
                    if (kpdValue >= -1 && kpdValue < 0) return '#c5172c'; // Оранжевый
                    if (kpdValue >= 0 && kpdValue < 1) return '#009F00'; // Светло-зеленый
                    if (kpdValue >= 1) return '#006400'; // Темно-зеленый
                    return '#DDDDDD'; // Серый по умолчанию
                }
            }

        // Отрисовываем левый график
        drawTreemap_turbin("#treemap-right", hierarchicalData_turbin);

        // Отрисовываем правый график (можно передать тот же или другой набор данных)
        drawTreemap_boiler("#treemap-left", hierarchicalData_boiler);
        drawKpdScatter();
        drawUrtScatter();

        // Автоподбор размера шрифта для рейтинга
        function fitRatingFontSize() {
            const container = document.getElementById('rating-container-vis');
            const wrapper = document.getElementById('rating-cards-vis');
            if (!container || !wrapper) {
                console.log('fitRatingFontSize: container or wrapper not found');
                return;
            }

            // Определяем масштаб браузера
            const browserZoom = window.devicePixelRatio || 1;

            // Используем полную доступную высоту контейнера
            const availableHeight = window.innerHeight - 60 - 45; // минус header и footer

            // Ограничиваем высоту контейнера
            container.style.height = availableHeight + 'px';
            container.style.bottom = 'auto';

            // Высота для карточек (минус заголовок ~35px)
            const cardsAvailableHeight = availableHeight - 35;
            wrapper.style.maxHeight = cardsAvailableHeight + 'px';
            wrapper.style.overflow = 'hidden';

            const cards = wrapper.querySelectorAll('.rating-card');
            console.log('fitRatingFontSize: cards count =', cards.length, 'availableHeight =', availableHeight, 'zoom =', browserZoom);
            if (cards.length === 0) return;

            // Если уже рассчитан размер шрифта и это пересчёт при zoom - не меняем шрифт
            if (wrapper.dataset.calculatedFontSize && browserZoom !== 1) {
                wrapper.style.fontSize = wrapper.dataset.calculatedFontSize + 'px';
                return;
            }

            // Начинаем с большого шрифта и уменьшаем пока не поместится
            let fontSize = 20;
            const minFontSize = 6;

            while (fontSize >= minFontSize) {
                wrapper.style.fontSize = fontSize + 'px';

                // Пересчитываем высоту контента
                let totalHeight = 0;
                cards.forEach(card => {
                    totalHeight += card.offsetHeight;
                });

                // Проверяем, не переполняется ли текст по ширине в каждой карточке
                let textFits = true;
                cards.forEach(card => {
                    const ratingInfo = card.querySelector('.rating-info');
                    if (ratingInfo && ratingInfo.scrollWidth > ratingInfo.clientWidth) {
                        textFits = false;
                    }
                });

                console.log('fitRatingFontSize: trying fontSize =', fontSize, 'totalHeight =', totalHeight, 'cardsAvailableHeight =', cardsAvailableHeight, 'textFits =', textFits);

                if (totalHeight <= cardsAvailableHeight && textFits) {
                    console.log('fitRatingFontSize: final fontSize =', fontSize, 'totalHeight =', totalHeight);
                    // Уменьшаем на 1 пункт для лучшего вида и сохраняем
                    const finalSize = fontSize - 1;
                    wrapper.style.fontSize = finalSize + 'px';
                    wrapper.dataset.calculatedFontSize = finalSize;

                    // Синхронизируем размер шрифта в рейтинге котлов
                    const boilerList = document.getElementById('boiler-rating-list');
                    if (boilerList) boilerList.style.fontSize = finalSize + 'px';
                    const turbineList = document.getElementById('turbine-rating-list');
                    if (turbineList) turbineList.style.fontSize = finalSize + 'px';

                    break;
                }
                fontSize -= 0.5;
            }
        }

        // Вызываем после загрузки и при ресайзе
        setTimeout(fitRatingFontSize, 300);
        window.addEventListener('resize', fitRatingFontSize);

        // Перезагружаем страницу если браузер восстановил её из BFCACHE,
        // чтобы гарантировать пересчёт с актуальными настройками оборудования.
        window.addEventListener('pageshow', function(e) {
            if (e.persisted) { location.reload(); }
        });

        function drawKpdScatter() {
            const svgEl = document.getElementById('kpd-scatter-svg');
            if (!svgEl) return;

            const checked = document.querySelectorAll('input[name="station-vis"]:checked');
            const selectedIds = new Set(Array.from(checked).map(cb => parseInt(cb.dataset.stationId)));

            const data = (hierarchicalData_boiler?.children || []).filter(d =>
                selectedIds.has(d.station) && d.kpd > 0 && d.kpd_percent_normal > 0
            );

            d3.select('#kpd-scatter-svg').selectAll('*').remove();

            if (data.length === 0) {
                svgEl.setAttribute('height', '40');
                d3.select('#kpd-scatter-svg')
                    .append('text')
                    .attr('x', 10).attr('y', 24)
                    .attr('fill', '#666').attr('font-size', '13px')
                    .text('Нет данных для выбранных станций');
                drawBoilerRating([]);
                return;
            }

            const allVals = data.flatMap(d => [d.kpd, d.kpd_percent_normal]);
            const minVal = Math.floor(Math.min(...allVals)) - 1;
            const maxVal = Math.ceil(Math.max(...allVals)) + 1;

            const margin = { top: 15, right: 20, bottom: 60, left: 70 };
            const container = document.getElementById('kpd-scatter-container');
            const totalWidth = container ? container.clientWidth - 30 : (svgEl.parentElement.clientWidth || 800);
            const width  = totalWidth - margin.left - margin.right;
            const height = Math.round(width * 0.35);

            svgEl.setAttribute('height', height + margin.top + margin.bottom);
            svgEl.setAttribute('width', totalWidth);

            // Синхронизируем высоту рейтинга котлов с высотой scatter-контейнера
            requestAnimationFrame(() => {
                const scatterCont = document.getElementById('kpd-scatter-container');
                const boilerPanel = document.getElementById('boiler-rating-panel');
                if (scatterCont && boilerPanel) {
                    boilerPanel.style.height = scatterCont.offsetHeight + 'px';
                }
                drawBoilerRating(data);
            });

            const svg = d3.select('#kpd-scatter-svg')
                .append('g')
                .attr('transform', `translate(${margin.left},${margin.top})`);

            const xScale = d3.scaleLinear().domain([minVal, maxVal]).range([0, width]);
            const yScale = d3.scaleLinear().domain([minVal, maxVal]).range([height, 0]);

            // Сетка X
            svg.append('g').attr('class', 'grid')
                .attr('transform', `translate(0,${height})`)
                .call(d3.axisBottom(xScale).tickSize(-height).tickFormat(''))
                .selectAll('line').attr('stroke', '#333').attr('stroke-dasharray', '3,3');
            svg.select('.grid .domain').remove();

            // Сетка Y
            svg.append('g').attr('class', 'grid-y')
                .call(d3.axisLeft(yScale).tickSize(-width).tickFormat(''))
                .selectAll('line').attr('stroke', '#333').attr('stroke-dasharray', '3,3');
            svg.select('.grid-y .domain').remove();

            // Диагональ y = x
            svg.append('line')
                .attr('x1', xScale(minVal)).attr('y1', yScale(minVal))
                .attr('x2', xScale(maxVal)).attr('y2', yScale(maxVal))
                .attr('stroke', '#555').attr('stroke-width', 1)
                .attr('stroke-dasharray', '5,4');

            // Ось X
            const xAxis = svg.append('g')
                .attr('transform', `translate(0,${height})`)
                .call(d3.axisBottom(xScale).ticks(6));
            xAxis.selectAll('text').attr('fill', '#888');
            xAxis.selectAll('line').attr('stroke', '#888');
            xAxis.select('.domain').attr('stroke', '#888');

            // Ось Y
            const yAxis = svg.append('g')
                .call(d3.axisLeft(yScale).ticks(6));
            yAxis.selectAll('text').attr('fill', '#888');
            yAxis.selectAll('line').attr('stroke', '#888');
            yAxis.select('.domain').attr('stroke', '#888');

            // Подписи осей
            svg.append('text')
                .attr('x', width / 2).attr('y', height + 38)
                .attr('fill', '#aaa').attr('font-size', '12px').attr('text-anchor', 'middle')
                .text('КПД КР/СР, %');
            svg.append('text')
                .attr('transform', 'rotate(-90)')
                .attr('x', -height / 2).attr('y', -42)
                .attr('fill', '#aaa').attr('font-size', '12px').attr('text-anchor', 'middle')
                .text('КПД факт, %');

            function dotColor(kpdPct) {
                if (kpdPct < -2) return '#8B0000';
                if (kpdPct < -1) return '#BF0000';
                if (kpdPct < 0)  return '#c5172c';
                if (kpdPct < 1)  return '#009F00';
                return '#006400';
            }

            function scatterZoom() {
                const z = parseFloat(document.documentElement.style.zoom);
                return isNaN(z) || z === 0 ? 1 : z;
            }

            const tooltip = document.getElementById('tooltip');
            svg.selectAll('.dot')
                .data(data)
                .enter().append('circle')
                .attr('class', 'dot')
                .attr('cx', d => xScale(d.kpd_percent_normal))
                .attr('cy', d => yScale(d.kpd))
                .attr('r', 6)
                .attr('fill', d => dotColor(d.kpd_percent))
                .attr('fill-opacity', 0.85)
                .attr('stroke', '#000').attr('stroke-width', 0.5)
                .on('mouseenter', function(event, d) {
                    if (!tooltip) return;
                    const z = scatterZoom();
                    tooltip.style.opacity = '1';
                    tooltip.style.display = 'block';
                    tooltip.innerHTML =
                        `<strong>Станция:</strong> ${stations[d.station] || d.station}<br>` +
                        `<strong>Котёл:</strong> ${d.boiler}<br>` +
                        `<strong>КПД факт:</strong> ${d.kpd.toFixed(2)}<br>` +
                        `<strong>КПД КР/СР:</strong> ${d.kpd_percent_normal.toFixed(2)}`;
                    tooltip.style.left = (event.pageX / z + 12) + 'px';
                    tooltip.style.top  = (event.pageY / z - tooltip.offsetHeight - 10) + 'px';
                })
                .on('mousemove', function(event) {
                    if (!tooltip) return;
                    const z = scatterZoom();
                    tooltip.style.left = (event.pageX / z + 12) + 'px';
                    tooltip.style.top  = (event.pageY / z - tooltip.offsetHeight - 10) + 'px';
                })
                .on('mouseleave', function() {
                    if (!tooltip) return;
                    tooltip.style.opacity = '0';
                    tooltip.style.display = 'none';
                });

        }

        // Возвращает КА-инциденты для конкретного котла (stationId, boilerId)
        // КА = котлоагрегат; если нет " -" в записи — относится ко всей станции.
        // Поиск по истории файлов: используем первый файл, в котором есть инциденты для котла.
        // Алиасы названий станций для поиска инцидентов (ключ — название из stations, значения — допустимые варианты в файле инцидентов)
        const stationAliases = {
            'ТуГРЭС': ['ТУГРЭС'],
            'БиТЭЦ': ['БийТЭЦ'],
            'БбТЭЦ': ['БрбТЭЦ'],
            'НкТЭЦ': ['НКТЭЦ']
        };

        function getBoilerIncidents(stationId, boilerId) {
            const stName = stations[stationId] || '';
            const boilerNorm = boilerId.replace(/^0+/, '').toLowerCase();
            const aliases = [stName, ...(stationAliases[stName] || [])];

            function filterRecords(records) {
                return (records || []).filter(r => {
                    const stn = r.StationName || '';
                    const stationMatch = aliases.some(a => stn === a || stn.includes(a) || a.includes(stn));
                    if (!stationMatch) return false;

                    const eq = r.EquipmentParameter || '';
                    const dashIdx = eq.indexOf(' -');

                    if (dashIdx === -1) return true;

                    const eqName = eq.substring(0, dashIdx).trim();
                    if (!/^КА/i.test(eqName)) return false;

                    const eqBoilerId = eqName.replace(/^КА[-\s]*/i, '').trim();
                    const eqNorm = eqBoilerId.replace(/^0+/, '').toLowerCase();
                    return eqNorm === boilerNorm;
                });
            }

            for (const records of (techParamsHistory || [])) {
                const found = filterRecords(records);
                if (found.length > 0) return found;
            }
            return [];
        }

        let _boilerRatingSorted = [];

        // ---- Общие утилиты для стилизованного Excel ----
        function xlsBorder() {
            return {
                top:    { style: 'thin', color: { rgb: '999999' } },
                bottom: { style: 'thin', color: { rgb: '999999' } },
                left:   { style: 'thin', color: { rgb: '999999' } },
                right:  { style: 'thin', color: { rgb: '999999' } }
            };
        }
        function xlsHeaderStyle() {
            return {
                font: { bold: true, color: { rgb: 'FFFFFF' }, sz: 11 },
                fill: { fgColor: { rgb: '2F5496' } },
                alignment: { horizontal: 'center', vertical: 'center', wrapText: true },
                border: xlsBorder()
            };
        }
        function xlsCellStyle(isNumber) {
            return {
                font: { sz: 10 },
                alignment: { vertical: 'center', wrapText: true, horizontal: isNumber ? 'center' : 'left' },
                border: xlsBorder()
            };
        }
        function xlsIncidentStyle() {
            return {
                font: { sz: 9, color: { rgb: '8B0000' } },
                alignment: { vertical: 'center', wrapText: true },
                border: xlsBorder()
            };
        }
        function xlsStyledExport(rows, headers, sheetName, fileName, incidentStartIdx) {
            const ws = XLSX.utils.aoa_to_sheet([]);
            // Заголовки
            XLSX.utils.sheet_add_aoa(ws, [headers], { origin: 'A1' });
            // Данные
            rows.forEach((row, r) => {
                XLSX.utils.sheet_add_aoa(ws, [row], { origin: XLSX.utils.encode_cell({ r: r + 1, c: 0 }) });
            });

            // Стили заголовков
            for (let c = 0; c < headers.length; c++) {
                const ref = XLSX.utils.encode_cell({ r: 0, c });
                if (ws[ref]) ws[ref].s = xlsHeaderStyle();
            }
            // Стили данных
            for (let r = 0; r < rows.length; r++) {
                for (let c = 0; c < rows[r].length; c++) {
                    const ref = XLSX.utils.encode_cell({ r: r + 1, c });
                    if (!ws[ref]) continue;
                    if (c >= incidentStartIdx) {
                        ws[ref].s = xlsIncidentStyle();
                    } else {
                        ws[ref].s = xlsCellStyle(c === 0 || c >= 3);
                    }
                }
                // Чередование фона строк
                if (r % 2 === 1) {
                    for (let c = 0; c < rows[r].length; c++) {
                        const ref = XLSX.utils.encode_cell({ r: r + 1, c });
                        if (ws[ref]) {
                            ws[ref].s = { ...ws[ref].s, fill: { fgColor: { rgb: 'F2F2F2' } } };
                        }
                    }
                }
            }

            // Ширина столбцов — автоподбор
            const colWidths = headers.map((h, c) => {
                let maxLen = h.length;
                rows.forEach(row => {
                    const val = row[c] != null ? String(row[c]) : '';
                    maxLen = Math.max(maxLen, val.length);
                });
                // Ограничиваем ширину инцидентов
                if (c >= incidentStartIdx) return { wch: Math.min(maxLen + 2, 50) };
                return { wch: Math.min(maxLen + 4, 30) };
            });
            ws['!cols'] = colWidths;

            // Высота строк
            const rowHeights = [{ hpt: 30 }]; // заголовок
            rows.forEach(row => {
                // Если есть инциденты — увеличиваем высоту
                let hasIncident = false;
                for (let c = incidentStartIdx; c < row.length; c++) {
                    if (row[c]) { hasIncident = true; break; }
                }
                rowHeights.push({ hpt: hasIncident ? 36 : 20 });
            });
            ws['!rows'] = rowHeights;

            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, sheetName);
            XLSX.writeFile(wb, fileName);
        }

        function exportBoilerRating() {
            if (_boilerRatingSorted.length === 0) return;

            // Определяем макс. число инцидентов
            let maxInc = 0;
            const dataRows = _boilerRatingSorted.map((d, i) => {
                const stName = stations[d.station] || String(d.station);
                const incidents = getBoilerIncidents(d.station, d.boiler);
                maxInc = Math.max(maxInc, incidents.length);
                return { d, i, stName, incidents };
            });

            const baseHeaders = ['№', 'Станция', 'Котёл', 'КПД факт', 'КПД КР/СР'];
            const incHeaders = Array.from({ length: maxInc }, (_, j) => `Инцидент ${j + 1}`);
            const headers = [...baseHeaders, ...incHeaders];

            const rows = dataRows.map(({ d, i, stName, incidents }) => {
                const base = [i + 1, stName, d.boiler, +d.kpd.toFixed(2), +d.kpd_percent_normal.toFixed(2)];
                const incs = Array.from({ length: maxInc }, (_, j) => {
                    if (j >= incidents.length) return '';
                    const inc = incidents[j];
                    let text = inc.EquipmentParameter || '';
                    if (inc.ActualValue != null) text += ` — Факт: ${(+inc.ActualValue).toFixed(2)}`;
                    if (inc.NormValue != null)   text += `, Норма: ${(+inc.NormValue).toFixed(2)}`;
                    if (inc.DaysCount)           text += ` (${inc.DaysCount} дн.)`;
                    return text;
                });
                return [...base, ...incs];
            });

            xlsStyledExport(rows, headers, 'Рейтинг котлов', 'boiler_rating.xlsx', 5);
        }

        function drawBoilerRating(data) {
            const list = document.getElementById('boiler-rating-list');
            if (!list) return;
            list.innerHTML = '';

            if (data.length === 0) {
                list.innerHTML = '<div style="color:#666;font-size:12px;padding:4px;">Нет данных</div>';
                _boilerRatingSorted = [];
                return;
            }

            // Сортируем по kpd_percent descending (лучшие сверху)
            const sorted = [...data].sort((a, b) => b.kpd_percent - a.kpd_percent);
            _boilerRatingSorted = sorted;

            // Берём font-size из станционного рейтинга
            const stationWrapper = document.getElementById('rating-cards-vis');
            const cardFontSize = (stationWrapper && stationWrapper.dataset.calculatedFontSize)
                ? stationWrapper.dataset.calculatedFontSize + 'px'
                : '11px';
            list.style.fontSize = cardFontSize;

            function scoreClass(p) {
                if (p < -2) return 'score-value-1';
                if (p < -1) return 'score-value-3';
                if (p <  0) return 'score-value-2';
                if (p <  1) return 'score-value-4';
                return 'score-value-5';
            }

            const tooltip = document.getElementById('tooltip');

            sorted.forEach((d, i) => {
                const stName = stations[d.station] || String(d.station);
                const stShort = stName;
                const pctStr = (d.kpd_percent >= 0 ? '+' : '') + d.kpd_percent.toFixed(1) + '%';

                const card = document.createElement('div');
                card.className = 'rating-card';
                card.innerHTML =
                    `<div class="rating-info">` +
                        `<span class="rank-number">${i + 1}.</span>` +
                        `<span class="user-name">${stShort}&nbsp;КА&nbsp;${d.boiler}</span>` +
                    `</div>` +
                    `<span class="${scoreClass(d.kpd_percent)}">${pctStr}</span>`;

                // Тултип с КПД и инцидентами
                card.addEventListener('mouseenter', function(e) {
                    if (!tooltip) return;
                    const incidents = getBoilerIncidents(d.station, d.boiler);
                    let html = `<strong>${d.boiler} — ${stName}</strong><br>` +
                               `КПД факт: <strong>${d.kpd.toFixed(2)}</strong><br>` +
                               `КПД КР/СР: <strong>${d.kpd_percent_normal.toFixed(2)}</strong>`;
                    if (incidents.length > 0) {
                        html += `<br><br><strong style="color:#f09">Инциденты:</strong>`;
                        incidents.forEach(inc => {
                            html += `<br>• ${inc.EquipmentParameter}`;
                            if (inc.ActualValue != null) html += ` — Факт: ${(+inc.ActualValue).toFixed(2)}`;
                            if (inc.NormValue != null)   html += `, Норма: ${(+inc.NormValue).toFixed(2)}`;
                            if (inc.DaysCount)           html += ` (${inc.DaysCount} дн.)`;
                        });
                    }
                    const z = parseFloat(document.documentElement.style.zoom) || 1;
                    tooltip.innerHTML = html;
                    tooltip.style.opacity = '1';
                    tooltip.style.display = 'block';
                    tooltip.style.left = (e.pageX / z + 14) + 'px';
                    tooltip.style.top  = (e.pageY / z - tooltip.offsetHeight - 10) + 'px';
                });

                card.addEventListener('mousemove', function(e) {
                    if (!tooltip) return;
                    const z = parseFloat(document.documentElement.style.zoom) || 1;
                    tooltip.style.left = (e.pageX / z + 14) + 'px';
                    tooltip.style.top  = (e.pageY / z - tooltip.offsetHeight - 10) + 'px';
                });

                card.addEventListener('mouseleave', function() {
                    if (!tooltip) return;
                    tooltip.style.opacity = '0';
                    tooltip.style.display = 'none';
                });

                list.appendChild(card);
            });
        }

        // ============================================================
        // ===========   Рейтинг турбин + УРТ scatter   ===============
        // ============================================================

        function drawUrtScatter() {
            const svgEl = document.getElementById('urt-scatter-svg');
            if (!svgEl) return;

            const checked = document.querySelectorAll('input[name="station-vis"]:checked');
            const selectedIds = new Set(Array.from(checked).map(cb => parseInt(cb.dataset.stationId)));

            const data = (hierarchicalData_turbin?.children || []).filter(d =>
                selectedIds.has(d.station) && d.urt > 0 && d.urt_percent_normal > 0 && !d.excluded
            );

            d3.select('#urt-scatter-svg').selectAll('*').remove();

            if (data.length === 0) {
                svgEl.setAttribute('height', '40');
                d3.select('#urt-scatter-svg')
                    .append('text')
                    .attr('x', 10).attr('y', 24)
                    .attr('fill', '#666').attr('font-size', '13px')
                    .text('Нет данных для выбранных станций');
                drawTurbineRating([]);
                return;
            }

            const allVals = data.flatMap(d => [d.urt, d.urt_percent_normal]);
            const minVal = Math.floor(Math.min(...allVals)) - 100;
            const maxVal = Math.ceil(Math.max(...allVals)) + 1;

            const margin = { top: 15, right: 20, bottom: 60, left: 70 };
            const container = document.getElementById('urt-scatter-container');
            const totalWidth = container ? container.clientWidth - 30 : (svgEl.parentElement.clientWidth || 800);
            const width  = totalWidth - margin.left - margin.right;
            const height = Math.round(width * 0.35);

            svgEl.setAttribute('height', height + margin.top + margin.bottom);
            svgEl.setAttribute('width', totalWidth);

            // Синхронизируем высоту рейтинга турбин с высотой scatter-контейнера
            requestAnimationFrame(() => {
                const scatterCont = document.getElementById('urt-scatter-container');
                const turbinePanel = document.getElementById('turbine-rating-panel');
                if (scatterCont && turbinePanel) {
                    turbinePanel.style.height = scatterCont.offsetHeight + 'px';
                }
                drawTurbineRating(data);
            });

            const svg = d3.select('#urt-scatter-svg')
                .append('g')
                .attr('transform', `translate(${margin.left},${margin.top})`);

            const xScale = d3.scaleLinear().domain([minVal, maxVal]).range([0, width]);
            const yScale = d3.scaleLinear().domain([minVal, maxVal]).range([height, 0]);

            // Сетка X
            svg.append('g').attr('class', 'grid')
                .attr('transform', `translate(0,${height})`)
                .call(d3.axisBottom(xScale).tickSize(-height).tickFormat(''))
                .selectAll('line').attr('stroke', '#333').attr('stroke-dasharray', '3,3');
            svg.select('.grid .domain').remove();

            // Сетка Y
            svg.append('g').attr('class', 'grid-y')
                .call(d3.axisLeft(yScale).tickSize(-width).tickFormat(''))
                .selectAll('line').attr('stroke', '#333').attr('stroke-dasharray', '3,3');
            svg.select('.grid-y .domain').remove();

            // Диагональ y = x
            svg.append('line')
                .attr('x1', xScale(minVal)).attr('y1', yScale(minVal))
                .attr('x2', xScale(maxVal)).attr('y2', yScale(maxVal))
                .attr('stroke', '#555').attr('stroke-width', 1)
                .attr('stroke-dasharray', '5,4');

            // Ось X
            const xAxis = svg.append('g')
                .attr('transform', `translate(0,${height})`)
                .call(d3.axisBottom(xScale).ticks(6));
            xAxis.selectAll('text').attr('fill', '#888');
            xAxis.selectAll('line').attr('stroke', '#888');
            xAxis.select('.domain').attr('stroke', '#888');

            // Ось Y
            const yAxis = svg.append('g')
                .call(d3.axisLeft(yScale).ticks(6));
            yAxis.selectAll('text').attr('fill', '#888');
            yAxis.selectAll('line').attr('stroke', '#888');
            yAxis.select('.domain').attr('stroke', '#888');

            // Подписи осей
            svg.append('text')
                .attr('x', width / 2).attr('y', height + 38)
                .attr('fill', '#aaa').attr('font-size', '12px').attr('text-anchor', 'middle')
                .text('УРТ номинал после КР/СР, г/кВт·ч');
            svg.append('text')
                .attr('transform', 'rotate(-90)')
                .attr('x', -height / 2).attr('y', -42)
                .attr('fill', '#aaa').attr('font-size', '12px').attr('text-anchor', 'middle')
                .text('УРТ факт, г/кВт·ч');

            // Для УРТ: меньше = лучше, поэтому пороги инвертированы относительно котлов
            function dotColorUrt(urtPct) {
                if (urtPct >  2) return '#8B0000';
                if (urtPct >  1) return '#BF0000';
                if (urtPct >  0) return '#c5172c';
                if (urtPct > -1) return '#009F00';
                return '#006400';
            }

            function urtScatterZoom() {
                const z = parseFloat(document.documentElement.style.zoom);
                return isNaN(z) || z === 0 ? 1 : z;
            }

            const tooltip = document.getElementById('tooltip');
            svg.selectAll('.dot')
                .data(data)
                .enter().append('circle')
                .attr('class', 'dot')
                .attr('cx', d => xScale(d.urt_percent_normal))
                .attr('cy', d => yScale(d.urt))
                .attr('r', 6)
                .attr('fill', d => dotColorUrt(d.urt_percent))
                .attr('fill-opacity', 0.85)
                .attr('stroke', '#000').attr('stroke-width', 0.5)
                .on('mouseenter', function(event, d) {
                    if (!tooltip) return;
                    const z = urtScatterZoom();
                    tooltip.style.opacity = '1';
                    tooltip.style.display = 'block';
                    tooltip.innerHTML =
                        `<strong>Станция:</strong> ${stations[d.station] || d.station}<br>` +
                        `<strong>Турбина:</strong> ${d.turbin}<br>` +
                        `<strong>УРТ факт:</strong> ${d.urt.toFixed(2)}<br>` +
                        `<strong>УРТ номинал после КР/СР:</strong> ${d.urt_percent_normal.toFixed(2)}`;
                    tooltip.style.left = (event.pageX / z + 12) + 'px';
                    tooltip.style.top  = (event.pageY / z - tooltip.offsetHeight - 10) + 'px';
                })
                .on('mousemove', function(event) {
                    if (!tooltip) return;
                    const z = urtScatterZoom();
                    tooltip.style.left = (event.pageX / z + 12) + 'px';
                    tooltip.style.top  = (event.pageY / z - tooltip.offsetHeight - 10) + 'px';
                })
                .on('mouseleave', function() {
                    if (!tooltip) return;
                    tooltip.style.opacity = '0';
                    tooltip.style.display = 'none';
                });
        }

        // Возвращает ТА-инциденты для конкретной турбины (stationId, turbinId).
        // ТА = турбоагрегат; если нет " -" в записи — относится ко всей станции.
        function getTurbineIncidents(stationId, turbinId) {
            const stName = stations[stationId] || '';
            const turbinNorm = String(turbinId).replace(/^0+/, '').toLowerCase();
            const aliases = [stName, ...(stationAliases[stName] || [])];

            function filterRecords(records) {
                return (records || []).filter(r => {
                    const stn = r.StationName || '';
                    const stationMatch = aliases.some(a => stn === a || stn.includes(a) || a.includes(stn));
                    if (!stationMatch) return false;

                    const eq = r.EquipmentParameter || '';
                    const dashIdx = eq.indexOf(' -');

                    if (dashIdx === -1) return true;

                    const eqName = eq.substring(0, dashIdx).trim();
                    if (!/^ТА/i.test(eqName)) return false;

                    const eqTurbinId = eqName.replace(/^ТА[-\s]*/i, '').trim();
                    const eqNorm = eqTurbinId.replace(/^0+/, '').toLowerCase();
                    return eqNorm === turbinNorm;
                });
            }

            for (const records of (techParamsHistory || [])) {
                const found = filterRecords(records);
                if (found.length > 0) return found;
            }
            return [];
        }

        let _turbineRatingSorted = [];

        function exportTurbineRating() {
            if (_turbineRatingSorted.length === 0) return;

            let maxInc = 0;
            const dataRows = _turbineRatingSorted.map((d, i) => {
                const stName = stations[d.station] || String(d.station);
                const incidents = getTurbineIncidents(d.station, d.turbin);
                maxInc = Math.max(maxInc, incidents.length);
                return { d, i, stName, incidents };
            });

            const baseHeaders = ['№', 'Станция', 'Турбина', 'УРТ факт', 'УРТ норматив'];
            const incHeaders = Array.from({ length: maxInc }, (_, j) => `Инцидент ${j + 1}`);
            const headers = [...baseHeaders, ...incHeaders];

            const rows = dataRows.map(({ d, i, stName, incidents }) => {
                const base = [i + 1, stName, d.turbin, +d.urt.toFixed(2), +d.urt_percent_normal.toFixed(2)];
                const incs = Array.from({ length: maxInc }, (_, j) => {
                    if (j >= incidents.length) return '';
                    const inc = incidents[j];
                    let text = inc.EquipmentParameter || '';
                    if (inc.ActualValue != null) text += ` — Факт: ${(+inc.ActualValue).toFixed(2)}`;
                    if (inc.NormValue != null)   text += `, Норма: ${(+inc.NormValue).toFixed(2)}`;
                    if (inc.DaysCount)           text += ` (${inc.DaysCount} дн.)`;
                    return text;
                });
                return [...base, ...incs];
            });

            xlsStyledExport(rows, headers, 'Рейтинг турбин', 'turbine_rating.xlsx', 5);
        }

        function drawTurbineRating(data) {
            const list = document.getElementById('turbine-rating-list');
            if (!list) return;
            list.innerHTML = '';

            if (data.length === 0) {
                list.innerHTML = '<div style="color:#666;font-size:12px;padding:4px;">Нет данных</div>';
                _turbineRatingSorted = [];
                return;
            }

            // Сортируем: больше (норма − факт) — выше (т.е. лучшие сверху)
            const sorted = [...data].sort(
                (a, b) => (b.urt_percent_normal - b.urt) - (a.urt_percent_normal - a.urt)
            );
            _turbineRatingSorted = sorted;

            const stationWrapper = document.getElementById('rating-cards-vis');
            const cardFontSize = (stationWrapper && stationWrapper.dataset.calculatedFontSize)
                ? stationWrapper.dataset.calculatedFontSize + 'px'
                : '11px';
            list.style.fontSize = cardFontSize;

            // Для УРТ: положительный % = плохо, отрицательный = хорошо.
            // scoreClass рассчитан на котлы (где наоборот), поэтому передаём -urt_percent.
            function scoreClass(p) {
                if (p < -2) return 'score-value-1';
                if (p < -1) return 'score-value-3';
                if (p <  0) return 'score-value-2';
                if (p <  1) return 'score-value-4';
                return 'score-value-5';
            }

            const tooltip = document.getElementById('tooltip');

            sorted.forEach((d, i) => {
                const stName = stations[d.station] || String(d.station);
                const stShort = stName;
                const pctStr = (d.urt_percent >= 0 ? '+' : '') + d.urt_percent.toFixed(1) + '%';

                const card = document.createElement('div');
                card.className = 'rating-card';
                card.innerHTML =
                    `<div class="rating-info">` +
                        `<span class="rank-number">${i + 1}.</span>` +
                        `<span class="user-name">${stShort}&nbsp;ТА&nbsp;${d.turbin}</span>` +
                    `</div>` +
                    `<span class="${scoreClass(-d.urt_percent)}">${pctStr}</span>`;

                card.addEventListener('mouseenter', function(e) {
                    if (!tooltip) return;
                    const incidents = getTurbineIncidents(d.station, d.turbin);
                    let html = `<strong>${d.turbin} — ${stName}</strong><br>` +
                               `УРТ факт: <strong>${d.urt.toFixed(2)}</strong><br>` +
                               `УРТ номинал после КР/СР: <strong>${d.urt_percent_normal.toFixed(2)}</strong>`;
                    if (incidents.length > 0) {
                        html += `<br><br><strong style="color:#f09">Инциденты:</strong>`;
                        incidents.forEach(inc => {
                            html += `<br>• ${inc.EquipmentParameter}`;
                            if (inc.ActualValue != null) html += ` — Факт: ${(+inc.ActualValue).toFixed(2)}`;
                            if (inc.NormValue != null)   html += `, Норма: ${(+inc.NormValue).toFixed(2)}`;
                            if (inc.DaysCount)           html += ` (${inc.DaysCount} дн.)`;
                        });
                    }
                    const z = parseFloat(document.documentElement.style.zoom) || 1;
                    tooltip.innerHTML = html;
                    tooltip.style.opacity = '1';
                    tooltip.style.display = 'block';
                    tooltip.style.left = (e.pageX / z + 14) + 'px';
                    tooltip.style.top  = (e.pageY / z - tooltip.offsetHeight - 10) + 'px';
                });

                card.addEventListener('mousemove', function(e) {
                    if (!tooltip) return;
                    const z = parseFloat(document.documentElement.style.zoom) || 1;
                    tooltip.style.left = (e.pageX / z + 14) + 'px';
                    tooltip.style.top  = (e.pageY / z - tooltip.offsetHeight - 10) + 'px';
                });

                card.addEventListener('mouseleave', function() {
                    if (!tooltip) return;
                    tooltip.style.opacity = '0';
                    tooltip.style.display = 'none';
                });

                list.appendChild(card);
            });
        }
    </script>
</div>
</body>
