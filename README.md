@using Newtonsoft.Json; 

@{
    ViewData["Title"] = "Альтаир Рейтинг";
}

@model Altair.Models.VisualisationViewModel

<head>
    <meta charset="UTF-8">
    <title>Treemap Visualization</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.8.5/d3.min.js"></script>
    <style>
        .rating-container {
        margin-left: 5px;
        margin-bottom: 5px;
        width: calc(12% + 40px);
        float: left;
        padding: 10px;
        padding-top: 0;
        background-color: #222;
        color: #fff;
        border-radius: 10px;
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
        font-family: sans-serif;
        font-size: 14px;
        line-height: 1.4;
        }
    body {
        display: flex;
        justify-content: space-between;
        background-color: black;
        color: white;
        font-family: sans-serif;
        }
    .rating-card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
    padding: 10px;
    background-color: #333;
    border-radius: 5px;
    box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2);
    transition: transform 0.3s ease;
    cursor: pointer;
    }
    .rating-info {
        display: flex;
        align-items: center;
        flex-grow: 1;
    }
    .radio-btn {
        margin-left: 10px;
        cursor: pointer;
        transform: scale(1);
        transform-origin: center;
    }
    .radio-btn input {
        cursor: pointer;
    }
    .rating-card:nth-child(even) {
        background-color: #2a2a2a;
    }
    .rank-number {
        font-weight: bold;
        color: #fff;
        font-size: 16px;
        margin-right: 10px;
    }
    .user-name {
        font-style: normal;
        font-size: 16px;
        color: #dddddd;
    }
    .score-value-0 {
        margin-left: 4px;
        font-size: 16px;
        font-weight: bold;
        color: #5d5d5d;
    }
    .score-value-1 {
        margin-left: 4px;
        font-size: 16px;
        font-weight: bold;
        color: #8B0000;
    }
    .score-value-2 {
        margin-left: 4px;
        font-size: 16px;
        font-weight: bold;
        color: #c5172c;
    }
    .score-value-3 {
        margin-left: 4px;
        font-size: 16px;
        font-weight: bold;
        color: #FF0000;
    }
    .score-value-4 {
        margin-left: 4px;
        font-size: 16px;
        font-weight: bold;
        color: #009F00;
    }
    .score-value-5 {
        margin-left: 4px;
        font-size: 16px;
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
        background-color: #3a3a3a !important;
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
    border: none;
    border-radius: 5px;
    background-color: #333;
    color: #b3b3b3;
    font-size: 16px;
    text-align: center;
    color: #dddddd;
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
    <div style="margin-top: 5px;">
        <div style="display:flex; width: 100%;"> <!-- Добавил width: 100% -->
            <div style="flex: 1; display: flex; flex-direction: column;"> <!-- Изменил структуру -->
                <div class="graph-container" id="treemap-left" style="text-align: center;">
                    <h2 style="margin-bottom: 4px; display: inline-block;">Котлы</h2>
                </div>
                <div class="graph-container" id="treemap-right" style="text-align: center;">
                    <h2 style="margin-bottom: 4px; display: inline-block;">Турбины</h2>
                </div>
            </div>
            <div class="rating-container">
                <div><h2 style="margin-bottom: 10px;margin-top: 5px;">Рейтинг:</h2><select style="float: right;margin-top: 5px;" id="periodSelect" onchange="reloadPage()">
                    <option value="week">Неделя</option>
                    <option value="month">Месяц</option>
                    <option value="year">Год</option>
                </select> </div>
            </div>
        </div>
    </div>
    
    <div id="tooltip" class="tooltip"></div> <!-- Специальный элемент для всплывающей подсказки -->

    <script>
    const select = document.getElementById('periodSelect');
    const tempData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Turbins));
    if (tempData[0].PeriodType === 0)
      select.value = 'week';
    if (tempData[0].PeriodType === 1)
      select.value = 'month';
    if (tempData[0].PeriodType === 2)
      select.value = 'year';
        function reloadPage() {
            const selectedPeriod = document.getElementById('periodSelect').value;
            const url = `@Url.Action("Visualisation", "Home")?selectedPeriod=${selectedPeriod}`;
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
        const rgres_kpdvalues = {
            "01А": 90.28,
            "01Б": 89.9,
            "02А": 91.41,
            "02Б": 91.28,
            "03А": 90.07,
            "03Б": 89.37,
            "04А": 92.08,
            "04Б": 91.85,
            "05А": 91.63,
            "05Б": 91.7,
            "06А": 88.28,
            "06Б": 88.54,
            "07": 91.37,
            "08": 91.68,
            "09": 90.61,
            "10": 90.42
        };
        const rgres_urtvalues = {
            "1": 1930.4,
            "2": 2006.3,
            "3": 1966.9,
            "4": 1941.6,
            "5": 1880.3,
            "6": 1949.3,
            "7": 1982.2,
            "8": 1950.7,
            "9": 1932.2,
            "10": 1925.7
        };
        const tugres_kpdvalues = {
            "01": 90.86,
            "02": 90.93,
            "03": 90.94,
            "04": 90.18,
            "05": 90.95,
            "06": 90.8,
            "07": 88.76,
            "08": 88.36,
            "09": 89.21,
            "10": 89.9,
            "11": 90.92,
            "12": 91.16,
            "13А": 90.66,
            "13Б": 91.83,
            "14А": 89.32,
            "14Б": 90.77
        };
        const tugres_urtvalues = {
            "1": 2476,
            "2": 2295,
            "3": 2282,
            "4": 2142,
            "5": 2110,
            "6": 2058,
            "7": 2031,
            "8": 2148,
            "9": 2081
        };
        const bgres_kpdvalues = {
            "01А": 90.6,
            "01Б": 90.15,
            "02А": 90.26,
            "02Б": 90.79,
            "03А": 89.86,
            "03Б": 90.62,
            "04А": 89.97,
            "04Б": 88.92,
            "05А": 90.25,
            "05Б": 90.32,
            "06А": 89.45,
            "06Б": 89.03
        };
        const bgres_urtvalues = {
            "1": 2004,
            "2": 2060,
            "3": 2048,
            "4": 2039,
            "5": 2102,
            "6": 1953
        };
        const ngres_kpdvalues = {
            "01А": 91.73,
            "01Б": 89.06,
            "02А": 87.93,
            "02Б": 88.18,
            "03А": 90.46,
            "03Б": 90.91,
            "04А": 91.06,
            "04Б": 89.07,
            "05А": 89.48,
            "05Б": 87.58,
            "06А": 89.85,
            "06Б": 88.58,
            "07А": 87.92,
            "07Б": 88.93
        };
        const ngres_urtvalues = {
            "1": 2360.8,
            "2": 2176.66,
            "3": 2261.85,
            "4": 2225.64,
            "5": 2295.6,
            "6": 2280.1,
            "7": 1934
        };
        const kgres2_kpdvalues = {
            "01А": 91.42,
            "01Б": 91.77,
            "02А": 91.58,
            "02Б": 91.97,
            "03А": 91.58, //нет данных
            "03Б": 91.97, //нет данных
            "04А": 91.7,
            "04Б": 89.54,
            "05А": 89.13,
            "05Б": 91.73,
            "06А": 91.9,
            "06Б": 92.03,
            "07А": 91.89,
            "07Б": 91.56,
            "08А": 92.55,
            "08Б": 91.72,
            "09А": 89.93, //нет данных
            "09Б": 89.93,
            "10А": 90.38,
            "10Б": 88.83
        };
        const kgres2_urtvalues = {
            "1": 2198.3,
            "2": 2228.8,
            "3": 2228.8, //нет данных
            "4": 2228.8, //нет данных
            "5": 1833.3,
            "6": 2174.5,
            "7": 2174.5, //нет данных
            "8": 2085.2,
            "9": 2423.6,
            "10": 2423.6 //нет данных
        };
        const pgres_kpdvalues = {
            "01А": 84.35,
            "01Б": 83.62,
            "02А": 83.62, //нет данных
            "02Б": 83.62, //нет данных
            "03А": 83.62, //нет данных
            "03Б": 83.62, //нет данных
            "04А": 83.7,
            "04Б": 84.7,
            "05": 87.14,
            "06": 87.14, //нет данных
            "07": 87.14, //нет данных
            "08": 78.4,
            "09": 83.62 //нет данных
        };
        const pgres_urtvalues = {
            "1": 2252.83,
            "2": 2252.83, //нет данных
            "3": 2252.83, //нет данных
            "4": 2371,
            "5": 2089,
            "6": 2005.5, 
            "7": 1986.2, //нет данных
            "8": 1986.2,
            "9": 1914.2
        };

                // ============ НОРМАТИВНЫЕ ЗНАЧЕНИЯ ИЗ EXCEL ФАЙЛА ============
        // (С АВТОМАТИЧЕСКИМ ЗАПОЛНЕНИЕМ ПРОПУСКОВ ПО ПРЕДЫДУЩИМ ЗНАЧЕНИЯМ)
        // КЛЮЧИ В КПД: двухзначный формат (01, 02, 03...)
        // БиТЭЦ и БбТЭЦ: добавлен символ М к номеру котла (01М, 02М, 03М...)

        // КрТЭЦ-2 (ID: 2) - лист КТЭЦ-2
        const krtec2_kpdvalues = {
            "01": 91.85,
            "02": 92.69,
            "03": 91.6,
            "04": 90.62,
            "05": 91.13,
            "06": 90.86
        };
        const krtec2_urtvalues = {
            "1": 1994.7,
            "2": 1261.1,
            "3": 1208.2,
            "4": 1208.2 // н/д → заполнено из котла/турбины №3
        };

        // НкТЭЦ (ID: 3) - лист НКТЭЦ
        const nktec_kpdvalues = {
            "08": 91.09,
            "09": 90.96,
            "10": 90.7,
            "11": 91.8,
            "12": 90.45,
            "13": 90.73,
            "14": 91.19,
            "15": 90.4,
            "16": 91.37
        };
        const nktec_urtvalues = {
            "7": 900.0,
            "9": 884.4,
            "11": 2017.0,
            "12": 1945.0,
            "13": 896.0,
            "14": 1683.0,
            "15": 2199.0
        };

        // КрТЭЦ-1 (ID: 4) - лист КТЭЦ-1
        const krtec1_kpdvalues = {
            "04": 89.08,
            "05": 90.41,
            "06": 88.42,
            "07": 89.43,
            "08": 88.43,
            "09": 90.56,
            "10": 90.43,
            "12": 89.32,
            "13": 90.11,
            "14": 89.65,
            "15": 90.03,
            "16": 90.05,
            "18": 90.83
        };
        const krtec1_urtvalues = {
            "3": 2963.7,
            "4": 2963.7, // н/д → заполнено из котла/турбины №3
            "5": 2963.7, // н/д → заполнено из котла/турбины №4
            "6": 2905.6,
            "7": 2905.6, // н/д → заполнено из котла/турбины №6
            "8": 2905.6, // н/д → заполнено из котла/турбины №7
            "9": 1839.0,
            "10": 1839.0, // н/д → заполнено из котла/турбины №9
            "11": 898.4,
            "12": 898.4 // н/д → заполнено из котла/турбины №11
        };

        // БарТЭЦ-2 (ID: 6) - лист БТЭЦ-2
        const bartec2_kpdvalues = {
            "06": 91.49,
            "07": 91.44,
            "10": 91.45,
            "11": 91.82,
            "12": 91.78,
            "13": 91.59,
            "14": 92.33,
            "15": 92.28,
            "16": 91.2,
            "17": 91.58,
            "18": 90.55
        };
        const bartec2_urtvalues = {
            "5": 1025.0,
            "6": 986.0,
            "7": 1031.0,
            "8": 2239.0,
            "9": 2260.0
        };

        // БарТЭЦ-3 (ID: 7) - лист БТЭЦ-3
        const bartec3_kpdvalues = {
            "01": 91.65,
            "02": 92.08,
            "03": 91.87,
            "04": 92.02,
            "05": 91.55
        };
        const bartec3_urtvalues = {
            "1": 1195.0,
            "2": 1070.0,
            "3": 1147.0
        };

        // АбТЭЦ (ID: 8) - лист АТЭЦ
        const abtec_kpdvalues = {
            "01": 90.25,
            "02": 91.9,
            "03": 91.55,
            "04": 92.61,
            "05": 90.86
        };
        const abtec_urtvalues = {
            "1": 2366.9,
            "2": 2230.5,
            "3": 2227.8,
            "4": 2064.1
        };

        // МинТЭЦ (ID: 10) - лист МТЭЦ
        const mintec_kpdvalues = {
            "01": 91.46,
            "02": 91.92
        };
        const mintec_urtvalues = {
            "1": 2214.4
        };

        // КрТЭЦ-3 (ID: 12) - лист КТЭЦ-3
        const krtec3_kpdvalues = {
            "01": 91.03,
            "02": 92.22
        };
        const krtec3_urtvalues = {
        };

        // КанТЭЦ (ID: 13) - лист КанТЭЦ
        const kantec_kpdvalues = {
            "02": 93.26,
            "03": 93.47,
            "04": 92.74,
            "05": 93.44,
            "06": 92.82,
            "07": 93.16
        };
        const kantec_urtvalues = {
            "1": 3565.2,
            "2": 969.3
        };

        // КемТЭЦ (ID: 14) - лист КемТЭЦ
        const kemtec_kpdvalues = {
            "01": 86.61,
            "05": 87.3,
            "08": 87.43,
            "09": 88.28,
            "10": 90.76,
            "11": 90.68
        };
        const kemtec_urtvalues = {
            "2": 1034.0,
            "3": 1007.0,
            "4": 956.0,
            "7": 975.0
        };

        // НТЭЦ-2 (ID: 17) - лист НТЭЦ-2
        const ntec2_kpdvalues = {
            "04": 87.35,
            "05": 88.93,
            "06": 89.54,
            "07": 91.38,
            "08": 91.96,
            "09": 91.34,
            "10": 92.21
        };
        const ntec2_urtvalues = {
            "3": 3201.9,
            "4": 3201.9, // н/д → заполнено из котла/турбины №3
            "5": 3740.3,
            "6": 2329.6,
            "7": 2341.6,
            "8": 2582.7,
            "9": 2576.7
        };

        // НТЭЦ-3 (ID: 18) - лист НТЭЦ-3
        const ntec3_kpdvalues = {
            "07": 89.53,
            "08": 89.18,
            "09": 90.97,
            "10": 91.02,
            "11": 90.87,
            "12": 90.43,
            "13": 90.7,
            "14": 91.43
        };
        const ntec3_urtvalues = {
            "1": 3245.5,
            "7": 1043.0,
            "9": 854.2,
            "10": 860.3,
            "11": 2264.9,
            "12": 2253.1,
            "13": 2194.8,
            "14": 2183.1
        };

        // НТЭЦ-4 (ID: 19) - лист НТЭЦ-4
        const ntec4_kpdvalues = {
            "05": 88.65,
            "06": 90.81,
            "07": 91.71,
            "08": 87.96,
            "09": 90.69,
            "10": 91.77,
            "11": 89.92,
            "12": 89.77
        };
        const ntec4_urtvalues = {
            "4": 3159.9,
            "5": 3273.9,
            "6": 2235.5,
            "7": 2238.1,
            "8": 2320.2
        };

        // НТЭЦ-5 (ID: 20) - лист НТЭЦ-5
        const ntec5_kpdvalues = {
            "01": 90.81,
            "02": 91.04,
            "03": 90.66,
            "04": 89.83,
            "05": 90.62,
            "06": 90.21
        };
        const ntec5_urtvalues = {
            "1": 2115.7,
            "2": 2089.4,
            "3": 2145.3,
            "4": 2141.1,
            "5": 2139.7,
            "6": 2136.4
        };

        // БбТЭЦ (ID: 21) - лист БТЭЦ (Барабинская ТЭЦ)
        const bbtec_kpdvalues = {
            "01М": 90.13,
            "02М": 91.73,
            "03М": 92.23,
            "04М": 90.17,
            "05М": 89.84
        };
        const bbtec_urtvalues = {
            "2": 3290.0,
            "3": 1387.0,
            "4": 1387.0, // н/д → заполнено из котла/турбины №3
            "5": 1387.0 // н/д → заполнено из котла/турбины №4
        };

        // БиТЭЦ (ID: 22) - лист БТЭЦ-1 (Бийская ТЭЦ)
        const bitec_kpdvalues = {
            "07М": 92.2,
            "10М": 91.76,
            "11М": 90.9,
            "12М": 92.54,
            "13М": 92.38,
            "14М": 91.72,
            "15М": 91.42,
            "16М": 90.91
        };
        const bitec_urtvalues = {
            "1": 989.1,
            "4": 1602.7,
            "6": 1702.4,
            "7": 1710.1
        };
        const kemgres_kpdvalues = {
            "03": 87.27,
            "04": 88.08,
            "10": 91.5,
            "11": 92.55,
            "12": 92.22,
            "13": 92.55,
            "14": 91.87
        }
        const kemgres_urtvalues = {
            "3": 899,
            "5": 910,
            "6": 1451,
            "7": 1139,
            "9": 903,
            "10": 987,
            "11": 1112,
            "12": 1338,
            "13": 1625
        }
        
        const kpdvalues = {
            25: rgres_kpdvalues,      // РефГРЭС
            9: tugres_kpdvalues,      // ТуГРЭС
            15: bgres_kpdvalues,      // БелГРЭС
            1: ngres_kpdvalues,       // НазГРЭС
            24: kgres2_kpdvalues,     // КрГРЭС-2
            26: pgres_kpdvalues,      // ПрГРЭС
            2: krtec2_kpdvalues,      // КрТЭЦ-2
            3: nktec_kpdvalues,       // НкТЭЦ
            4: krtec1_kpdvalues,      // КрТЭЦ-1
            6: bartec2_kpdvalues,     // БарТЭЦ-2
            7: bartec3_kpdvalues,     // БарТЭЦ-3
            8: abtec_kpdvalues,       // АбТЭЦ
            10: mintec_kpdvalues,     // МинТЭЦ
            12: krtec3_kpdvalues,     // КрТЭЦ-3
            13: kantec_kpdvalues,     // КанТЭЦ
            14: kemtec_kpdvalues,     // КемТЭЦ
            17: ntec2_kpdvalues,      // НТЭЦ-2
            18: ntec3_kpdvalues,      // НТЭЦ-3
            19: ntec4_kpdvalues,      // НТЭЦ-4
            20: ntec5_kpdvalues,      // НТЭЦ-5
            22: bitec_kpdvalues,      // БиТЭЦ
            21: bbtec_kpdvalues,       // БбТЭЦ
            5: kemgres_kpdvalues
        };

        const urtvalues = {
            25: rgres_urtvalues,      // РефГРЭС
            9: tugres_urtvalues,      // ТуГРЭС
            15: bgres_urtvalues,      // БелГРЭС
            1: ngres_urtvalues,       // НазГРЭС
            24: kgres2_urtvalues,     // КрГРЭС-2
            26: pgres_urtvalues,      // ПрГРЭС
            2: krtec2_urtvalues,      // КрТЭЦ-2
            3: nktec_urtvalues,       // НкТЭЦ
            4: krtec1_urtvalues,      // КрТЭЦ-1
            6: bartec2_urtvalues,     // БарТЭЦ-2
            7: bartec3_urtvalues,     // БарТЭЦ-3
            8: abtec_urtvalues,       // АбТЭЦ
            10: mintec_urtvalues,     // МинТЭЦ
            12: krtec3_urtvalues,     // КрТЭЦ-3
            13: kantec_urtvalues,     // КанТЭЦ
            14: kemtec_urtvalues,     // КемТЭЦ
            17: ntec2_urtvalues,      // НТЭЦ-2
            18: ntec3_urtvalues,      // НТЭЦ-3
            19: ntec4_urtvalues,      // НТЭЦ-4
            20: ntec5_urtvalues,      // НТЭЦ-5
            22: bitec_urtvalues,      // БиТЭЦ
            21: bbtec_urtvalues,       // БбТЭЦ
            5: kemgres_urtvalues
        };


        // Берём данные из C# и превращаем их в пригодный для D3.js формат
    const turbinsData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Turbins));
    const boilersData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Boilers));
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
        const kpd_percent = (((item.KPD - baseKPD) / item.KPD) * 100);
        const kpd_percent_normal = baseKPD;

        return {
            kpd: item.KPD,
            size: item.Production,
            station: item.StationID,
            boiler: item.BoilerID,
            kpd_percent: kpd_percent,
            kpd_percent_normal: kpd_percent_normal
        };
    }) 
};

        // Объединяем данные и сортируем по StationID
        const combinedData = hierarchicalData_turbin.children.concat(hierarchicalData_boiler.children);
        
        
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

        // Проверка на случай, если totalSum равен 0
        return totalSum !== 0 ? (sumUrtPercent + sumKpdPercent)*2 / totalSum : 0;

        }

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
        // Генерируем карточки рейтинга с радиокнопками
        ratings.forEach((rating, index) => {
            const cardDiv = ratingContainer.append('div')
                .attr('class', 'rating-card');
            // Левая часть - информация о станции
            const ratingInfo = cardDiv.append('div')
                .attr('class', 'rating-info')
                .on('click', function() {
                    window.open(stationLinks[rating.StationID], '_blank');
                });
            ratingInfo.append('span')
                .attr('class', 'rank-number')
                .text(index + 1 + '.');
            ratingInfo.append('span')
                .attr('class', 'user-name')
                .text(stations[rating.StationID]);
            ratingInfo.append('span')
                .attr('class', selectColorForRating(rating.rating))
                .text(`${rating.rating.toFixed(2)}%`);
            // Правая часть - радиокнопка
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
        }); 
        // Сбрасываем выделение при клике вне карточек
        document.addEventListener('click', function(e) {
            if (!e.target.closest('.rating-card')) {
                resetHighlight();
                d3.selectAll('input[name="station"]').property('checked', false);
            }
        });

        // Общая ширина окна
        const totalWidth = document.body.clientWidth; // Ограничиваем ширину
        const graphWidth = totalWidth - (totalWidth / 6.8) ; // Ширина одного графика (оставляем пространство между графиками)
        const height = window.innerHeight * 0.85;

        
        
        const tooltip = d3.select("#tooltip");

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
                    const textContent = `${stations[d.data.station]} ТА${d.data.turbin} ${d.data.urt_percent.toFixed(2)}%`;

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

                    // Переменная таймера для скрытия подсказки
                    let hideTimeout;

                    // Функции обработки подсказки
                    function showTooltip(event, d) {
                        const content = `
                            Станция: ${stations[d.data.station]}<br/>
                            Турбина: ${d.data.turbin}<br/>
                            УРТ: ${d.data.urt.toFixed(2)}<br/>
                            Норма УРТ: ${d.data.urt_percent_normal.toFixed(2)}<br/>
                            Расход тепла: ${d.data.size.toFixed(2)}
                        `;
                        
                        clearTimeout(hideTimeout);

                        // Определяем расстояние от нижнего края окна
                        const bottomSpace = window.innerHeight - event.pageY;
                            tooltip.html(content)
                                .style("opacity", 1)
                                .style("top", event.pageY - tooltip.node().offsetHeight + "px")
                                .style("left", event.pageX - 150 + "px");
                    }

                    function moveTooltip(event) {
                        // Здесь логика перемещения аналогична showTooltip,
                        // проверяя свободное пространство и меняя положение
                        const bottomSpace = window.innerHeight - event.pageY;

                            tooltip.style("top", event.pageY - tooltip.node().offsetHeight + "px");
                        tooltip.style("left", event.pageX - 150 + "px");
                    }

                    function hideTooltip() {
                        hideTimeout = setTimeout(() => {
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
               .attr("fill", d => selectColorForKPD(d.data.kpd_percent))
               .on("mouseenter", showTooltip) // Показываем подсказку при наведении
               .on("mousemove", moveTooltip) // Обновляем позицию подсказки
               .on("mouseleave", hideTooltip); // Скрываем подсказку при уходе мыши
            // Добавляем текст, соблюдая ограничения и переносы
            cell.filter(d => (d.x1 - d.x0) >= 38 && (d.y1 - d.y0) >= 40) // Только если ширина достаточна
                .each(function(d) {
                    const g = d3.select(this);
                    const rectWidth = d.x1 - d.x0; // Ширина прямоугольника
                    const rectHeight = d.y1 - d.y0; // Высота прямоугольника
                    const textContent = `${stations[d.data.station]} КА${d.data.boiler} ${d.data.kpd_percent.toFixed(2)}%`;

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
            
                let hideTimeout;
                            // Функции для всплывающих подсказок
                function showTooltip(event, d) {
                    const content = `
                        Станция: ${stations[d.data.station]}<br/>
                        Котёл: ${d.data.boiler}<br/>
                        КПД: ${d.data.kpd.toFixed(2)}<br/>
                        Норма КПД: ${d.data.kpd_percent_normal.toFixed(2)}<br/>
                        Выработка тепла: ${d.data.size.toFixed(2)}
                    `;
                    clearTimeout(hideTimeout);
                    tooltip.html(content)
                        .style("opacity", 1)
                        .style("top", event.pageY + 10 + "px")
                        .style("left", event.pageX - 150 + "px");
                }

                function moveTooltip(event) {
                    tooltip.style("top", event.pageY + 10 + "px")
                        .style("left", event.pageX - 150 + "px");
                }

                function hideTooltip() {
                    hideTimeout = setTimeout(() => {
                        tooltip.style("opacity", 0)
                    },50);
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

        document.getElementById('r_1').addEventListener('click', function () {
            window.location.href = '@Url.Action("Contacts", "Home")';
        });
        document.getElementById('r_2').addEventListener('click', function () {
            window.location.href = '@Url.Action("Index", "Home")';
        });
    </script>
</div>
</body>
