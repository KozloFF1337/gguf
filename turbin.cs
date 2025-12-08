namespace Altair.Models
{
    public class Turbin
    {
        public int Id { get; set; }
        public int StationID { get; set; }
        public string TurbinID { get; set; }
        public PeriodType PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public double URT { get; set; }
        public double Consumption { get; set; }
    }
}