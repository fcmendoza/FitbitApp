using System;
using System.Configuration;

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
        public bool ProteinGoalAchived => ProteinTotal >= _proteinGoal;
        public bool ProteinWins => ProteinTotal > CarbsTotal;
        public bool CarbProteinRatioIsHigh => CarbsTotal/ProteinTotal > 2;

        public FoodSummary()
        {
            _proteinGoal = int.TryParse(ConfigurationManager.AppSettings["ProteinGoal"], out _proteinGoal) ? _proteinGoal : 120;
        }
        private int _proteinGoal;
    }
}
