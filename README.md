@using Newtonsoft.Json;
@using Altair.Services;

@{
    ViewData["Title"] = "Альтаир Рейтинг";
}

@model HomeIndexViewModel

<head>
    <meta charset="UTF-8">
    <title>Treemap Visualization</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.8.5/d3.min.js"></script>
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
/* Кнопка для раскрытия графика Экономия */
.economy-toggle-btn {
    background: linear-gradient(135deg, #333 0%, #444 100%);
    border: 1px solid #555;
    border-radius: 8px;
    color: #ddd;
    padding: 8px 20px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
    display: inline-flex;
    align-items: center;
    gap: 8px;
}
.economy-toggle-btn:hover {
    background: linear-gradient(135deg, #444 0%, #555 100%);
    color: #fd817e;
    border-color: #fd817e;
}
.economy-toggle-btn.active {
    background: linear-gradient(135deg, #fd817e 0%, #ff6b6b 100%);
    color: white;
    border-color: #fd817e;
}
#economy-btn-arrow {
    transition: transform 0.3s ease;
}
.economy-toggle-btn.active #economy-btn-arrow {
    transform: rotate(180deg);
}

/* Кликабельное кольцо Итого */
.summary-clickable {
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease;
}
.summary-clickable:hover {
    transform: scale(1.05);
    filter: brightness(1.2);
}
.summary-clickable:active {
    transform: scale(0.98);
}

    html, body {
        overflow: hidden !important;
        height: 100vh;
        margin: 0;
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
    position: fixed;
    background-color: rgba(0, 0, 0, 0.85);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    z-index: 9999;
    opacity: 0;
    transition: opacity 0.05s ease-in-out;
    pointer-events: none;
    white-space: nowrap;
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

.gauges-container{
  flex: 0 0 auto;
  display: flex;
  gap: 12px;
  align-items: stretch;
  justify-content: stretch;
  padding: 0;
  box-sizing: border-box;
  flex-wrap: nowrap;
  min-width: 0;
  height: 240px;
}

    .bars-container{
  flex: 1 0 auto;
  display: flex;
  gap: 8px;
  align-items: stretch;
  justify-content: center;
  box-sizing: border-box;
  margin-top: 10px;
  margin-left: 6px;
  margin-bottom: 8px;
  flex-wrap: wrap;
  max-height: calc(100% - 10px);
  overflow: hidden;
}

.bar-card{
  width: clamp(288px, 46vw, 696px);
  flex: 1 1 clamp(288px, 46vw, 696px);
  background: #222;
  border-radius: 12px;
  padding: 5px;
  box-shadow: 0 2px 8px rgba(0,0,0,.3);
  display: flex;
  flex-direction: column;
}
.mini-title{
  margin: 0 0 5px;
  font-size: 19px;
  color: #dddddd;
  text-align: center;
  letter-spacing: .2px;
}
.gauge-svg,
.mini-svg{
  width: 100%;
  height: auto;                 /* высота следует из viewBox */
  display: block;
}
  /* минималистичная палитра/типографика для баров */
  .mini-legend{
  display:flex; gap:17px; justify-content:center; align-items:center;
  color: #aaaaaa; font:19px/1 sans-serif; margin: 2px 0 10px;
}
.sw{ width:12px; height:12px; border-radius:999px; display:inline-block; margin-right:7px; }
.sw-a{ background:#74c0ff; }   /* Серия 1 - Текущий год (светло-синий) */
.sw-b{ background:#0078A8; }   /* Серия 2 - Прошлый год (синий) */

.sw-boilers{ background:#0078A8; }
.sw-turbines{ background:#74c0ff; }
.multi-legend{
  color: #aaaaaa; font:12px/1 sans-serif;
  margin: 4px 0 6px;
}

.barA-fill{ fill:#74c0ff; }    /* верхняя полоса - Текущий год (светло-синий) */
.barB-fill{ fill:#0078A8; }    /* нижняя полоса - Прошлый год (синий) */
.bar-label{
  fill: #aaaaaa; font:8px/1 sans-serif;
  text-anchor:end; dominant-baseline:middle;
}
.bar-value{
  fill: #dddddd; font:8px/1 sans-serif;
  dominant-baseline:middle;
}
.origin-line{
  stroke: #444;
  stroke-width:1.5;
  shape-rendering:crispEdges;
}
.gauge-card,
.summary-card{
  flex: 1 1 0;
  min-width: 0;
  max-width: none;
  height: 100%;
  background: #222;
  border-radius: 12px;
  padding: 0;
  box-shadow: 0 2px 8px rgba(0,0,0,.3);
  display: flex;
  flex-direction: column;
  align-items: center;
  overflow: hidden;
}

/* Заголовок */
.summary-title{
  margin: 5px 0;
  font-size: 22px;
  color: #dddddd;
  text-align: center;
  letter-spacing: .2px;
}

/* Кольцо-процент (тонкая «пончик»-полоса) */
.summary-ring{
  --p: 78;
  --th: 12px;
  --clr: #0078A8;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: conic-gradient(var(--clr) calc(var(--p)*1%), #333 0);
  -webkit-mask: radial-gradient(farthest-side, transparent calc(50% - var(--th)), #000 calc(50% - var(--th) + 1px));
          mask: radial-gradient(farthest-side, transparent calc(50% - var(--th)), #000 calc(50% - var(--th) + 1px));
  position: relative;
  margin-bottom: 10px;
}
.gauge-title{
  margin: 5px 0;
  font-size: 22px;
  color: #dddddd;
  text-align: center;
  letter-spacing: .2px;
}

/* Итоги: число справа от кольца */
.summary-ring .value{ display:none; }
.summary-inline{ display:flex; align-items:center; gap:10px; justify-content:center; margin-bottom:8px; }
.summary-big{ color: white; font:700 26px/1 sans-serif; }

/* Два KPI-бейджа под кольцом */
.kpis{
  display:grid; grid-template-columns: 1fr 1fr; gap:8px; width:100%; max-width:480px;
}
.kpi{
  background: #2a2a2a; border-radius:10px; padding:8px 10px;
  display:flex; flex-direction:column; gap:3px;
}
.kpi-label{ color: #aaaaaa; font:12px/1 sans-serif; text-transform:uppercase; letter-spacing:.2px; }
.kpi-value{ color: #dddddd; font:700 17px/1 sans-serif; }

/* Контейнер и карточки под линейные графики */
.lines-container{
  display:flex;
  flex-direction:column;  /* большие графики — один под другим */
  gap:10px;
  margin-top:10px;
}
.line-card{
  margin-left: 10px;
  width:100% -10px;
  background: #222;
  border-radius:12px;
  padding:10px 12px;
  box-shadow:0 2px 10px rgba(0,0,0,.35);
  display: flex;
  flex-direction: column;
}
.line-title{
  margin:0 0 6px;
  font-size:20px;
  color: #dddddd;
  text-align:center;
  letter-spacing:.2px;
}
.line-svg{ width:100%; flex: 1; display:block; min-height: 200px; }

/* Оси/сетка - адаптивные к теме */
.axis path, .axis line{ stroke: #444; }
.axis text{ fill: #aaaaaa; font:12px sans-serif; }
.grid line{ stroke: #2a2a2a; }
.grid .domain{ stroke:none; }

/* Цвета серий */
.line-boilers{ stroke:#0078A8; fill:none; stroke-width:2.5; }
.line-turbines{ stroke:#ffd400; fill:none; stroke-width:2.5; }
.line-total{ stroke:#EF4156; fill:none; stroke-width:2.8; }

.dot{ stroke:#111; stroke-width:1.5; }
.dot-boilers{ fill:#0078A8; }
.dot-turbines{ fill:#ffd400; }
.dot-total{ fill:#EF4156; }

/* Подпись значения справа от общей серии (большим шрифтом) */
.total-big{
  color: white; font:700 28px/1 sans-serif;
  margin-left:auto; margin-right:4px; /* прижать вправо в заголовке */
}
/* Заливки "прошлый год" — те же цвета, но полупрозрачные */
.area-boilers-prev  { fill: rgba(116,192,255,.25); }
.area-turbines-prev { fill: rgba(255, 212, 0, 0.25); }
.area-total-prev    { fill: rgba(255, 116, 116, 0.25); }

/* Маленькие плашки в легенде */
.sw-line-boilers{ background:#0078A8; }
.sw-line-turbines{ background:#ffd400; }
.sw-line-total{ background:#EF4156; }

.sw-area-boilers{ background:rgba(116,192,255,.25); border:1px solid #0078A8; }
.sw-area-turbines{ background:rgba(255,212,0,.25); border:1px solid #ffd400; }
.sw-area-total{ background:rgba(255, 116, 116, 0.25); border:1px solid #EF4156; }

.multi-legend{
  color: #aaaaaa; font:12px/1 sans-serif; margin: 4px 0 6px; text-align:center;
}
.multi-legend .sw{
  width:12px; height:12px; border-radius:3px; display:inline-block; margin:0 6px 0 12px;
  vertical-align:middle;
}

/* Панель управления графиком */
.line-controls{
  display:flex;
  gap:14px;
  align-items:center;
  justify-content:center;
  margin:4px 0 6px;
}

/* Тумблер */
.switch{
  position:relative; display:inline-block;
  width:44px; height:24px;
}
.switch input{ display:none; }
.switch .slider{
  position:absolute; inset:0;
  background:#444; border-radius:999px;
  transition:.2s;
}
.switch .slider:before{
  content:""; position:absolute; height:18px; width:18px;
  left:3px; top:3px; background:#fff; border-radius:50%; transition:.2s;
}
.switch input:checked + .slider{ background:#00d26a; }
.switch input:checked + .slider:before{ transform:translateX(20px); }

.switch-label{
  color:#bdbdbd; font:12px/1 sans-serif; margin-left:4px; min-width:46px; text-align:left;
}

/* Карточка с много-серийным графиком — нужна для абсолютного позиционирования панели */
.line-card{
  position: relative; /* НОВОЕ */
}

/* Заголовок: оставим центр, но зарезервируем место справа под тумблеры */
.line-card .line-title{
  padding-right: 130px; /* чтобы заголовок не пересекался с панелью справа */
}

/* ПАНЕЛЬ ТУМБЛЕРОВ — переносим в правый верхний угол карточки */
.line-controls{
  position:absolute;    /* НОВОЕ: фиксируем в углу */
  top:8px;              /* отступ сверху */
  right:10px;           /* отступ справа */
  display:flex;
  gap:14px;
  align-items:center;
  justify-content:flex-end;
  margin:0;             /* был margin — убираем */
  z-index: 2;           /* поверх легенды на узких экранах */
}

/* Тумблер — нейтральная серо-графитовая палитра */
.switch{
  position:relative; display:inline-block;
  width:44px; height:24px;
}
.switch input{ display:none; }
.switch .slider{
  position:absolute; inset:0;
  background:#555;              /* базовый фон (выкл) */
  border-radius:999px;
  transition:.2s;
}
.switch .slider:before{
  content:""; position:absolute; height:18px; width:18px;
  left:3px; top:3px;
  background:#e9eaee;           /* нейтральный «белый» */
  border-radius:50%;
  box-shadow:0 1px 2px rgba(0,0,0,.35);
  transition:.2s;
}
/* было #00d26a — СМЕНИЛИ на нейтральный */
.switch input:checked + .slider{ background:#8f9399; }  /* серо-стальной */
.switch input:checked + .slider:before{ transform:translateX(20px); }

.switch-label{
  color: #aaaaaa; font:12px/1 sans-serif;
  margin-left:4px; min-width:46px; text-align:left;
}


    </style>
</head>
<body>
    <div style="margin-top: 5px; margin-right: 210px; height: calc(100vh - 5px); overflow: hidden; box-sizing: border-box;">
        <!-- Панель выбора периода -->
        <div class="period-panel" style="width: 100%; display: flex; justify-content: center; align-items: center; gap: 15px; margin-bottom: 10px; padding: 10px; background-color: #222; border-radius: 10px; margin-left: 5px; box-sizing: border-box;">
            <div style="display: flex; gap: 10px; align-items: center;">
                <span style="color: #dddddd; font-size: 16px; font-weight: bold;">Период:</span>
                <select id="periodSelectMain" onchange="onPeriodChangeMain()">
                    <option value="day">День</option>
                    <option value="month">Месяц</option>
                    <option value="year">Год</option>
                </select>
            </div>
            <div style="display: flex; gap: 10px; align-items: center;">
                <span style="color: #dddddd; font-size: 16px; font-weight: bold;">Дата:</span>
                <select id="dateSelectMain" onchange="reloadPageMain()" style="min-width: 150px;"></select>
            </div>
        </div>

        <div style="display:flex; width: 100%; align-items:stretch;">
            <div style="width: 100%; display: flex; flex-direction: column;">
                <div class="gauges-container" style="margin-left: 5px;">
                    <div class="gauge-card">
                    <h3 class="gauge-title">Котлы (КПД)</h3>
                    <svg id="gauge-1" class="gauge-svg" viewBox="0 20 320 160" preserveAspectRatio="xMidYMid meet"></svg>
                    </div>

                    <div class="summary-card" id="summary-card-click" style="cursor: pointer;">
                    <h3 class="summary-title" style="margin-bottom: 10px;">Итого</h3>

                    <!-- Кольцо с процентом (кликабельное) -->
                    <div class="summary-inline summary-clickable">
                    <div class="summary-ring" id="summary-ring"></div>
                    <div class="summary-big" id="summary-percent-out">0%</div>
                    </div>

                    <!-- Два KPI: Экономия (отрицательные) / Перерасход (положительные) -->
                    <div class="kpis" style="margin-bottom: 8px;">
                        <div class="kpi" style="position: relative;">
                        <div class="kpi-label" id="kpi-rub-label">Резервы, ₽</div>
                        <div class="kpi-value" id="kpi-rub">—</div>
                        </div>
                        <div class="kpi" style="position: relative;">
                        <div class="kpi-label" id="kpi-tut-label">Резервы, ТУТ</div>
                        <div class="kpi-value" id="kpi-tut">—</div>
                        </div>
                    </div>
                    </div>

                    <div class="gauge-card">
                    <h3 class="gauge-title">Турбины (УРТ)</h3>
                    <svg id="gauge-2" class="gauge-svg" viewBox="0 20 320 160" preserveAspectRatio="xMidYMid meet"></svg>
                    </div>
                </div>
                <div class="bars-container">
                    <!-- Блоки с барами (по умолчанию видны) -->
                    <div id="bars-content" style="display: flex; gap: 8px; width: 100%;">
                        <div class="bar-card" style="position: relative;">
                            <span style="position:absolute;top:6px;right:8px;background:#e67e22;color:#fff;font-size:10px;padding:2px 7px;border-radius:8px;z-index:2;">в разработке</span>
                            <div class="mini-title">Котлы - потери тепла (КПД)</div>
                            <div class="mini-legend"><span class="sw sw-a"></span>Текущий год<span class="sw sw-b"></span>Прошлый год</div>
                            <svg id="bars-left" class="mini-svg"></svg>
                        </div>

                        <div class="bar-card" style="position: relative;">
                            <span style="position:absolute;top:6px;right:8px;background:#e67e22;color:#fff;font-size:10px;padding:2px 7px;border-radius:8px;z-index:2;">в разработке</span>
                            <div class="mini-title">Турбины - резервы (РТЭ)</div>
                            <div class="mini-legend"><span class="sw sw-a"></span>Текущий год<span class="sw sw-b"></span>Прошлый год</div>
                            <svg id="bars-right" class="mini-svg"></svg>
                        </div>
                    </div>

                    <!-- График Экономия (по умолчанию скрыт) -->
                    <div id="economy-chart-container" class="line-card" style="display: none; flex: 1; margin: 0; margin-left: 0; width: 100%; max-height: 100%; overflow: hidden;">
                        <div class="line-title" style="text-align:center;">Котлы / Турбины / Итоги (сумма)</div>

                        <!-- ПАНЕЛЬ ПЕРЕКЛЮЧАТЕЛЕЙ -->
                        <div class="line-controls">
                        <!-- ₽ / ТУТ (по умолчанию checked = рубли) -->
                        <label class="switch">
                            <input type="checkbox" id="toggle-currency" checked>
                            <span class="slider"></span>
                        </label>
                        <span class="switch-label" id="label-currency">₽</span>

                        <!-- Накопительно -->
                        <label class="switch" style="margin-left:18px;">
                            <input type="checkbox" id="toggle-cum">
                            <span class="slider"></span>
                        </label>
                        <span class="switch-label">Накопительно</span>
                        </div>

                        <!-- ЛЕГЕНДА (текущий год — линия, прошлый — заливка) -->
                        <div class="multi-legend">
                        <span class="sw sw-line-boilers"></span>Котлы (тек.)
                        <span class="sw sw-area-boilers"></span>Котлы (прошл.)
                        <span class="sw sw-line-turbines" style="margin-left:14px;"></span>Турбины (тек.)
                        <span class="sw sw-area-turbines"></span>Турбины (прошл.)
                        <span class="sw sw-line-total" style="margin-left:14px;"></span>Итоги (тек.)
                        <span class="sw sw-area-total"></span>Итоги (прошл.)
                        </div>

                        <svg id="line-multi" class="line-svg" viewBox="0 0 1000 270" preserveAspectRatio="xMidYMid meet"></svg>
                    </div>
                </div>

            </div>
            </div>
        </div>
    </div>
    <div class="rating-container" id="rating-container-index">
        <div class="rating-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; margin-top: 5px;">
            <h2 style="margin: 0; font-size: 14px;">Рейтинг:</h2>
            <button id="toggle-all-index" class="toggle-all-btn" title="Выбрать/Снять все" style="font-size: 14px; padding: 2px 8px;">☑</button>
        </div>
        <div class="rating-cards-wrapper" id="rating-cards-index"></div>
    </div>
    
    <div id="tooltip" class="tooltip"></div> <!-- Специальный элемент для всплывающей подсказки -->

    <script>
    // Текущее состояние из модели
    const currentPeriodType = @((int)Model.SelectedPeriod);
    const currentSelectedDate = '@(Model.SelectedDate?.ToString("yyyy-MM-dd") ?? "")';

    // Панель выбора периода (вверху)
    const periodSelectMain = document.getElementById('periodSelectMain');
    const dateSelectMain = document.getElementById('dateSelectMain');

    // Устанавливаем текущий тип периода
    if (currentPeriodType === 0) periodSelectMain.value = 'day';
    else if (currentPeriodType === 1) periodSelectMain.value = 'month';
    else if (currentPeriodType === 2) periodSelectMain.value = 'year';

    // Загружаем доступные даты при загрузке страницы
    document.addEventListener('DOMContentLoaded', () => loadAvailableDates());

    async function loadAvailableDates() {
        const period = periodSelectMain.value;
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

            dateSelectMain.innerHTML = '';

            if (period === 'year') {
                if (data.years && data.years.length > 0) {
                    data.years.forEach(y => {
                        const opt = document.createElement('option');
                        opt.value = y.date;
                        opt.textContent = y.displayText;
                        dateSelectMain.appendChild(opt);
                    });
                }
            } else {
                if (data.dates) {
                    data.dates.forEach(d => {
                        const option = document.createElement('option');
                        option.value = d.date;
                        option.textContent = d.displayText;
                        dateSelectMain.appendChild(option);
                    });
                }
            }

            // Устанавливаем текущую выбранную дату если есть
            if (currentSelectedDate && dateSelectMain.querySelector(`option[value="${currentSelectedDate}"]`)) {
                dateSelectMain.value = currentSelectedDate;
            }
        } catch (error) {
            console.error('Ошибка загрузки дат:', error);
        }
    }

    // Функции для панели выбора периода
    function onPeriodChangeMain() {
        loadAvailableDates().then(() => {
            if (dateSelectMain.options.length > 0) {
                reloadPageMain();
            }
        });
    }

    function reloadPageMain() {
        const selectedPeriod = periodSelectMain.value;
        const selectedDate = dateSelectMain.value;
        // Сохраняем в localStorage для синхронизации между вкладками
        localStorage.setItem('selectedPeriod', selectedPeriod);
        if (selectedDate) {
            localStorage.setItem('selectedDate', selectedDate);
        } else {
            localStorage.removeItem('selectedDate');
        }
        let url = `@Url.Action("Index", "Home")?selectedPeriod=${selectedPeriod}`;
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

        // Флаг исключения турбин ТЭЦ (читаем из localStorage, по умолчанию true)
        const excludeTecTurbinesFlag = localStorage.getItem('excludeTecTurbines') !== 'false';

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


        // ============ КОНЕЦ БЛОКА НОРМАТИВНЫХ ЗНАЧЕНИЙ ============


        // [УДАЛЕНО: Все захардкоженные константы теперь загружаются динамически из normative_config.xlsx]
        // Берём данные из C# и превращаем их в пригодный для D3.js формат
    const turbinsData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Turbines));
    const boilersData = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Boilers));
    const PRECALC_RESERVES_RUB = @Model.ReservesRub.ToString(System.Globalization.CultureInfo.InvariantCulture);
    const RESERVES_RUB_BY_STATION = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.ReservesRubByStation));
    // Цена ТУТ по станциям (руб./ТУТ). Пустой объект для годового периода.
    const PRICE_BY_STATION = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.PriceByStation));
    // Для годового периода: рубли по станциям = SUM месячных reserves_rub (не yearly aggregate).
    // Гарантирует совпадение с суммой месячных значений.
    const YEARLY_RUB_BY_STATION = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.YearlyRubByStationFromMonthly));
    // Только турбины по станциям за год — для исключения ТЭЦ-турбин при активной галочке.
    const YEARLY_TURB_RUB_BY_STATION = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.YearlyTurbRubByStation));

    // Читаем настройку исключения турбин ТЭЦ из localStorage (по умолчанию true)
    const excludeTecFromUrl = localStorage.getItem('excludeTecTurbines') !== 'false';

    // Читаем состояние выбора оборудования из localStorage
    const _eqStateRaw = (() => {
        try { return JSON.parse(localStorage.getItem('equipmentSelectionState') || '{}'); }
        catch(e) { return {}; }
    })();
    const eqTurbines = _eqStateRaw.turbines || {};
    const eqBoilers  = _eqStateRaw.boilers  || {};

    // Вычисляем долю ВКЛЮЧЁННОГО оборудования по каждой станции (для пропорционального пересчёта
    // годовых рублей и графика, где данные хранятся как агрегаты на уровне станции).
    const stationTurbFrac = {};  // stId -> доля потребления включённых турбин (0–1)
    const stationBoilFrac = {};  // stId -> доля выработки включённых котлов (0–1)
    (() => {
        const tTotal = {}, tIncl = {}, bTotal = {}, bIncl = {};
        turbinsData.forEach(item => {
            const sid = item.StationID;
            const con = item.Consumption || 0;
            tTotal[sid] = (tTotal[sid] || 0) + con;
            const key = `${sid}_${item.TurbinID}`;
            const isTec = tecStations.includes(sid);
            const excl = (eqTurbines[key] === false) || (excludeTecFromUrl && isTec);
            if (!excl) tIncl[sid] = (tIncl[sid] || 0) + con;
        });
        boilersData.forEach(item => {
            const sid = item.StationID;
            const prod = item.Production || 0;
            bTotal[sid] = (bTotal[sid] || 0) + prod;
            const key = `${sid}_${item.BoilerID}`;
            if (eqBoilers[key] !== false) bIncl[sid] = (bIncl[sid] || 0) + prod;
        });
        for (const sid of Object.keys(tTotal)) {
            const total = tTotal[sid];
            stationTurbFrac[parseInt(sid)] = total > 0 ? (tIncl[sid] || 0) / total : 0;
        }
        for (const sid of Object.keys(bTotal)) {
            const total = bTotal[sid];
            stationBoilFrac[parseInt(sid)] = total > 0 ? (bIncl[sid] || 0) / total : 0;
        }
    })();

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

            return {
                urt: item.URT,
                size: shouldExclude ? 0 : item.Consumption, // Обнуляем вес для ТЭЦ турбин
                station: item.StationID,
                turbin: item.TurbinID,
                urt_percent: shouldExclude ? 0 : (((item.URT - nominalValue) / nominalValue) * 100),
                urt_percent_normal: nominalValue,
                excluded: shouldExclude, // Флаг для визуализации
                generation_ee: item.GenerationEE || 0,
                kpd_netto: item.KpdNetto || 0,
                koeff_tp: item.KoeffTP || 0
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

        const equipKeyB = `${item.StationID}_${item.BoilerID}`;
        const isBoilerEnabled = eqBoilers[equipKeyB] !== false;

        return {
            kpd: item.KPD,
            size: isBoilerEnabled ? item.Production : 0,  // Исключённые котлы обнуляем
            consumption: item.Consumption,   // Для tooltip сохраняем расход топлива
            station: item.StationID,
            boiler: item.BoilerID,
            kpd_percent: isBoilerEnabled ? kpd_percent : 0,
            kpd_percent_normal: kpd_percent_normal
        };
    })
};

        // Объединяем данные и сортируем по StationID
        const combinedData = hierarchicalData_turbin.children.concat(hierarchicalData_boiler.children);
        
        
        // Функция для вычисления рейтинга
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
const temp = stationsWithoutData.filter(station => station.hasData !== true);
// Объединяем: сначала станции с данными, потом без данных
const allRatings = [...ratings, ...temp];

// Сохраняем рейтинг в localStorage для синхронизации с Visualisation
localStorage.setItem('altairRatings', JSON.stringify(allRatings));

// Выбираем контейнер рейтинга (карточки добавляем в wrapper)
const ratingContainer = d3.select('#rating-cards-index');

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

    // Чекбокс справа (только для станций с данными)
    if (rating.hasData) {
        const checkboxDiv = cardDiv.append('div')
            .attr('class', 'radio-btn')
            .on('click', function(e) {
                e.stopPropagation();
                // Сохраняем состояние чекбоксов в localStorage
                saveCheckboxState();
            });

        // Загружаем состояние из localStorage
        const savedState = JSON.parse(localStorage.getItem('stationCheckboxState') || '{}');
        const isChecked = savedState[rating.StationID] !== undefined ? savedState[rating.StationID] : true;

        checkboxDiv.append('input')
            .attr('type', 'checkbox')
            .attr('name', 'station-index')
            .attr('data-station-id', rating.StationID)
            .attr('id', `station-index-${rating.StationID}`)
            .property('checked', isChecked);
    }
});

// Функция сохранения состояния чекбоксов и обновления гейджей
function saveCheckboxState() {
    const state = {};
    d3.selectAll('input[name="station-index"]').each(function() {
        const stationId = d3.select(this).attr('data-station-id');
        state[stationId] = d3.select(this).property('checked');
    });
    localStorage.setItem('stationCheckboxState', JSON.stringify(state));
    // Обновляем гейджи при изменении чекбоксов
    if (typeof updateAllGauges === 'function') {
        updateAllGauges();
    }
}

// Кнопка "Выбрать/Снять все" для Index
// Определяем начальное состояние на основе сохранённых чекбоксов
const savedStateInit = JSON.parse(localStorage.getItem('stationCheckboxState') || '{}');
let allCheckedIndex = Object.keys(savedStateInit).length === 0 || Object.values(savedStateInit).every(v => v === true);
document.getElementById('toggle-all-index').textContent = allCheckedIndex ? '☑' : '☐';

document.getElementById('toggle-all-index').addEventListener('click', function() {
    allCheckedIndex = !allCheckedIndex;
    d3.selectAll('input[name="station-index"]').property('checked', allCheckedIndex);
    this.textContent = allCheckedIndex ? '☑' : '☐';
    saveCheckboxState();
});

// Автоподбор размера шрифта для рейтинга
function fitRatingFontSize() {
    const container = document.getElementById('rating-container-index');
    const wrapper = document.getElementById('rating-cards-index');
    if (!container || !wrapper) return;

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
    if (cards.length === 0) return;

    // Начинаем с большого шрифта и уменьшаем пока не поместится
    let fontSize = 20;
    const minFontSize = 6;

    while (fontSize >= minFontSize) {
        wrapper.style.fontSize = fontSize + 'px';

        // Проверяем, помещается ли всё по высоте
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

        if (totalHeight <= cardsAvailableHeight && textFits) {
            const finalSize = fontSize - 1;
            wrapper.style.fontSize = finalSize + 'px';
            break;
        }
        fontSize -= 0.5;
    }
}

// Вызываем после загрузки и при ресайзе
setTimeout(fitRatingFontSize, 100);
window.addEventListener('resize', fitRatingFontSize);

    function drawGauge(svgId, value, { start = -Math.PI/2, end = Math.PI/2, orient = 'auto', displayMultiplier = 1 } = {}){
  const svg = d3.select('#'+svgId);
  svg.selectAll('*').remove();

  // Геометрия
  const W = 320, H = 180;
  const cx = W/2;
  const rOuter = 130;
  const thickness = 45;
  const rInner = 85;
  const cy = 160;

  // Определяем цвет на основе значения: красный для отрицательных, зелёный для неотрицательных
  const arcColor = value < 0 ? '#EF4156' : '#00d26a';
  const textColor = value < 0 ? '#EF4156' : '#00d26a';

  // Кламп значения для дуги (от 0 до 100, где 50 = 0%)
  // Значение от -5% до +5% отображается от 0 до 100
  const normalizedValue = Math.max(0, Math.min(100, 50 + value * 10)); // -5% = 0, 0% = 50, +5% = 100

  // Группа в центре
  const gRoot = svg.append('g').attr('transform', `translate(${cx},${cy})`);

  // Шкала угла
  const scale = d3.scaleLinear().domain([0,100]).range([start, end]);

  // Фон дуги (полная дуга серая)
  gRoot.append('path')
    .attr('d', d3.arc().innerRadius(rInner).outerRadius(rOuter).startAngle(start).endAngle(end))
    .attr('fill', '#3a3a3a');

  // Центральная линия (ноль) - тонкая вертикальная линия посередине
  gRoot.append('line')
    .attr('x1', 0).attr('y1', -rOuter)
    .attr('x2', 0).attr('y2', -rInner)
    .attr('stroke', '#666')
    .attr('stroke-width', 2);

  // Заполненная часть дуги - один цвет без градиента
  gRoot.append('path')
    .attr('d', d3.arc().innerRadius(rInner).outerRadius(rOuter).startAngle(start).endAngle(scale(normalizedValue)))
    .attr('fill', arcColor);

  // Число по центру с цветом зависящим от знака
  // displayMultiplier позволяет отображать значение с другим знаком (напр. -1 для УРТ)
  const displayValue = value * displayMultiplier;
  const c = displayValue > 0 ? '+' : '';
  gRoot.append('text')
    .attr('x', 0).attr('y', -10)
    .attr('text-anchor','middle')
    .attr('fill', textColor)
    .attr('font-family','sans-serif').attr('font-size','28px').attr('font-weight','700')
    .text(`${c}${displayValue.toFixed(2)}%`);
}
// ---- Расчёт средневзвешенного рейтинга для котлов и турбин ----
// Для котлов: взвешиваем по выработке тепла (Production/size)
// Для турбин: взвешиваем по расходу топлива (Consumption/size)

function getSelectedStations() {
    const savedState = JSON.parse(localStorage.getItem('stationCheckboxState') || '{}');
    // Если состояние пустое - все станции выбраны
    if (Object.keys(savedState).length === 0) return null; // null означает "все"
    // Возвращаем массив ID выбранных станций
    return Object.entries(savedState)
        .filter(([id, checked]) => checked)
        .map(([id]) => parseInt(id));
}

function calculateWeightedRatingBoilers(boilersArray, selectedStations) {
    // boilersArray - массив объектов с kpd_percent и size (Production)
    let sumRatingWeighted = 0;
    let totalWeight = 0;

    boilersArray.forEach(boiler => {
        // Фильтруем по выбранным станциям
        if (selectedStations !== null && !selectedStations.includes(boiler.station)) return;

        const rating = boiler.kpd_percent || 0;
        const weight = boiler.size || 0; // Production - выработка тепла

        if (weight > 0) {
            sumRatingWeighted += rating * weight;
            totalWeight += weight;
        }
    });

    return totalWeight > 0 ? sumRatingWeighted / totalWeight : 0;
}

function calculateWeightedRatingTurbines(turbinesArray, selectedStations) {
    // turbinesArray - массив объектов с urt_percent и size (Consumption)
    let sumRatingWeighted = 0;
    let totalWeight = 0;

    turbinesArray.forEach(turbine => {
        // Фильтруем по выбранным станциям
        if (selectedStations !== null && !selectedStations.includes(turbine.station)) return;

        // Для турбин: чем выше УРТ, тем хуже. Рейтинг уже вычислен как urt_percent
        // Отрицательное значение urt_percent означает экономию (лучше нормы)
        const rating = turbine.urt_percent || 0;
        const weight = turbine.size || 0; // Consumption - расход топлива

        if (weight > 0) {
            sumRatingWeighted += rating * weight;
            totalWeight += weight;
        }
    });

    return totalWeight > 0 ? sumRatingWeighted / totalWeight : 0;
}

// ---- Расчёт резервов в ТУТ ----
// ТУТ_total_станции = −B_total × stationRating / 100
// где stationRating = ((100 + boilerRating) / (100 + turbineRating) − 1) × 100
// ТУТ_котлов = Σ (KPD_norm − KPD_fact)/100 × b.size
// ТУТ_турбин = ТУТ_total − ТУТ_котлов

function calculateReserves(selectedStations) {
    const boilersByStation = {};
    hierarchicalData_boiler.children.forEach(b => {
        if (selectedStations !== null && !selectedStations.includes(b.station)) return;
        if (b.size <= 0) return;
        if (!boilersByStation[b.station]) boilersByStation[b.station] = [];
        boilersByStation[b.station].push(b);
    });
    const turbinesByStation = {};
    hierarchicalData_turbin.children.forEach(t => {
        if (selectedStations !== null && !selectedStations.includes(t.station)) return;
        if (t.excluded) return;
        if (t.size <= 0) return;
        if (!turbinesByStation[t.station]) turbinesByStation[t.station] = [];
        turbinesByStation[t.station].push(t);
    });
    let totalTUT = 0;
    Object.keys(boilersByStation).forEach(stId => {
        const boilers = boilersByStation[stId];
        const turbines = turbinesByStation[stId] || [];
        const B_size = boilers.reduce((acc, b) => acc + b.size, 0);
        if (B_size <= 0) return;
        const B_consumption = boilers.reduce((acc, b) => acc + b.consumption, 0);
        const boilerRating = boilers.reduce((acc, b) => acc + b.kpd_percent * b.size, 0) / B_size;
        const turbineWeightSum = turbines.reduce((acc, t) => acc + t.size, 0);
        const turbineRating = turbineWeightSum > 0
            ? turbines.reduce((acc, t) => acc + t.urt_percent * t.size, 0) / turbineWeightSum : 0;
        const stationRating = ((100 + boilerRating) / (100 + turbineRating) - 1) * 100;
        totalTUT += -B_consumption * stationRating / 100;
    });
    return totalTUT;
}

// Начальные значения
let GAUGE_LEFT = 0;
let GAUGE_RIGHT = 0;
let RESERVES_TUT = 0;
let RESERVES_RUB = PRECALC_RESERVES_RUB;

// Расчёт рублей: ТУТ_по_станции × цена_по_станции.
// Для суточного и месячного периода: берём цену из PRICE_BY_STATION (загружена из raw_fuel_prices_monthly).
// Для годового периода: PRICE_BY_STATION пустой — суммируем месячные рубли из YEARLY_RUB_BY_STATION.
// Фильтры станций и галочка ТЭЦ работают идентично calculateReserves().
function calculateReservesRub(selectedStations) {
    // Годовой период — суммируем месячные рубли из БД по выбранным станциям.
    // Применяем пропорциональное масштабирование на основе доли включённого оборудования.
    if (Object.keys(PRICE_BY_STATION).length === 0) {
        let total = 0;
        for (const [stId, val] of Object.entries(YEARLY_RUB_BY_STATION)) {
            const stationId = parseInt(stId);
            if (selectedStations !== null && !selectedStations.includes(stationId)) continue;
            let stationTurbRub = YEARLY_TURB_RUB_BY_STATION[stId] || 0;
            let stationBoilRub = val - stationTurbRub;
            // Применяем исключение ТЭЦ: обнуляем долю турбин для ТЭЦ-станций
            if (excludeTecFromUrl && tecStations.includes(stationId)) {
                stationTurbRub = 0;
            }
            // Пропорциональный учёт включённого оборудования внутри станции
            const turbFrac = stationTurbFrac[stationId] !== undefined ? stationTurbFrac[stationId] : 1.0;
            const boilFrac = stationBoilFrac[stationId] !== undefined ? stationBoilFrac[stationId] : 1.0;
            total += stationTurbRub * turbFrac + stationBoilRub * boilFrac;
        }
        return total;
    }

    // Суточный/месячный период: считаем ТУТ_total по каждой станции, умножаем на цену
    const tutByStation = {};

    const _boilersByStation = {};
    hierarchicalData_boiler.children.forEach(b => {
        if (selectedStations !== null && !selectedStations.includes(b.station)) return;
        if (b.size <= 0) return;
        if (!_boilersByStation[b.station]) _boilersByStation[b.station] = [];
        _boilersByStation[b.station].push(b);
    });
    const _turbinesByStation = {};
    hierarchicalData_turbin.children.forEach(t => {
        if (selectedStations !== null && !selectedStations.includes(t.station)) return;
        if (t.excluded) return;
        if (t.size <= 0) return;
        if (!_turbinesByStation[t.station]) _turbinesByStation[t.station] = [];
        _turbinesByStation[t.station].push(t);
    });
    Object.keys(_boilersByStation).forEach(stId => {
        const boilers = _boilersByStation[stId];
        const turbines = _turbinesByStation[stId] || [];
        const B_size = boilers.reduce((acc, b) => acc + b.size, 0);
        if (B_size <= 0) return;
        const B_consumption = boilers.reduce((acc, b) => acc + b.consumption, 0);
        const boilerRating = boilers.reduce((acc, b) => acc + b.kpd_percent * b.size, 0) / B_size;
        const turbineWeightSum = turbines.reduce((acc, t) => acc + t.size, 0);
        const turbineRating = turbineWeightSum > 0
            ? turbines.reduce((acc, t) => acc + t.urt_percent * t.size, 0) / turbineWeightSum : 0;
        const stationRating = ((100 + boilerRating) / (100 + turbineRating) - 1) * 100;
        tutByStation[stId] = -B_consumption * stationRating / 100;
    });

    // Рубли = ТУТ × цена для каждой станции
    let total = 0;
    for (const [stId, tut] of Object.entries(tutByStation)) {
        const price = PRICE_BY_STATION[parseInt(stId)] || 0;
        total += tut * price;
    }
    return total;
}

const fmtInt = d3.format(",");
const fmtRu = n => fmtInt(Math.round(n)).replace(/,/g, " ");
function fmtMoneyRu(n){
  const abs = Math.abs(n);
  if (abs >= 1e9)  return `${(n/1e9).toFixed(1).replace('.', ',')} млрд`;
  if (abs >= 1e6)  return `${(n/1e6).toFixed(1).replace('.', ',')} млн`;
  if (abs >= 1e3)  return `${(n/1e3).toFixed(1).replace('.', ',')} тыс`;
  return fmtRu(n);
}

// ---- Цвет по проценту (красный → жёлтый → зелёный) ----
const _interpRY = d3.interpolateRgb("#ff2d2d", "#ffd400");
const _interpYG = d3.interpolateRgb("#ffd400", "#00d26a");
function colorForPercent(p){
  const t = Math.max(0, Math.min(1, p/100));
  return t < 0.5 ? _interpRY(t*2) : _interpYG((t-0.5)*2);
}

// ---- Средневзвешенный рейтинг Итого по станциям ----
// Для каждой станции считаем стационный рейтинг по формуле ((100+boiler)/(100-turbine)-1)*100,
// затем взвешиваем по суммарному Production котлов этой станции.
function calculateOverallRatingByStations(selectedStations) {
  // Собираем данные по станциям
  const stationMap = {};

  // Котлы
  hierarchicalData_boiler.children.forEach(b => {
    if (selectedStations !== null && !selectedStations.includes(b.station)) return;
    if (!stationMap[b.station]) stationMap[b.station] = { boilers: [], turbines: [] };
    stationMap[b.station].boilers.push(b);
  });

  // Турбины
  hierarchicalData_turbin.children.forEach(t => {
    if (selectedStations !== null && !selectedStations.includes(t.station)) return;
    if (!stationMap[t.station]) stationMap[t.station] = { boilers: [], turbines: [] };
    stationMap[t.station].turbines.push(t);
  });

  let sumWeightedRating = 0;
  let sumProduction = 0;

  Object.entries(stationMap).forEach(([stId, { boilers, turbines }]) => {
    const stationId = parseInt(stId);
    const isTec = tecStations.includes(stationId);
    const excludeTurbines = excludeTecTurbinesFlag && isTec;

    // boilerRating — средневзвешенный kpd_percent по Production
    const boilerWeightSum = boilers.reduce((acc, b) => acc + (b.size || 0), 0);
    const boilerRating = boilerWeightSum > 0
      ? boilers.reduce((acc, b) => acc + (b.kpd_percent || 0) * (b.size || 0), 0) / boilerWeightSum
      : 0;

    // turbineRating — средневзвешенный urt_percent по Consumption
    const activeTurbines = excludeTurbines ? [] : turbines;
    const turbineWeightSum = activeTurbines.reduce((acc, t) => acc + (t.size || 0), 0);
    const turbineRating = turbineWeightSum > 0
      ? activeTurbines.reduce((acc, t) => acc + (t.urt_percent || 0) * (t.size || 0), 0) / turbineWeightSum
      : 0;

    // Рейтинг станции для Итого-gauge: формула с (100 + turbineRating)
    // (отличается от per-station calculateRating, где знаменатель (100 - turbineRating))
    const stationRating = ((100 + boilerRating) / (100 + turbineRating) - 1) * 100;

    // Вес = суммарный Production котлов станции
    const stationProduction = boilerWeightSum;

    if (stationProduction > 0) {
      sumWeightedRating += stationRating * stationProduction;
      sumProduction += stationProduction;
    }
  });

  return sumProduction > 0 ? sumWeightedRating / sumProduction : 0;
}

// Итоговый рейтинг Итого (средневзвешенный по станциям)
let OVERALL_RATING = 0;

// Флаг видимости графика экономии (объявлен здесь, до updateAllGauges, чтобы избежать TDZ-ошибки)
let economyVisible = false;

// ---- Функция обновления всех гейджей на основе выбранных станций ----
function updateAllGauges() {
  const selectedStations = getSelectedStations();

  // Вычисляем рейтинги с учётом фильтра
  GAUGE_LEFT = calculateWeightedRatingBoilers(hierarchicalData_boiler.children, selectedStations);
  GAUGE_RIGHT = calculateWeightedRatingTurbines(hierarchicalData_turbin.children, selectedStations) * -1;
  OVERALL_RATING = calculateOverallRatingByStations(selectedStations);
  RESERVES_TUT = calculateReserves(selectedStations);
  RESERVES_RUB = calculateReservesRub(selectedStations);

  console.log('Обновление гейджей. Выбранные станции:', selectedStations);
  console.log('Котлы:', GAUGE_LEFT.toFixed(2) + '%', 'Турбины:', GAUGE_RIGHT.toFixed(2) + '%', 'Итого:', OVERALL_RATING.toFixed(2) + '%');

  // Перерисовываем спидометры
  drawGauge('gauge-1', GAUGE_LEFT);
  drawGauge('gauge-2', GAUGE_RIGHT, { displayMultiplier: -1 });

  // Обновляем карточку «Итоги»
  updateSummary();

  // Перерисовываем график экономии если открыт (безопасный вызов — до инициализации функция не определена)
  if (typeof economyVisible !== 'undefined' && economyVisible && typeof rerender === 'function') {
    rerender();
  }
}

// ---- Обновляем карточку «Итоги» ----
function updateSummary(){
  // overall для кольца: шкала 0–100, 50 = ноль, >50 = положительный рейтинг
  // Ограничиваем OVERALL_RATING диапазоном [-100, 100] для отображения
  const clampedRating = Math.max(-100, Math.min(100, OVERALL_RATING));
  const overall = 50 + clampedRating / 2;
  const ring = document.getElementById('summary-ring');
  const percentOut = document.getElementById('summary-percent-out');
  const rubEl = document.getElementById('kpi-rub');
  const tutEl = document.getElementById('kpi-tut');
  const rubLabelEl = document.getElementById('kpi-rub-label');
  const tutLabelEl = document.getElementById('kpi-tut-label');

  const totalValue = OVERALL_RATING;   // средневзвешенный по станциям
  const c = totalValue > 0 ? '+' : '';

  // Цвет: зелёный для положительных, красный для отрицательных
  const summaryColor = totalValue >= 0 ? '#00d26a' : '#EF4156';

  // Процент и кольцо
  ring.style.setProperty('--p', overall.toFixed(1));
  ring.style.setProperty('--clr', summaryColor);
  percentOut.textContent = `${c}${totalValue.toFixed(2)}%`;
  percentOut.style.color = summaryColor;

  // Определяем тип: Экономия (отрицательные значения = хорошо) или Перерасход (положительные = плохо)
  // Отрицательный резерв = экономия топлива (зелёный)
  // Положительный резерв = перерасход топлива (красный)
  const isEconomy = RESERVES_TUT < 0;
  const labelText = isEconomy ? 'Экономия' : 'Перерасход';
  const valueColor = isEconomy ? '#00d26a' : '#EF4156';

  // Обновляем метки
  rubLabelEl.textContent = `${labelText}, ₽`;
  tutLabelEl.textContent = `${labelText}, ТУТ`;

  // Обновляем значения с цветом (абсолютные значения, знак уже в названии)
  const absTut = Math.abs(RESERVES_TUT);
  const absRub = Math.abs(RESERVES_RUB);

  tutEl.textContent = fmtRu(absTut);
  tutEl.style.color = valueColor;

  rubEl.textContent = `₽ ${fmtMoneyRu(absRub)}`;
  rubEl.style.color = valueColor;
}

// Первоначальный расчёт и отрисовка
updateAllGauges();

// Перезагружаем страницу если браузер восстановил её из BFCACHE (back-forward cache),
// чтобы гарантировать пересчёт с актуальными настройками оборудования из localStorage.
window.addEventListener('pageshow', function(e) {
    if (e.persisted) { location.reload(); }
});

        // ===== НАСТРОЙКИ МАСШТАБА =====
  const UNIT = 'ТУТ';                 // единица измерения
  const SCALE_MODE = 'auto';          // 'auto' или 'fixed'
  const FIXED_MAX = 3000;             // если SCALE_MODE === 'fixed'

  // ===== КОНСТАНТНЫЕ ДАННЫЕ в ТУТ (пример) =====
  // Две серии (a и b) на каждый показатель.
  const LEFT_BARS2 = [
    { label: 'С уходящими газами, q₂', a: 1250, b: 980 },
    { label: ['От хим. и мех. неполноты',' сгорания топлива, q₃ q₄'], a: 840,  b: 920 },
    { label: 'От наружного охлаждения котла, q₅', a: 1630, b: 1510 },
    { label: 'С физ. теплом шлака, q₆', a: 690,  b: 730 },
    { label: 'Пуски, q', sub: 'пуск', a: 1420, b: 1330 },
  ];
  const RIGHT_BARS2 = [
    { label: 'Вакуум, W', a: 910,  b: 820 },
    { label: 'Температура пит. воды, T', sub: 'пв', a: 1360, b: 1480 },
    { label: 'Режим 1 корп', a: 1970, b: 1820 },
    { label: 'Прочее', a: 1210, b: 990  },
    { label: 'Фактор эл. нагрузки, N', sub: 'э', a: 780,  b: 860  },
  ];

  // формат числа "1 234"
const _fmtInt = d3.format(",");
const fmtTU = v => _fmtInt(Math.round(+v||0)).replace(/,/g," ");

// общий максимум (если нужно)
function computeSharedMax(datasets){
  let m = 0;
  datasets.forEach(rows => rows.forEach(d => { m = Math.max(m, +d.a||0, +d.b||0); }));
  return m * 1.05; // небольшой запас
}

// ДВОЙНЫЕ БАРЫ: линия слева, подпись перед линией, бары вправо (левый край прямой, правый скруглён)
// rows: [{label, a, b}], opts: {sharedMax, unit, barHeight, gap}
function drawDualBars(svgId, rows, opts = {}) {
  const UNIT = opts.unit || 'ТУТ';

  const svg = d3.select('#'+svgId);
  svg.selectAll('*').remove();

  const barH = opts.barHeight ?? 22;   // высота дорожки
  const gap  = opts.gap ?? 10;
  const minPadLbl = 90;                // минимум слева под подпись показателя
  const minPadVal = 50;                // минимум справа под вывод значений
  const padT = 8, padB = 12;
  const W = 320;                       // viewBox — SVG резиновый

  // 1) замеряем реальные ширины подписей и значений
  const meas = svg.append('g').attr('opacity', 0);
  const mLabel = meas.append('text').attr('class','bar-label');
  const mValue = meas.append('text').attr('class','bar-value');

  // Собираем все тексты меток (включая многострочные и с подстрочным индексом)
  const labelTexts = rows.flatMap(d => Array.isArray(d.label) ? d.label : [d.label + (d.sub || '')]);
  const valueTexts = rows.flatMap(d => [ `${fmtTU(d.a)} ${UNIT}`, `${fmtTU(d.b)}` ]);

  const wLabel = s => { mLabel.text(s); return mLabel.node().getComputedTextLength(); };
  const wValue = s => { mValue.text(s); return mValue.node().getComputedTextLength(); };

  let maxLabelW = 0; labelTexts.forEach(s => maxLabelW = Math.max(maxLabelW, wLabel(s)));
  let maxValueW = 0; valueTexts.forEach(s => maxValueW = Math.max(maxValueW, wValue(s)));
  meas.remove();

  // поля: слева под подпись, справа под числа
  let padLbl = Math.max(minPadLbl, Math.ceil(maxLabelW) + 12);
  let padVal = Math.max(minPadVal, Math.ceil(maxValueW) + 10);

  // гарантированный минимум ширины под бары
  const minBarW = 100;
  padLbl = Math.min(padLbl, W - padVal - minBarW);

  const innerW = Math.max(minBarW, W - padLbl - padVal - 1);
  const H = padT + rows.length*(barH+gap) - gap + padB;
  svg.attr('viewBox', `0 0 ${W} ${H}`);

  // ось в x = padLbl
  const axisX = padLbl;

  // общий масштаб по X
  const xMax = opts.sharedMax ?? computeSharedMax([rows]);
  const x = d3.scaleLinear().domain([0, xMax]).range([0, innerW]);

  // линия-ось
  svg.append('line')
    .attr('class','origin-line')
    .attr('x1', axisX).attr('y1', padT - 2)
    .attr('x2', axisX).attr('y2', padT + rows.length*(barH+gap) - gap + 2);

  const g = svg.append('g').attr('transform', `translate(${axisX + 1},${padT})`);

  // path: прямой левый край (у оси), правый скруглён
  function pillRight(yTop, height, width){
    const w = Math.max(0, +width||0);
    if (w <= 0) return '';
    const r = Math.min(height/2, w);
    const xL = 0, xR = w, xRm = w - r;
    const yT = yTop, yB = yTop + height;
    // M 0,yT H w-r Q w,yT w,yT+r V w,yB-r Q w,yB w-r,yB H 0 Z
    return `M ${xL},${yT} H ${xRm} Q ${xR},${yT} ${xR},${yT+r} V ${xR},${yB-r} Q ${xR},${yB} ${xRm},${yB} H ${xL} Z`;
  }

  rows.forEach((d,i)=>{
    const y = i*(barH+gap);
    const half = (barH/2) - 2;


    // серия 1 (верх)
    const aW = x(Math.max(0, +d.a||0));
    g.append('path').attr('d', pillRight(y + 1, half, aW)).attr('class','barA-fill');

    // серия 2 (низ)
    const bW = x(Math.max(0, +d.b||0));
    g.append('path').attr('d', pillRight(y + barH - 1 - half, half, bW)).attr('class','barB-fill');

    // значения — сразу ПОСЛЕ соответствующей полосы (справа от неё)
    g.append('text')
      .attr('class','bar-value')
      .attr('x', aW + 8)
      .attr('y', y + 1 + half/2 + .5)
      .attr('text-anchor','start')
      .text(`${fmtTU(d.a)}`);

    g.append('text')
      .attr('class','bar-value')
      .attr('x', bW + 8)
      .attr('y', y + barH - 1 - half + half/2 + .5)
      .attr('text-anchor','start')
      .text(`${fmtTU(d.b)}`);
  });

  // подписи показателей — ПЕРЕД линией, справа выравниваем к оси
  const labelsG = svg.append('g').attr('transform', `translate(${axisX - 8},${padT})`);
  rows.forEach((d, i) => {
    const yCenter = i * (barH + gap) + barH / 2;
    if (Array.isArray(d.label)) {
      // Многострочная метка
      const lines = d.label;
      const lineHeight = 10;
      const totalHeight = (lines.length - 1) * lineHeight;
      lines.forEach((line, j) => {
        labelsG.append('text')
          .attr('class', 'bar-label')
          .attr('x', 0)
          .attr('y', yCenter - totalHeight / 2 + j * lineHeight + 0.5)
          .attr('text-anchor', 'end')
          .text(line);
      });
    } else if (d.sub) {
      // Метка с подстрочным индексом (например, T с индексом "пв")
      const textEl = labelsG.append('text')
        .attr('class', 'bar-label')
        .attr('x', 0)
        .attr('y', yCenter + 0.5)
        .attr('text-anchor', 'end');
      textEl.append('tspan').text(d.label);
      textEl.append('tspan').attr('dy', '2.5').attr('font-size', '70%').text(d.sub);
    } else {
      // Однострочная метка
      labelsG.append('text')
        .attr('class', 'bar-label')
        .attr('x', 0)
        .attr('y', yCenter + 0.5)
        .attr('text-anchor', 'end')
        .text(d.label);
    }
  });
}

const SHARED_MAX = computeSharedMax([LEFT_BARS2, RIGHT_BARS2]);
drawDualBars('bars-left',  LEFT_BARS2,  { sharedMax: SHARED_MAX, unit: UNIT });
drawDualBars('bars-right', RIGHT_BARS2, { sharedMax: SHARED_MAX, unit: UNIT });


const MONTHS = ['Янв','Фев','Мар','Апр','Май','Июн','Июл','Авг','Сен','Окт','Ноя','Дек'];

// Помесячные рубли по каждой станции (стationId → [[turbines×12],[boilers×12]]).
// Используются в transformAll() для фильтрации по галочкам станций и ТЭЦ.
const MONTHLY_RUB_BY_STATION      = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.MonthlyRubByStation));
const MONTHLY_RUB_BY_STATION_PREV = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.MonthlyRubByStationPrev));


function drawMultiLineEnhanced(svgId, seriesDefs, areaDefs, unitInfo){
  const svg = d3.select('#'+svgId);
  svg.selectAll('*').remove();

  const W = 1000, H = 270;
  const m = {top: 24, right: 24, bottom: 36, left: 70};
  const innerW = W - m.left - m.right;
  const innerH = H - m.top - m.bottom;

  const x = d3.scalePoint().domain(MONTHS).range([0, innerW]).padding(0.5);

  const allVals = [
    ...seriesDefs.flatMap(s => s.values),
    ...areaDefs.flatMap(a => a.values)
  ];
  // Учитываем и минимум, и максимум для поддержки отрицательных значений
  const yMin = d3.min(allVals) || 0;
  const yMax = d3.max(allVals) || 0;
  // Добавляем отступ 10% с обеих сторон
  const yPadding = Math.max(Math.abs(yMax), Math.abs(yMin)) * 0.1 || 1;
  const y = d3.scaleLinear()
    .domain([Math.min(yMin - yPadding, 0), Math.max(yMax + yPadding, 0)])
    .nice()
    .range([innerH, 0]);

  const g = svg.append('g').attr('transform', `translate(${m.left},${m.top})`);

  // сетка и оси
  g.append('g').attr('class','grid')
    .call(d3.axisLeft(y).ticks(6).tickSize(-innerW).tickFormat(''));
  g.append('g').attr('class','axis')
    .call(d3.axisLeft(y).ticks(6).tickFormat(fmtMoneyShort));

  // Горизонтальная ось X на уровне y=0 (линия нуля)
  const y0 = y(0);
  g.append('g').attr('class','axis axis-x-zero')
    .attr('transform', `translate(0,${y0})`)
    .call(d3.axisBottom(x).tickSize(0));

  // Линия нуля (выделенная)
  g.append('line')
    .attr('x1', 0).attr('x2', innerW)
    .attr('y1', y0).attr('y2', y0)
    .attr('stroke', '#666').attr('stroke-width', 1.5)
    .attr('stroke-dasharray', '4 2');

  // необязательная подпись оси Y (единицы)
  g.append('text')
    .attr('x', -m.left + 4).attr('y', -8)
    .attr('fill', '#bdbdbd').attr('font-family','sans-serif').attr('font-size','12px')
    .text(unitInfo.unitText);

  // Функция проверки наличия данных (0 = нет данных)
  const hasData = v => v !== 0;

  // Линия с разрывами где нет данных
  const line = d3.line()
    .defined(hasData)
    .x((_,i)=> x(MONTHS[i]))
    .y(d => y(d))
    .curve(d3.curveMonotoneX);

  // Область заливки от линии нуля до значения (работает и для отрицательных)
  const area = d3.area()
    .x((_,i)=> x(MONTHS[i]))
    .y0(y(0))
    .y1(d => y(d))
    .curve(d3.curveMonotoneX);

  // СНАЧАЛА — заливки прошлого года
  areaDefs.forEach(a => {
    g.append('path').datum(a.values).attr('class', a.areaClass).attr('d', area);
  });

  // ПОТОМ — линии текущего года (с разрывами)
  seriesDefs.forEach(s => {
    g.append('path').datum(s.values).attr('class', s.lineClass).attr('d', line);
  });

  // точки текущего года (только где есть данные)
  seriesDefs.forEach(s => {
    g.selectAll('.'+s.dotClass)
      .data(s.values.map((v,i)=> ({v,i})).filter(d => hasData(d.v)))
      .enter().append('circle')
        .attr('class', `dot ${s.dotClass}`)
        .attr('r', 4.5)
        .attr('cx', d=> x(MONTHS[d.i]))
        .attr('cy', d=> y(d.v));
  });

  // фокус
  const focusLine = g.append('line')
    .attr('y1', 0).attr('y2', innerH)
    .attr('stroke', '#4a4a4a').attr('stroke-dasharray', '4 4')
    .style('opacity', 0);

  const focusDots = seriesDefs.map(s =>
    g.append('circle')
      .attr('r', 6).attr('stroke','#fff').attr('stroke-width',2)
      .attr('fill', s.focusFill)
      .style('opacity', 0)
  );

  // Tooltip вешаем на document.documentElement (<html>), а не на body.
  // Если на body есть transform: scale(...), то position:fixed на его потомке
  // позиционируется относительно body-системы координат, а не viewport —
  // из-за этого чем правее курсор, тем сильнее дрейфует tooltip.
  // У <html> трансформов нет, поэтому position:fixed здесь = viewport-координаты.
  // Переиспользуем если уже создан (при повторном rerender не дублируется).
  let _tipEl = document.getElementById('economy-chart-tip');
  if (!_tipEl) {
    _tipEl = document.createElement('div');
    _tipEl.id = 'economy-chart-tip';
    _tipEl.style.cssText = [
      'position:fixed',
      'background:rgba(0,0,0,0.88)',
      'color:#fff',
      'padding:8px 12px',
      'border-radius:6px',
      'z-index:2147483647',
      'pointer-events:none',
      'white-space:nowrap',
      'font:12px/1.5 sans-serif',
      'opacity:0',
      'transition:opacity 0.06s'
    ].join(';');
    document.documentElement.appendChild(_tipEl);
  }

  // оверлей
  g.append('rect')
    .attr('x', 0).attr('y', 0).attr('width', innerW).attr('height', innerH)
    .attr('fill', 'transparent')
    .on('mousemove', (event)=>{
      const [mx] = d3.pointer(event);
      // ближайший месяц
      let idx = 0, best = Infinity;
      for(let i=0;i<MONTHS.length;i++){
        const xi = x(MONTHS[i]);
        const d = Math.abs(mx - xi);
        if (d < best){ best = d; idx = i; }
      }
      const xv = x(MONTHS[idx]);
      focusLine.attr('x1',xv).attr('x2',xv).style('opacity',1);

      focusDots.forEach((dot, k)=>{
        const v = seriesDefs[k].values[idx];
        if (v !== 0) {
          dot.attr('cx', xv).attr('cy', y(v)).style('opacity',1);
        } else {
          dot.style('opacity',0);
        }
      });

      const val = unitInfo.formatVal;
      const fmtVal = (v) => v === 0 ? '<span style="color:#666">нет данных</span>' : val(v);

      // Сначала заполняем контент (чтобы offsetWidth был актуальным)
      _tipEl.innerHTML = `
        <div style="color:#bdbdbd;margin-bottom:5px;font-size:13px;"><b>${MONTHS[idx]}</b></div>
        <div style="margin-bottom:2px"><span style="display:inline-block;width:10px;height:10px;background:#74c0ff;border-radius:50%;margin-right:6px;vertical-align:middle"></span>Котлы: ${fmtVal(seriesDefs[0].values[idx])} <span style="color:#b8dbff">(пред.: ${fmtVal(areaDefs[0].values[idx])})</span></div>
        <div style="margin-bottom:2px"><span style="display:inline-block;width:10px;height:10px;background:#ffd400;border-radius:50%;margin-right:6px;vertical-align:middle"></span>Турбины: ${fmtVal(seriesDefs[1].values[idx])} <span style="color:#ffe47a">(пред.: ${fmtVal(areaDefs[1].values[idx])})</span></div>
        <div><span style="display:inline-block;width:10px;height:10px;background:#EF4156;border-radius:50%;margin-right:6px;vertical-align:middle"></span><b>Итоги: ${fmtVal(seriesDefs[2].values[idx])}</b> <span style="color:#EE5A6B">(пред.: ${fmtVal(areaDefs[2].values[idx])})</span></div>
      `;

      // Позиционируем с компенсацией CSS-трансформа родителя.
      // Если на html/body стоит transform:scale(...), position:fixed дочернего элемента
      // позиционируется в локальных координатах трансформированного предка, а не в viewport.
      // getBoundingClientRect() возвращает реальные viewport-координаты предка с учётом transform,
      // что позволяет точно конвертировать viewport-coords → local-coords родителя.
      const par  = _tipEl.parentElement;
      const parR = par.getBoundingClientRect();
      const sx   = par.offsetWidth  > 0 ? parR.width  / par.offsetWidth  : 1;
      const sy   = par.offsetHeight > 0 ? parR.height / par.offsetHeight : 1;

      const tw = _tipEl.offsetWidth  || 220;
      const th = _tipEl.offsetHeight || 80;
      const vw = window.innerWidth;
      const vh = window.innerHeight;
      const gap = 14;

      // Желаемая позиция в координатах viewport
      let vpLeft = event.clientX + gap;
      let vpTop  = event.clientY - 10;

      // Boundary clamping в viewport-пространстве (учитываем реальный px-размер тултипа)
      if (vpLeft + tw * sx > vw - 6) vpLeft = event.clientX - tw * sx - gap;
      if (vpTop  + th * sy > vh - 6) vpTop  = vh - th * sy - 6;
      if (vpTop  < 6)                vpTop  = 6;
      if (vpLeft < 6)                vpLeft = 6;

      // Конвертируем viewport-coords → локальные coords родителя
      _tipEl.style.left    = ((vpLeft - parR.left) / sx) + 'px';
      _tipEl.style.top     = ((vpTop  - parR.top)  / sy) + 'px';
      _tipEl.style.opacity = '1';
    })
    .on('mouseleave', ()=>{
      focusLine.style('opacity',0);
      focusDots.forEach(dot => dot.style('opacity',0));
      _tipEl.style.opacity = '0';
    });
}
/* ===== Настройки переключателей ===== */
// Коэффициент перевода рубли→ТУТ для графиков (обратный: данные уже в рублях)
// Если есть резервы в ТУТ и рублях, вычисляем средний курс; иначе дефолт 5000
const RUB_PER_TUT = (RESERVES_TUT !== 0 && RESERVES_RUB !== 0) ? Math.abs(RESERVES_RUB / RESERVES_TUT) : 5000;
let isRub = true;         // true = ₽ (по умолчанию, т.к. данные в рублях), false = ТУТ
let isCum = false;        // false = помесячно, true = накопительно

// Форматтеры
const _fmtN = d3.format(',');
const fmtIntSp = n => _fmtN(Math.round(n)).replace(/,/g,' ');
function fmtMoneyShort(n){
  const abs = Math.abs(n);
  const sign = n < 0 ? '−' : '';
  if (abs >= 1e9)  return `${sign}${(abs/1e9).toFixed(1).replace('.', ',')} млрд`;
  if (abs >= 1e6)  return `${sign}${(abs/1e6).toFixed(1).replace('.', ',')} млн`;
  if (abs >= 1e3)  return `${sign}${(abs/1e3).toFixed(1).replace('.', ',')} тыс`;
  return `${sign}${fmtIntSp(abs)}`;
}

// Преобразования
const toCum = arr => arr.reduce((acc,v,i)=> (acc[i]=(i?acc[i-1]:0)+v, acc), []);
// Данные уже в рублях, для ТУТ делим на курс
const toTut = arr => arr.map(v => RUB_PER_TUT > 0 ? v / RUB_PER_TUT : 0);

// Собирает серии по станционным данным с учётом фильтра станций, флага ТЭЦ
// и пропорциональным учётом выбранного оборудования (stationTurbFrac/stationBoilFrac).
// stationData: { stationId: [[turbines×12],[boilers×12]] }
// selectedStations: null (все) или массив ID выбранных станций
// excludeTec: true/false — исключать ли турбины ТЭЦ-станций
function buildSeriesFromStations(stationData, selectedStations, excludeTec) {
  const b = new Array(12).fill(0);
  const t = new Array(12).fill(0);
  const s = new Array(12).fill(0);
  for (const [stIdStr, arr] of Object.entries(stationData)) {
    const stId = parseInt(stIdStr);
    if (selectedStations !== null && !selectedStations.includes(stId)) continue;
    const turbines = arr[0];  // arr[0] = turbines[12]
    const boilers  = arr[1];  // arr[1] = boilers[12]
    const isTec = tecStations.includes(stId);
    // Пропорциональный учёт включённого оборудования внутри станции
    const turbFrac = stationTurbFrac[stId] !== undefined ? stationTurbFrac[stId] : 1.0;
    const boilFrac = stationBoilFrac[stId] !== undefined ? stationBoilFrac[stId] : 1.0;
    for (let i = 0; i < 12; i++) {
      const bVal = (boilers[i] || 0) * boilFrac;
      const tVal = (excludeTec && isTec) ? 0 : (turbines[i] || 0) * turbFrac;
      b[i] += bVal;
      t[i] += tVal;
      s[i] += bVal + tVal;
    }
  }
  return { b, t, s };
}

// Применить текущие тумблеры к данным (возвращает новые массивы).
// Перерасход (положительные значения) рисуется выше 0, экономия — ниже 0.
function transformAll(){
  const selectedStations = getSelectedStations();

  const curr = buildSeriesFromStations(MONTHLY_RUB_BY_STATION,      selectedStations, excludeTecFromUrl);
  const prev = buildSeriesFromStations(MONTHLY_RUB_BY_STATION_PREV, selectedStations, excludeTecFromUrl);

  let b = curr.b, t = curr.t, s = curr.s;
  let bp = prev.b, tp = prev.t, sp = prev.s;

  // Если режим ТУТ — конвертируем из рублей в ТУТ
  if (!isRub){
    b = toTut(b); t = toTut(t); s = toTut(s);
    bp = toTut(bp); tp = toTut(tp); sp = toTut(sp);
  }

  if (isCum){
    b = toCum(b); t = toCum(t); s = toCum(s);
    bp = toCum(bp); tp = toCum(tp); sp = toCum(sp);
  }
  return { b,t,s,bp,tp,sp };
}

// Перерисовка с учётом тумблеров
function rerender(){
  const { b,t,s,bp,tp,sp } = transformAll();

  // Обновим подпись единиц у оси/подсказок
  const unitInfo = {
    unitText: isRub ? '₽' : 'ТУТ',
    formatVal(v){
      return isRub ? `₽ ${fmtMoneyShort(v)}` : `${fmtIntSp(v)} ТУТ`;
    }
  };

  // Рисуем (замени ниже вызов на твой drawMultiLine с поддержкой unitText/formatVal в тултипе)
  drawMultiLineEnhanced('line-multi',
    [
      { title:'Котлы',   values: b, lineClass:'line-boilers',  dotClass:'dot-boilers',  focusFill:'#74c0ff' },
      { title:'Турбины', values: t, lineClass:'line-turbines', dotClass:'dot-turbines', focusFill:'#ffd400' },
      { title:'Итоги',   values: s, lineClass:'line-total',    dotClass:'dot-total',    focusFill:'#EF4156' },
    ],
    [
      { title:'Котлы (прошл.)',   values: bp, areaClass:'area-boilers-prev'  },
      { title:'Турбины (прошл.)', values: tp, areaClass:'area-turbines-prev' },
      { title:'Итоги (прошл.)',   values: sp, areaClass:'area-total-prev'    },
    ],
    unitInfo
  );

  // Обновить подпись возле тумблера валюты
  document.getElementById('label-currency').textContent = isRub ? '₽' : 'ТУТ';
}

// Слушатели тумблеров
document.getElementById('toggle-currency').addEventListener('change', e=>{
  isRub = e.target.checked;
  rerender();
});
document.getElementById('toggle-cum').addEventListener('change', e=>{
  isCum = e.target.checked;
  rerender();
});

// Клик на блок "Итого" для переключения графика Экономия
// (economyVisible объявлена выше, рядом с OVERALL_RATING, чтобы избежать TDZ в updateAllGauges)
document.getElementById('summary-card-click').addEventListener('click', function() {
  const barsContent = document.getElementById('bars-content');
  const chartContainer = document.getElementById('economy-chart-container');

  economyVisible = !economyVisible;

  if (economyVisible) {
    // Показать график, скрыть бары
    barsContent.style.display = 'none';
    chartContainer.style.display = 'flex';
    rerender();
  } else {
    // Показать бары, скрыть график
    chartContainer.style.display = 'none';
    barsContent.style.display = 'flex';
  }
});

</script>
</div>
</body>
