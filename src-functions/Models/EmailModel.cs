namespace src_functions.Models
{
    public class EmailModel
    {
        public string? EmailAddress { get; set; }
        public string? DrinkName { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> DrinkIngredients { get; set; } = new List<string>();
    }
}
