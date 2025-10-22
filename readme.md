cell.filter(d => (d.x1 - d.x0) >= 30 && (d.y1 - d.y0) >= 30) // Только если ширина достаточна
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
