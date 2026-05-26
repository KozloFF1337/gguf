1. raw_boilers.csv
SELECT *
FROM raw_boilers
WHERE stationid IN (1, 2, 22)
  AND date BETWEEN '2026-03-01' AND '2026-03-07'
ORDER BY stationid, boilerid, date;
2. raw_boilers_monthly.csv
SELECT *
FROM raw_boilers_monthly
WHERE stationid IN (1, 2, 22)
  AND month_date BETWEEN '2025-01-01' AND '2026-12-01'
ORDER BY stationid, boilerid, month_date;
3. raw_boilers_yearly.csv
SELECT *
FROM raw_boilers_yearly
WHERE stationid IN (1, 2, 22)
ORDER BY stationid, boilerid, year_date;
4. raw_turbins.csv
SELECT *
FROM raw_turbins
WHERE stationid IN (1, 2, 22)
  AND date BETWEEN '2026-03-01' AND '2026-03-07'
ORDER BY stationid, turbinid, date;
5. raw_turbins_monthly.csv
SELECT *
FROM raw_turbins_monthly
WHERE stationid IN (1, 2, 22)
  AND month_date BETWEEN '2025-01-01' AND '2026-12-01'
ORDER BY stationid, turbinid, month_date;
6. raw_turbins_yearly.csv
SELECT *
FROM raw_turbins_yearly
WHERE stationid IN (1, 2, 22)
ORDER BY stationid, turbinid, year_date;
7. Boilers.csv — EF-таблица с двойными кавычками
SELECT *
FROM "Boilers"
WHERE "StationID" IN (1, 2, 22)
ORDER BY "StationID", "BoilerID", "Date";
8. Turbins.csv
SELECT *
FROM "Turbins"
WHERE "StationID" IN (1, 2, 22)
ORDER BY "StationID", "TurbinID", "Date";
9. reserves_rub.csv — самая важная
SELECT *
FROM reserves_rub
WHERE stationid IN (1, 2, 22)
  AND date BETWEEN '2025-01-01' AND '2026-12-31'
ORDER BY stationid, date, period_type;
10. raw_fuel_prices_monthly.csv
SELECT *
FROM raw_fuel_prices_monthly
WHERE stationid IN (1, 2, 22)
ORDER BY stationid, month_date;
11. (опционально) raw_fuel_prices.csv и raw_fuel_prices_yearly.csv
Если эти таблицы у вас заполнены — пришлите тоже:

SELECT * FROM raw_fuel_prices         WHERE stationid IN (1, 2, 22) ORDER BY stationid, date;
SELECT * FROM raw_fuel_prices_yearly  WHERE stationid IN (1, 2, 22) ORDER BY stationid, year_date;
