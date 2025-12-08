using Microsoft.Data.SqlClient;
using Altair.Models;
using Npgsql;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
public static class DBFunctions
{
    public static string Correct(int x)
    {
        if (x < 10)
            return "0" + x;
        else
            return x.ToString();
    }

public static void GenerateDB_KA(BoilerRecord rec_KA, Dictionary<int, List<string>> matching_dict_KA, string insertQuerry_KA, string format, NpgsqlConnection connection1, SqlConnection connection)
{
    foreach (var dic in matching_dict_KA)
    {
        foreach (string code in dic.Value)
        {
            string sql_start = $"exec dbo.p_GetParamValuePivot '{Correct(dic.Key)}','Основная', ";
            string sql_end = ", '2025-01-01 00:00:00',  '2025-12-31 23:00:00',  'Сутки';";
            
            // Парсим StationID и BoilerID
            rec_KA.StationID = short.Parse(code.Substring(11, 2));
            rec_KA.BoilerID = code.Substring(1, 2) + ((code[9] == '0') ? "" : code[9].ToString());
            rec_KA.BoilerID = rec_KA.BoilerID.Replace("A", "А").Replace("B", "Б");
            
            string a = sql_start + code + sql_end;
            SqlCommand command = new SqlCommand(a, connection);
            SqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                try
                {
                    // Дата сохраняется без изменений
                    rec_KA.Date = DateTime.ParseExact(reader[0].ToString().Substring(0, 10), format, CultureInfo.InvariantCulture);
                    
                    // Обрабатываем каждое значение отдельно
                    rec_KA.Consumption = TryParseDouble(reader[1]);
                    rec_KA.KPD = TryParseDouble(reader[2]);
                    rec_KA.Production = TryParseDouble(reader[3]);
                    rec_KA.Hours = int.TryParse(reader[4]?.ToString(), out var hours) ? hours : 0;
                    rec_KA.humidity = TryParseDouble(reader[5]);
                    rec_KA.ash = TryParseDouble(reader[6]);
                    rec_KA.temp_fact = TryParseDouble(reader[7]);
                    rec_KA.temp_nominal = TryParseDouble(reader[8]);
                    rec_KA.temp_koef = TryParseDouble(reader[9]);
                    // Создаем команду для вставки в PostgreSQL
                    using (var insertCommand = new NpgsqlCommand(insertQuerry_KA, connection1))
                    {
                        insertCommand.Parameters.AddWithValue("@BoilerID", rec_KA.BoilerID);
                        insertCommand.Parameters.AddWithValue("@StationID", rec_KA.StationID);
                        insertCommand.Parameters.AddWithValue("@Production", rec_KA.Production);
                        insertCommand.Parameters.AddWithValue("@KPD", rec_KA.KPD);
                        insertCommand.Parameters.AddWithValue("@Date", rec_KA.Date);
                        insertCommand.Parameters.AddWithValue("@Consumption", rec_KA.Consumption);
                        insertCommand.Parameters.AddWithValue("@Hours", rec_KA.Hours);
                        insertCommand.Parameters.AddWithValue("@Temp_fact", rec_KA.temp_fact);
                        insertCommand.Parameters.AddWithValue("@Temp_nominal", rec_KA.temp_nominal);
                        insertCommand.Parameters.AddWithValue("@Temp_koef", rec_KA.temp_koef);
                        insertCommand.Parameters.AddWithValue("@Humidity", rec_KA.humidity);
                        insertCommand.Parameters.AddWithValue("@Ash", rec_KA.ash);
                        
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке строки: {ex.Message}");
                }
            }
            reader.Close();
        }
    }
}

// Функция безопасной конвертации двойных значений
private static double TryParseDouble(object value)
{
    if (value is DBNull || string.IsNullOrWhiteSpace(value.ToString()))
        return 0;

    // Сначала нормализуем строку, заменяя запятые на точки
    string normalizedValue = value.ToString().Trim().Replace(',', '.');

    // Затем пытаемся распарсить нормализованное значение
    if (!double.TryParse(normalizedValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
        return 0;

    return Math.Round(result, 3); // Округляем до трёх знаков после запятой
}

private static double ParseOrDefault(object value, double defaultValue)
{
    if (value is DBNull || string.IsNullOrEmpty(value.ToString()))
        return defaultValue;
    
    try
    {
        return Math.Round(double.Parse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture), 3);
    }
    catch
    {
        return defaultValue;
    }
}

public static void GenerateDB_TA(TurbinRecord rec_TA, Dictionary<int, List<string>> matching_dict_TA, string insertQuerry_TA, string format, NpgsqlConnection connection1, SqlConnection connection)
{
    foreach (var dic in matching_dict_TA)
    {
        foreach (string code in dic.Value)
        {
            string sql_start = $"exec dbo.p_GetParamValuePivot '{Correct(dic.Key)}','Основная', ";
            string sql_end = ", '2025-01-01 00:00:00',  '2025-12-31 23:00:00',  'Сутки';";
            
            rec_TA.StationID = short.Parse(code.Substring(11, 2));
            rec_TA.TurbinID = code.Substring(1, 2) + ((code[9] == '0') ? "" : code[9].ToString());
            rec_TA.TurbinID = rec_TA.TurbinID.Replace("A", "А").Replace("B", "Б");
            
            string a = sql_start + code + sql_end;
            SqlCommand command = new SqlCommand(a, connection);
            SqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                try
                {
                    rec_TA.Date = DateTime.ParseExact(reader[0].ToString().Substring(0, 10), format, CultureInfo.InvariantCulture);
                    rec_TA.URT = TryParseDouble(reader[1]);
                    rec_TA.Consumption = TryParseDouble(reader[2]);
                    rec_TA.Hours = int.TryParse(reader[3]?.ToString(), out var hours) ? hours : 0;
                    rec_TA.variation = TryParseDouble(reader[4]);
                    
                    // ✅ ДОБАВЛЕНО: Читаем 5-е значение (номинальный УРТ из T002DU)
                    rec_TA.NominalURT = TryParseDouble(reader[5]);
                    
                    using (var insertCommand = new NpgsqlCommand(insertQuerry_TA, connection1))
                    {
                        insertCommand.Parameters.AddWithValue("@TurbinID", rec_TA.TurbinID);
                        insertCommand.Parameters.AddWithValue("@StationID", rec_TA.StationID);
                        insertCommand.Parameters.AddWithValue("@URT", rec_TA.URT);
                        insertCommand.Parameters.AddWithValue("@Date", rec_TA.Date);
                        insertCommand.Parameters.AddWithValue("@Consumption", rec_TA.Consumption);
                        insertCommand.Parameters.AddWithValue("@Hours", rec_TA.Hours);
                        insertCommand.Parameters.AddWithValue("@Variation", rec_TA.variation);
                        
                        // ✅ ДОБАВЛЕНО
                        insertCommand.Parameters.AddWithValue("@NominalURT", rec_TA.NominalURT);
                        
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке строки: {ex.Message}");
                }
                    switch (rec_TA.StationID)
                    {
                        case 25:
                            switch (rec_TA.TurbinID)
                            {
                                case "01А":
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    } 
            }
            reader.Close();
        }
    }
}


    public static void GetRelevantWeekData_TA(Turbin rec_WT, List<Turbin> weekTurbins, string select_TA, NpgsqlConnection connection1, SqlConnection connection)
{
    int index = weekTurbins.Count();
    int temp_StationID = 0;
    string temp_TurbinID = "0";
    double sum_cons = 0;
    double sum_multi_urt_cons = 0;
    double last_consumption = 0;
    DateTime temp_dt = new DateTime(2000, 1, 1);
    
    // ✅ ДОБАВЛЕНО
    double sum_nominal_urt = 0;
    int count = 0;
    
    using (var command = new NpgsqlCommand(select_TA, connection1))
    {
        using (var reader = command.ExecuteReader())
        {
            int i = 1;
            while (reader.Read())
            {
                if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_TurbinID == reader.GetString(reader.GetOrdinal("TurbinID")))
                {
                    sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_multi_urt_cons += (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt += reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                    count++;
                    i++;
                }
                else
                {
                    if (DateTime.Now.AddDays(-7).Date <= temp_dt.Date)
                    {
                        rec_WT.TurbinID = temp_TurbinID;
                        rec_WT.StationID = temp_StationID;
                        rec_WT.URT = (sum_multi_urt_cons / sum_cons);
                        rec_WT.Consumption = last_consumption;
                        rec_WT.PeriodValue = i;
                        
                        // ✅ ДОБАВЛЕНО: среднее номинальное значение
                        rec_WT.NominalURT = count > 0 ? sum_nominal_urt / count : 0;
                    }
                    rec_WT.PeriodType = PeriodType.Week;
                    weekTurbins.Add(rec_WT);
                    rec_WT = new Turbin();
                    i = 1;
                    count = 1; // ✅ ДОБАВЛЕНО
                    temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                    temp_TurbinID = reader.GetString(reader.GetOrdinal("TurbinID"));
                    sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_multi_urt_cons = (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt = reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                }
            }
        }
    }
    weekTurbins.RemoveAt(index);
}

public static void GetRelevantMonthData_TA(Turbin rec_WT, List<Turbin> weekTurbins, string select_TA, NpgsqlConnection connection1, SqlConnection connection)
{
    int index = weekTurbins.Count();
    int temp_StationID = 0;
    string temp_TurbinID = "0";
    double sum_cons = 0;
    double sum_multi_urt_cons = 0;
    double sum_urt = 0;
    double last_consumption = 0;
    DateTime temp_dt = new DateTime(2000, 1, 1);
    
    // ✅ ДОБАВЛЕНО
    double sum_nominal_urt = 0;
    int count = 0;
    
    using (var command = new NpgsqlCommand(select_TA, connection1))
    {
        using (var reader = command.ExecuteReader())
        {
            int i = 1;
            while (reader.Read())
            {
                if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_TurbinID == reader.GetString(reader.GetOrdinal("TurbinID")))
                {
                    sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_urt += reader.GetDouble(reader.GetOrdinal("URT"));
                    sum_multi_urt_cons += (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt += reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                    count++;
                    i++;
                }
                else
                {
                    if (DateTime.Now.AddDays(-30).Date <= temp_dt.Date)
                    {
                        rec_WT.TurbinID = temp_TurbinID;
                        rec_WT.StationID = temp_StationID;
                        rec_WT.URT = (sum_multi_urt_cons / sum_cons);
                        rec_WT.Consumption = sum_cons;
                        rec_WT.PeriodValue = i;
                        
                        // ✅ ДОБАВЛЕНО
                        rec_WT.NominalURT = count > 0 ? sum_nominal_urt / count : 0;
                    }
                    rec_WT.PeriodType = PeriodType.Month;
                    weekTurbins.Add(rec_WT);
                    rec_WT = new Turbin();
                    i = 1;
                    count = 1; // ✅ ДОБАВЛЕНО
                    temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                    temp_TurbinID = reader.GetString(reader.GetOrdinal("TurbinID"));
                    sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_urt = reader.GetDouble(reader.GetOrdinal("URT"));
                    sum_multi_urt_cons = (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt = reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                }
            }
        }
    }
    weekTurbins.RemoveAt(index);
}

public static void GetRelevantYearData_TA(Turbin rec_WT, List<Turbin> weekTurbins, string select_TA, NpgsqlConnection connection1, SqlConnection connection)
{
    int index = weekTurbins.Count();
    int temp_StationID = 0;
    string temp_TurbinID = "0";
    double sum_cons = 0;
    double sum_urt = 0;
    double sum_multi_urt_cons = 0;
    double last_consumption = 0;
    DateTime temp_dt = new DateTime(2000, 1, 1);
    
    // ✅ ДОБАВЛЕНО
    double sum_nominal_urt = 0;
    int count = 0;
    
    using (var command = new NpgsqlCommand(select_TA, connection1))
    {
        using (var reader = command.ExecuteReader())
        {
            int i = 1;
            while (reader.Read())
            {
                if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_TurbinID == reader.GetString(reader.GetOrdinal("TurbinID")))
                {
                    sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_urt += reader.GetDouble(reader.GetOrdinal("URT"));
                    sum_multi_urt_cons += (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt += reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                    count++;
                    i++;
                }
                else
                {
                    if (DateTime.Now.AddDays(-365).Date <= temp_dt.Date)
                    {
                        rec_WT.TurbinID = temp_TurbinID;
                        rec_WT.StationID = temp_StationID;
                        rec_WT.URT = (sum_multi_urt_cons / sum_cons);
                        rec_WT.Consumption = (sum_cons);
                        rec_WT.PeriodValue = i;
                        
                        // ✅ ДОБАВЛЕНО
                        rec_WT.NominalURT = count > 0 ? sum_nominal_urt / count : 0;
                    }
                    rec_WT.PeriodType = PeriodType.Year;
                    weekTurbins.Add(rec_WT);
                    rec_WT = new Turbin();
                    i = 1;
                    count = 1; // ✅ ДОБАВЛЕНО
                    temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                    temp_TurbinID = reader.GetString(reader.GetOrdinal("TurbinID"));
                    sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                    sum_urt = reader.GetDouble(reader.GetOrdinal("URT"));
                    sum_multi_urt_cons = (reader.GetDouble(reader.GetOrdinal("URT")) + reader.GetDouble(reader.GetOrdinal("variation"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                    last_consumption = reader.GetDouble(reader.GetOrdinal("consumption"));
                    temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    
                    // ✅ ДОБАВЛЕНО
                    sum_nominal_urt = reader.GetDouble(reader.GetOrdinal("nominal_urt"));
                }
            }
        }
    }
    weekTurbins.RemoveAt(index);
}


    public static void GetRelevantWeekData_KA(Boiler rec_WB, List<Boiler> weekBoilers, string select_KA, NpgsqlConnection connection1, SqlConnection connection)
        {
            int index = weekBoilers.Count();
            int temp_StationID = 0;
            string temp_boilerid = "0";
            double sum_multi_kpd_cons = 0;
            double last_production = 0;
            double sum_cons = 0;

            DateTime temp_dt = new DateTime(2000, 1, 1);
            using (var command = new NpgsqlCommand(select_KA, connection1))
            {
                using (var reader = command.ExecuteReader())
                {
                   int i = 1; 
                    while (reader.Read())
                    {
                    
                    if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_boilerid == reader.GetString(reader.GetOrdinal("boilerID")))
                    {
                        sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_multi_kpd_cons += (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                        i++;
                    }
                    else
                    {
                        if (DateTime.Now.AddDays(-7).Date <= temp_dt.Date)
                        {
                            rec_WB.BoilerID = temp_boilerid;
                            rec_WB.StationID = temp_StationID;
                            rec_WB.KPD = (sum_multi_kpd_cons / sum_cons);
                            rec_WB.Production = last_production;
                            rec_WB.Consumption = sum_cons / i;
                            rec_WB.PeriodValue = i;
                        }
                        rec_WB.PeriodType = PeriodType.Week;
                        weekBoilers.Add(rec_WB);
                        rec_WB = new Boiler();
                        i = 1;
                        temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                        temp_boilerid = reader.GetString(reader.GetOrdinal("boilerID"));
                        sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_multi_kpd_cons = (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    }
                    }
                }
            }
            weekBoilers.RemoveAt(index);
        }
    public static void GetRelevantMonthData_KA(Boiler rec_WB, List<Boiler> weekBoilers, string select_KA, NpgsqlConnection connection1, SqlConnection connection)
        {       
            int index = weekBoilers.Count();
            int temp_StationID = 0;
            string temp_boilerid = "0";
            double sum_multi_kpd_cons = 0;
            double sum_prod = 0;
            double last_production = 0;
            double sum_cons = 0;

            DateTime temp_dt = new DateTime(2000, 1, 1);
            using (var command = new NpgsqlCommand(select_KA, connection1))
            {
                using (var reader = command.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                    
                    if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_boilerid == reader.GetString(reader.GetOrdinal("boilerID")))
                    {
                        sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_multi_kpd_cons += (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_prod += reader.GetDouble(reader.GetOrdinal("production"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                        i++;
                    }
                    else
                    {
                        if (DateTime.Now.AddDays(-30).Date <= temp_dt.Date)
                        {
                            rec_WB.BoilerID = temp_boilerid;
                            rec_WB.StationID = temp_StationID;
                            rec_WB.KPD = (sum_multi_kpd_cons / sum_cons);
                            rec_WB.Production = sum_prod;
                            rec_WB.Consumption = sum_cons / i;
                            rec_WB.PeriodValue = i;
                        }
                        rec_WB.PeriodType = PeriodType.Month;
                        weekBoilers.Add(rec_WB);
                        rec_WB = new Boiler();
                        i = 1;
                        temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                        temp_boilerid = reader.GetString(reader.GetOrdinal("boilerID"));
                        sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_prod = reader.GetDouble(reader.GetOrdinal("production"));
                        sum_multi_kpd_cons = (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    }
                    }
                }
            }
            weekBoilers.RemoveAt(index);
        }
    public static void GetRelevantYearData_KA(Boiler rec_WB, List<Boiler> weekBoilers, string select_KA, NpgsqlConnection connection1, SqlConnection connection)
        {
            int index = weekBoilers.Count();
            int temp_StationID = 0;
            string temp_boilerid = "0";
            double sum_multi_kpd_cons = 0;
            double sum_prod = 0;
            double last_production = 0;
            double sum_cons = 0;

            DateTime temp_dt = new DateTime(2000, 1, 1);
            using (var command = new NpgsqlCommand(select_KA, connection1))
            {
                using (var reader = command.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                    
                    if (temp_StationID == reader.GetInt32(reader.GetOrdinal("StationID")) && temp_boilerid == reader.GetString(reader.GetOrdinal("boilerID")))
                    {
                        sum_cons += reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_multi_kpd_cons += (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        sum_prod += reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                        i++;
                    }
                    else
                    {
                        if (DateTime.Now.AddDays(-365).Date <= temp_dt.Date)
                        {
                            rec_WB.BoilerID = temp_boilerid;
                            rec_WB.StationID = temp_StationID;
                            rec_WB.KPD = (sum_multi_kpd_cons / sum_cons);
                            rec_WB.Production = sum_prod;
                            rec_WB.Consumption = sum_cons / i;
                            rec_WB.PeriodValue = i;
                        }
                        rec_WB.PeriodType = PeriodType.Year;
                        weekBoilers.Add(rec_WB);
                        rec_WB = new Boiler();
                        i = 1;
                        temp_StationID = reader.GetInt32(reader.GetOrdinal("StationID"));
                        temp_boilerid = reader.GetString(reader.GetOrdinal("boilerID"));
                        sum_cons = reader.GetDouble(reader.GetOrdinal("consumption"));
                        sum_prod = reader.GetDouble(reader.GetOrdinal("production"));
                        sum_multi_kpd_cons = (reader.GetDouble(reader.GetOrdinal("KPD")) - reader.GetDouble(reader.GetOrdinal("humidity")) - reader.GetDouble(reader.GetOrdinal("ash")) + (reader.GetDouble(reader.GetOrdinal("temp_fact")) - reader.GetDouble(reader.GetOrdinal("temp_nominal"))) * reader.GetDouble(reader.GetOrdinal("temp_koef"))) * reader.GetDouble(reader.GetOrdinal("consumption"));
                        last_production = reader.GetDouble(reader.GetOrdinal("production"));
                        temp_dt = reader.GetDateTime(reader.GetOrdinal("date"));
                    }
                    }
                }
            }
            weekBoilers.RemoveAt(index);
        }
    public static void InsertFinalData_TA(string truncate_week_TA, string insertQuerry_week_TA, List<Turbin> weekTurbins, NpgsqlConnection connection1)
{
    using (var trunc = new NpgsqlCommand(truncate_week_TA, connection1))
    {
        trunc.ExecuteNonQuery();
    }
    foreach (var WT in weekTurbins)
    {
        using (var week_ins = new NpgsqlCommand(insertQuerry_week_TA, connection1))
        {
            try
            {
                week_ins.Parameters.AddWithValue("@TurbinID", WT.TurbinID);
                week_ins.Parameters.AddWithValue("@StationID", WT.StationID);
                week_ins.Parameters.AddWithValue("@URT", WT.URT);
                week_ins.Parameters.AddWithValue("@Consumption", WT.Consumption);
                week_ins.Parameters.AddWithValue("@PeriodType", (int)WT.PeriodType);
                week_ins.Parameters.AddWithValue("@PeriodValue", WT.PeriodValue);
                
                // ✅ ДОБАВЛЕНО
                week_ins.Parameters.AddWithValue("@NominalURT", WT.NominalURT);
                
                week_ins.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка для турбины {WT.TurbinID}: {ex.Message}");
            }
        }
    }
}


    public static void InsertFinalData_KA(string truncate_week_KA, string insertQuerry_week_KA, List<Boiler> weekBoilers, NpgsqlConnection connection1)
    {
        using (var trunc = new NpgsqlCommand(truncate_week_KA, connection1))
        {
            trunc.ExecuteNonQuery();
        }
        foreach (var WB in weekBoilers)
        {
            using (var week_ins = new NpgsqlCommand(insertQuerry_week_KA, connection1))
            {
                week_ins.Parameters.AddWithValue("@BoilerID", WB.BoilerID);
                week_ins.Parameters.AddWithValue("@StationID", WB.StationID);
                week_ins.Parameters.AddWithValue("@KPD", WB.KPD);
                week_ins.Parameters.AddWithValue("@Production", WB.Production);
                week_ins.Parameters.AddWithValue("@Consumption", WB.Consumption);
                week_ins.Parameters.AddWithValue("@PeriodType", (int)WB.PeriodType);
                week_ins.Parameters.AddWithValue("@PeriodValue", WB.PeriodValue);
                week_ins.ExecuteNonQuery();
            }
        }
    } 
}