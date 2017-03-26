using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Unit
    {
        public int id { get; set; }
        public string name { get; set; }
        public string plural { get; set; }
    }

    public class LoggedFood
    {
        public string accessLevel { get; set; }
        public decimal amount { get; set; }
        public string brand { get; set; }
        public int calories { get; set; }
        public int foodId { get; set; }
        public string locale { get; set; }
        public int mealTypeId { get; set; }
        public string name { get; set; }
        public Unit unit { get; set; }
        public List<int> units { get; set; }
        public string creatorEncodedId { get; set; }
    }

    public class NutritionalValues
    {
        public int calories { get; set; }
        public decimal carbs { get; set; }
        public decimal fat { get; set; }
        public decimal fiber { get; set; }
        public decimal protein { get; set; }
        public decimal sodium { get; set; }
    }

    public class Food
    {
        public bool isFavorite { get; set; }
        public string logDate { get; set; }
        public object logId { get; set; }
        public LoggedFood loggedFood { get; set; }
        public NutritionalValues nutritionalValues { get; set; }
    }

    public class Goals
    {
        public int calories { get; set; }
    }

    public class Summary
    {
        public int calories { get; set; }
        public decimal carbs { get; set; }
        public decimal fat { get; set; }
        public decimal fiber { get; set; }
        public decimal protein { get; set; }
        public decimal sodium { get; set; }
        public int water { get; set; }
    }

    public class FoodResponse
    {
        public List<Food> foods { get; set; }
        public Goals goals { get; set; }
        public Summary summary { get; set; }
    }
}
