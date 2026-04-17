SELECT 
    r.stationid,
    r.date,
    r.tut_total,
    r.reserves_total,
    CASE WHEN r.tut_total <> 0 THEN ROUND((r.reserves_total / r.tut_total)::numeric, 0) ELSE NULL END AS implied_rub_per_tut,
    p.price_per_tut AS actual_price
FROM reserves_rub r
LEFT JOIN raw_fuel_prices_monthly p 
    ON p.stationid = r.stationid 
    AND p.month_date = r.date
WHERE r.period_type = 1
  AND r.tut_total <> 0
ORDER BY r.date DESC, r.stationid
LIMIT 30;
