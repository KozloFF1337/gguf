Ок, правим точечно твой текущий файл — чтобы

ряд со спидометрами и «Итогами» растягивался на всю ширину, как гистограммы;

число «Итоги» было справа от круга и крупным.

Ниже — что именно заменить.

1) CSS — растянуть ряд спидометров «в край» и вынести число итогов

ЗАМЕНИ блок .gauges-container и карточки на это:

.gauges-container{
  flex: 1 1 auto;
  display: flex;
  gap: 16px;
  align-items: flex-start;
  justify-content: stretch;   /* карточки растягиваются */
  padding: 0;                 /* без внутренних отступов */
  box-sizing: border-box;
  flex-wrap: nowrap;
  overflow-x: auto;
  min-width: 0;
}
.gauge-card,
.summary-card{
  flex: 1 1 0;                /* равные доли ширины */
  min-width: 0;               /* разрешаем сжиматься */
  max-width: none;            /* убираем верхний предел */
  background:#222;
  border-radius:12px;
  padding:0;                  /* убираем внутренние отступы */
  box-shadow:0 2px 10px rgba(0,0,0,.35);
  display:flex; flex-direction:column; align-items:center;
}
.gauge-title{
  margin:6px 0;
  font-size:24px;
  color:#ddd;
  text-align:center;
  letter-spacing:.3px;
}

/* Итоги: число справа от кольца */
.summary-ring .value{ display:none; } /* на случай, если останется старый div */
.summary-inline{ display:flex; align-items:center; gap:12px; justify-content:center; margin-bottom:10px; }
.summary-big{ color:#fff; font:700 28px/1 sans-serif; }


Остальные твои стили оставь как есть.

2) HTML — убрать ограничение ширины и вынести число итогов вправо

А) В верхнем контейнере замени ширину 88% на 100%:

<!-- БЫЛО -->
<div style="width: 88%;">
<!-- СТАЛО -->
<div style="width: 100%;">


Б) В блоке «Итоги» замени кусок с кольцом:

<!-- БЫЛО -->
<div class="summary-ring" id="summary-ring">
  <div class="value" id="summary-percent">0%</div>
</div>

<!-- СТАЛО -->
<div class="summary-inline">
  <div class="summary-ring" id="summary-ring"></div>
  <div class="summary-big" id="summary-percent-out">0%</div>
</div>


В) У обоих спидометров поправь viewBox, чтобы не было внутренних полей:

<!-- БЫЛО -->
<svg id="gauge-1" class="gauge-svg" viewBox="0 20 320 160" preserveAspectRatio="xMidYMid meet"></svg>
<svg id="gauge-2" class="gauge-svg" viewBox="0 20 320 160" preserveAspectRatio="xMidYMid meet"></svg>

<!-- СТАЛО -->
<svg id="gauge-1" class="gauge-svg" viewBox="0 0 320 180" preserveAspectRatio="xMidYMid meet"></svg>
<svg id="gauge-2" class="gauge-svg" viewBox="0 0 320 180" preserveAspectRatio="xMidYMid meet"></svg>

3) JS — «распереть» дугу на всю ширину и вывести число итогов справа

А) Полностью замени функцию drawGauge(...) на эту:

function drawGauge(svgId, value){
  const svg = d3.select('#'+svgId);
  svg.selectAll('*').remove();

  const W = 320, H = 180;
  const cx = W/2;
  const rOuter = (W/2) - 1;    // дуга почти до краёв по горизонтали
  const thickness = 28;
  const rInner = rOuter - thickness;
  const cy = rOuter + 10;      // лёгкий верхний зазор

  // Градиент (красный -> жёлтый -> зелёный)
  const defs = svg.append('defs');
  const gradId = `grad-${svgId}`;
  const grad = defs.append('linearGradient')
    .attr('id', gradId)
    .attr('gradientUnits', 'userSpaceOnUse')
    .attr('x1', 0).attr('y1', cy)
    .attr('x2', W).attr('y2', cy);
  grad.append('stop').attr('offset','0%').attr('stop-color','#ff2d2d');
  grad.append('stop').attr('offset','50%').attr('stop-color','#ffd400');
  grad.append('stop').attr('offset','100%').attr('stop-color','#00d26a');

  // Полукруг от -π до 0 (слева->вправо)
  const start = -Math.PI, end = 0;
  const scale = d3.scaleLinear().domain([0,100]).range([start, end]);

  // Фон дуги
  svg.append('g').attr('transform', `translate(${cx},${cy})`)
    .append('path')
    .attr('d', d3.arc().innerRadius(rInner).outerRadius(rOuter).startAngle(start).endAngle(end))
    .attr('fill', '#3a3a3a');

  // Значение
  const valAngle = scale(Math.max(0, Math.min(100, value)));
  svg.append('g').attr('transform', `translate(${cx},${cy})`)
    .append('path')
    .attr('d', d3.arc().innerRadius(rInner).outerRadius(rOuter).startAngle(start).endAngle(valAngle))
    .attr('fill', `url(#${gradId})`);

  // Число в центре можно оставить/убрать; сейчас оставлено:
  svg.append('text')
    .attr('x', cx).attr('y', cy - 10)
    .attr('text-anchor','middle').attr('fill','#fff')
    .attr('font-family','sans-serif').attr('font-size','28px').attr('font-weight','700')
    .text(`${Math.round(value)}%`);
}


Б) В updateSummary() выносим число итогов вправо — замени использование старого id:

// БЫЛО:
const percentEl = document.getElementById('summary-percent');
// ...
percentEl.textContent = `${Math.round(overall)}%`;

// СТАЛО:
const percentOut = document.getElementById('summary-percent-out');
// ...
percentOut.textContent = `${Math.round(overall)}%`;


Важное замечание: у тебя ниже по файлу есть две пары вызовов drawGauge:

drawGauge('gauge-1', 70);
drawGauge('gauge-2', 85);

// потом ещё раз:
drawGauge('gauge-1', GAUGE_LEFT);
drawGauge('gauge-2', GAUGE_RIGHT);


Оставь одну пару (вторую, с константами GAUGE_LEFT/RIGHT) и удали первые два вызова, чтобы не перерисовывать лишний раз.

Готово: спидометры + «Итоги» растянутся на всю ширину (как гистограммы), а общий процент будет крупно справа от кольца и не будет перекрываться.
