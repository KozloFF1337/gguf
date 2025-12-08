public static class SQL_config
{
    public static string truncate_week_TA = @"TRUNCATE TABLE ""Turbins"" RESTART IDENTITY CASCADE;";
    public static string truncate_week_KA = @"TRUNCATE TABLE ""Boilers"" RESTART IDENTITY CASCADE;";
    public static string format = "dd.MM.yyyy";
    public static string connectionStringPGSQL = @"Server=localhost;Port=5432;Database=ASTEP_RES;User Id=postgres;Password=root;";
    public static string connectionString = "Server=KMR-S-APP-TEP3.CORP.SUEK.RU; Database=ASTEPSGKID; Trusted_Connection = True; TrustServerCertificate=true;";
    public static string createTableQuery = @"
    CREATE TABLE IF NOT EXISTS Boilers (
        ID SERIAL PRIMARY KEY,
        BoilerID VARCHAR(10) NOT NULL,
        stationID SMALLINT,
        KPD NUMERIC(3, 3),
        value NUMERIC(10, 3), 
        Date TIMESTAMP
    );";
    public static string insertQuerry_KA = @"
    INSERT INTO raw_boilers(BoilerID, stationID, KPD, production, date, consumption, hours, temp_fact, temp_nominal, temp_koef, humidity, ash)
    VALUES(@BoilerID, @StationID, @KPD, @Production, @Date, @Consumption, @Hours, @Temp_fact, @Temp_nominal, @Temp_koef, @Humidity, @Ash)  
    ON CONFLICT (BoilerID, stationID, date) DO NOTHING;";

    public static string insertQuerry_TA = @"
    INSERT INTO raw_turbins(TurbinID, stationID, URT, consumption, date, hours, variation)
    VALUES(@TurbinID, @StationID, @URT, @Consumption, @Date, @Hours, @Variation)  
    ON CONFLICT (TurbinID, stationID, date) DO NOTHING;";

    public static string insertQuerry_week_KA = @"
    INSERT INTO ""Boilers""(""BoilerID"", ""StationID"", ""KPD"", ""Production"", ""Consumption"",""PeriodType"",""PeriodValue"")
    VALUES (@BoilerID, @StationID, @KPD, @Production, @Consumption, @PeriodType, @PeriodValue);";

    public static string insertQuerry_week_TA = @"
    INSERT INTO ""Turbins""(""TurbinID"", ""StationID"", ""URT"", ""Consumption"",""PeriodType"",""PeriodValue"")
    VALUES(@TurbinID, @StationID, @URT, @Consumption, @PeriodType, @PeriodValue);";
}