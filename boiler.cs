namespace Altair.Models
{
    public class Boiler
    {
        public int Id { get; set; }
        public int StationID { get; set; }
        public string BoilerID { get; set; }
        public PeriodType PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public double KPD { get; set; }
        public double Production { get; set; }
        public double Consumption { get; set; }
    }
}