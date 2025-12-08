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
                    
                    // ✅ ИЗМЕНЕНО: Проверяем существование 6-го поля (индекс 5)
                    if (reader.FieldCount > 5)
                    {
                        rec_TA.NominalURT = TryParseDouble(reader[5]);
                    }
                    else
                    {
                        rec_TA.NominalURT = 0;
                    }
                    
                    using (var insertCommand = new NpgsqlCommand(insertQuerry_TA, connection1))
                    {
                        insertCommand.Parameters.AddWithValue("@TurbinID", rec_TA.TurbinID);
                        insertCommand.Parameters.AddWithValue("@StationID", rec_TA.StationID);
                        insertCommand.Parameters.AddWithValue("@URT", rec_TA.URT);
                        insertCommand.Parameters.AddWithValue("@Date", rec_TA.Date);
                        insertCommand.Parameters.AddWithValue("@Consumption", rec_TA.Consumption);
                        insertCommand.Parameters.AddWithValue("@Hours", rec_TA.Hours);
                        insertCommand.Parameters.AddWithValue("@Variation", rec_TA.variation);
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
