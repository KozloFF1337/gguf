        private void CalculateYearlyData(NpgsqlConnection pgConn)
        {
            // Расчет годовых данных турбин из raw_turbins_monthly
            // (включая все месяцы, даже если год ещё не завершён)
            string calculateYearlyTurbinsQuery = @"
                INSERT INTO ""Turbins""(""TurbinID"", ""StationID"", ""URT"", ""Consumption"", ""PeriodType"", ""PeriodValue"", ""NominalURT"", ""Date"")
                SELECT
                    turbinid,
                    stationid,
                    SUM((urt + variation) * consumption) / NULLIF(SUM(consumption), 0) as urt,
                    SUM(consumption) as consumption,
                    2 as periodtype,
                    COUNT(*) as periodvalue,
                    SUM(nominal_urt * consumption) / NULLIF(SUM(consumption), 0) as nominal_urt,
                    DATE_TRUNC('year', month_date)::date as year_date
                FROM raw_turbins_monthly
                WHERE hours > 0 AND consumption > 0
                GROUP BY turbinid, stationid, DATE_TRUNC('year', month_date)
                ON CONFLICT (""TurbinID"", ""StationID"", ""PeriodType"", ""Date"")
                WHERE ""Date"" IS NOT NULL
                DO UPDATE SET
                    ""URT"" = EXCLUDED.""URT"",
                    ""Consumption"" = EXCLUDED.""Consumption"",
                    ""PeriodValue"" = EXCLUDED.""PeriodValue"",
                    ""NominalURT"" = EXCLUDED.""NominalURT"";";

            using (var cmd = new NpgsqlCommand(calculateYearlyTurbinsQuery, pgConn))
            {
                cmd.ExecuteNonQuery();
            }

            // Расчет годовых данных котлов из raw_boilers_monthly
            // (включая все месяцы, даже если год ещё не завершён)
            string calculateYearlyBoilersQuery = @"
                INSERT INTO ""Boilers""(""BoilerID"", ""StationID"", ""KPD"", ""Production"", ""Consumption"", ""PeriodType"", ""PeriodValue"", ""Date"")
                SELECT
                    boilerid,
                    stationid,
                    SUM((kpd - humidity - ash + (temp_fact - temp_nominal) * temp_koef) * production) / NULLIF(SUM(production), 0) as kpd,
                    SUM(production) as production,
                    SUM(consumption) as consumption,
                    2 as periodtype,
                    COUNT(*) as periodvalue,
                    DATE_TRUNC('year', month_date)::date as year_date
                FROM raw_boilers_monthly
                WHERE hours > 0 AND consumption > 0
                GROUP BY boilerid, stationid, DATE_TRUNC('year', month_date)
                ON CONFLICT (""BoilerID"", ""StationID"", ""PeriodType"", ""Date"")
                WHERE ""Date"" IS NOT NULL
                DO UPDATE SET
                    ""KPD"" = EXCLUDED.""KPD"",
                    ""Production"" = EXCLUDED.""Production"",
                    ""Consumption"" = EXCLUDED.""Consumption"",
                    ""PeriodValue"" = EXCLUDED.""PeriodValue"";";

            using (var cmd = new NpgsqlCommand(calculateYearlyBoilersQuery, pgConn))
            {
                cmd.ExecuteNonQuery();
            }
        }
