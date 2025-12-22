-- Таблица для хранения месячных данных котлов (из АСТЭП с параметром 'Месяц')
CREATE TABLE IF NOT EXISTS raw_boilers_monthly (
    id SERIAL PRIMARY KEY,
    BoilerID VARCHAR(10) NOT NULL,
    stationID SMALLINT NOT NULL,
    KPD NUMERIC(10, 3),
    production NUMERIC(15, 3),
    month_date DATE NOT NULL,
    consumption NUMERIC(15, 3),
    hours INTEGER,
    temp_fact NUMERIC(10, 3),
    temp_nominal NUMERIC(10, 3),
    temp_koef NUMERIC(10, 5),
    humidity NUMERIC(10, 3),
    ash NUMERIC(10, 3),
    UNIQUE(BoilerID, stationID, month_date)
);

-- Таблица для хранения месячных данных турбин (из АСТЭП с параметром 'Месяц')
CREATE TABLE IF NOT EXISTS raw_turbins_monthly (
    id SERIAL PRIMARY KEY,
    TurbinID VARCHAR(10) NOT NULL,
    stationID SMALLINT NOT NULL,
    URT NUMERIC(10, 3),
    consumption NUMERIC(15, 3),
    month_date DATE NOT NULL,
    hours INTEGER,
    variation NUMERIC(10, 3),
    nominal_urt NUMERIC(10, 3),
    UNIQUE(TurbinID, stationID, month_date)
);

-- Добавляем колонку Date в таблицу Boilers если её нет
ALTER TABLE "Boilers" ADD COLUMN IF NOT EXISTS "Date" DATE;

-- Добавляем колонку Date в таблицу Turbins если её нет
ALTER TABLE "Turbins" ADD COLUMN IF NOT EXISTS "Date" DATE;

-- Создаём уникальный индекс для UPSERT в Boilers
CREATE UNIQUE INDEX IF NOT EXISTS idx_boilers_unique 
ON "Boilers"("BoilerID", "StationID", "PeriodType", "Date") 
WHERE "Date" IS NOT NULL;

-- Создаём уникальный индекс для UPSERT в Turbins
CREATE UNIQUE INDEX IF NOT EXISTS idx_turbins_unique 
ON "Turbins"("TurbinID", "StationID", "PeriodType", "Date") 
WHERE "Date" IS NOT NULL;
