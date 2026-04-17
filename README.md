SELECT 
    m.stationid,
    ROUND(m.sum_tut_monthly::numeric, 2)   AS tut_monthly_sum,
    ROUND(y.tut_total::numeric, 2)          AS tut_yearly,
    ROUND(m.sum_rub_monthly::numeric, 2)   AS rub_monthly_sum,
    ROUND(y.reserves_total::numeric, 2)     AS rub_yearly,
    ROUND((m.sum_rub_monthly - y.reserves_total)::numeric, 2) AS rub_diff
FROM (
    SELECT stationid,
           SUM(tut_total)      AS sum_tut_monthly,
           SUM(reserves_total) AS sum_rub_monthly
    FROM reserves_rub
    WHERE period_type = 1 AND EXTRACT(YEAR FROM date) = 2026
    GROUP BY stationid
) m
LEFT JOIN (
    SELECT stationid, tut_total, reserves_total
    FROM reserves_rub
    WHERE period_type = 2 AND EXTRACT(YEAR FROM date) = 2026
) y ON m.stationid = y.stationid
ORDER BY stationid;
