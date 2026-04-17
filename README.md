SELECT
    SUM(tut_boilers + CASE WHEN stationid IN (2,3,4,5,6,7,8,10,12,13,14,17,18,19,20,21,22) THEN 0 ELSE tut_turbines END) AS tut_total,
    SUM(reserves_boilers + CASE WHEN stationid IN (2,3,4,5,6,7,8,10,12,13,14,17,18,19,20,21,22) THEN 0 ELSE reserves_turbines END) AS rub_total
FROM reserves_rub
WHERE period_type = 2
  AND EXTRACT(YEAR FROM date) = 2026;
