using System;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Npgsql;


using (NpgsqlConnection connection1 = new NpgsqlConnection(SQL_config.connectionStringPGSQL))
{
    try
    {
        DateTime dt = DateTime.Now.AddDays(-9);
        string select_KA = $@"SELECT *
                FROM raw_boilers
                WHERE (boilerid, stationid) IN (
                    SELECT boilerid, stationid
                        FROM raw_boilers
                        WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND hours = 24 AND production > 0 AND consumption > 0 AND kpd > 0
                        GROUP BY boilerid, stationid
                        HAVING COUNT(*) > 3
                        AND NOT EXISTS (
                            SELECT 1
                            FROM raw_boilers AS t2
                            WHERE t2.boilerid = raw_boilers.boilerid
                            AND t2.stationid = raw_boilers.stationid
                            AND t2.hours <> 24
                            AND t2.date = (SELECT MAX(date)
                                            FROM raw_boilers AS t3
                                            WHERE t3.boilerid = t2.boilerid
                                            AND t3.stationid = t2.stationid)
                            )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND hours = 24 AND production > 0 AND consumption > 0 AND kpd > 0
                            ORDER BY stationid, boilerid, date;";
        string select_TA = $@"SELECT *
                FROM raw_turbins
                WHERE (turbinid, stationid) IN (
                    SELECT turbinid, stationid
                        FROM raw_turbins
                        WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND hours = 24 AND consumption > 0 AND urt > 0
                        GROUP BY turbinid, stationid
                        HAVING COUNT(*) > 3
                        AND NOT EXISTS (
                            SELECT 1
                            FROM raw_turbins AS t2
                            WHERE t2.turbinid = raw_turbins.turbinid
                            AND t2.stationid = raw_turbins.stationid
                            AND t2.hours <> 24
                            AND t2.date = (SELECT MAX(date)
                                            FROM raw_turbins AS t3
                                            WHERE t3.turbinid = t2.turbinid
                                            AND t3.stationid = t2.stationid)
                            )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND hours = 24 AND consumption > 0 AND urt > 0
                                        ORDER BY stationid, turbinid, date;";
        using (SqlConnection connection = new SqlConnection(SQL_config.connectionString))
        {
            connection.Open();
            connection1.Open();
            DBFunctions.GenerateDB_KA(Akscodes.rec_KA, Akscodes.matching_dict_KA, SQL_config.insertQuerry_KA, SQL_config.format, connection1, connection);
            Console.WriteLine("Сделано -1");
            DBFunctions.GenerateDB_TA(Akscodes.rec_TA, Akscodes.matching_dict_TA, SQL_config.insertQuerry_TA, SQL_config.format, connection1, connection);
            Console.WriteLine("Сделано 0");
            DBFunctions.GetRelevantWeekData_TA(Akscodes.rec_WT, Akscodes.weekTurbins, select_TA, connection1, connection);
            Console.WriteLine("Сделано 1.1");
            DBFunctions.GetRelevantWeekData_KA(Akscodes.rec_WB, Akscodes.weekBoilers, select_KA, connection1, connection);
            Console.WriteLine("Сделано 2.1");
            dt = DateTime.Now.AddDays(-32);
            select_KA = $@"SELECT *
                FROM raw_boilers
                WHERE (boilerid, stationid) IN (
                    SELECT boilerid, stationid
                        FROM raw_boilers
                        WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND production > 0 AND consumption > 0 AND kpd > 0
                        GROUP BY boilerid, stationid
                        HAVING COUNT(*) > 3
                        AND NOT EXISTS (
                            SELECT 1
                            FROM raw_boilers AS t2
                            WHERE t2.boilerid = raw_boilers.boilerid
                            AND t2.stationid = raw_boilers.stationid
                            AND t2.hours > 24
                            AND t2.date = (SELECT MAX(date)
                                            FROM raw_boilers AS t3
                                            WHERE t3.boilerid = t2.boilerid
                                            AND t3.stationid = t2.stationid)
                            )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND production > 0 AND consumption > 0 AND kpd > 0
                            ORDER BY stationid, boilerid, date;";
            select_TA = $@"SELECT *
                    FROM raw_turbins
                    WHERE (turbinid, stationid) IN (
                        SELECT turbinid, stationid
                            FROM raw_turbins
                            WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND consumption > 0 AND urt > 0
                            GROUP BY turbinid, stationid
                            HAVING COUNT(*) > 3
                            AND NOT EXISTS (
                                SELECT 1
                                FROM raw_turbins AS t2
                                WHERE t2.turbinid = raw_turbins.turbinid
                                AND t2.stationid = raw_turbins.stationid
                                AND t2.hours > 24
                                AND t2.date = (SELECT MAX(date)
                                                FROM raw_turbins AS t3
                                                WHERE t3.turbinid = t2.turbinid
                                                AND t3.stationid = t2.stationid)
                                )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND consumption > 0 AND urt > 0
                                            ORDER BY stationid, turbinid, date;";
            DBFunctions.GetRelevantMonthData_TA(Akscodes.rec_WT, Akscodes.weekTurbins, select_TA, connection1, connection);
            Console.WriteLine("Сделано 1.2");
            DBFunctions.GetRelevantMonthData_KA(Akscodes.rec_WB, Akscodes.weekBoilers, select_KA, connection1, connection);
            Console.WriteLine("Сделано 2.2");
            dt = DateTime.Now.AddDays(-367);
            select_KA = $@"SELECT *
                FROM raw_boilers
                WHERE (boilerid, stationid) IN (
                    SELECT boilerid, stationid
                        FROM raw_boilers
                        WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND production > 0 AND consumption > 0 AND kpd > 0
                        GROUP BY boilerid, stationid
                        HAVING COUNT(*) > 3
                        AND NOT EXISTS (
                            SELECT 1
                            FROM raw_boilers AS t2
                            WHERE t2.boilerid = raw_boilers.boilerid
                            AND t2.stationid = raw_boilers.stationid
                            AND t2.hours > 24
                            AND t2.date = (SELECT MAX(date)
                                            FROM raw_boilers AS t3
                                            WHERE t3.boilerid = t2.boilerid
                                            AND t3.stationid = t2.stationid)
                            )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND production > 0 AND consumption > 0 AND kpd > 0
                            ORDER BY stationid, boilerid, date;";
            select_TA = $@"SELECT *
                FROM raw_turbins
                WHERE (turbinid, stationid) IN (
                    SELECT turbinid, stationid
                        FROM raw_turbins
                        WHERE date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND consumption > 0 AND urt > 0
                        GROUP BY turbinid, stationid
                        HAVING COUNT(*) > 3
                        AND NOT EXISTS (
                            SELECT 1
                            FROM raw_turbins AS t2
                            WHERE t2.turbinid = raw_turbins.turbinid
                            AND t2.stationid = raw_turbins.stationid
                            AND t2.hours > 24
                            AND t2.date = (SELECT MAX(date)
                                            FROM raw_turbins AS t3
                                            WHERE t3.turbinid = t2.turbinid
                                            AND t3.stationid = t2.stationid)
                            )) and date > '{dt.Year + "-" + dt.Month + "-" + dt.Day}' AND consumption > 0 AND urt > 0
                                        ORDER BY stationid, turbinid, date;";
            DBFunctions.GetRelevantYearData_TA(Akscodes.rec_WT, Akscodes.weekTurbins, select_TA, connection1, connection);
            Console.WriteLine("Сделано 1.3");
            DBFunctions.GetRelevantYearData_KA(Akscodes.rec_WB, Akscodes.weekBoilers, select_KA, connection1, connection);
            Console.WriteLine("Сделано 2.3");
            DBFunctions.InsertFinalData_TA(SQL_config.truncate_week_TA, SQL_config.insertQuerry_week_TA, Akscodes.weekTurbins, connection1);
            Console.WriteLine("Сделано 3");
            DBFunctions.InsertFinalData_KA(SQL_config.truncate_week_KA, SQL_config.insertQuerry_week_KA, Akscodes.weekBoilers, connection1);
            Console.WriteLine("Сделано 4");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message} ");
    }
}

