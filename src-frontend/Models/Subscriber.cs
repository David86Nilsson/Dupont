using System.ComponentModel.DataAnnotations;

namespace src_frontend.Models
{
    public class Subscriber
    {
        [Key]
        public string Id { get; set; } = "";

        public string? EmailAddress { get; set; }
    }
}
