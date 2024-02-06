namespace src_functions.Models
{
    public class DrinkModel
    {
        public string? Name { get; set; }
        public List<string> Ingredients { get; set; } = new List<string>();
    }
}
