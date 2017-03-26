using System;

namespace ConsoleApp.Models
{
    public class FoodSummary
    {
        public DateTime Date { get; set; }
        public decimal CaloriesTotal { get; set; }
        public decimal ProteinTotal { get; set; }
        public decimal CarbsTotal { get; set; }
        public decimal ProteinPercentage => ProteinTotal + CarbsTotal > 0 ? ProteinTotal / (ProteinTotal + CarbsTotal) * 100 : 0;
        public decimal CarbsPercentage => ProteinTotal + CarbsTotal > 0 ? CarbsTotal / (ProteinTotal + CarbsTotal) * 100 : 0;
        public bool ProteinGoalAchived => ProteinTotal >= 120;
        public bool ProteinWins => ProteinTotal > CarbsTotal;
    }
}
