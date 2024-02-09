namespace src_functions.Models
{
    public class ServiceBusDataModel
    {
        public string? id { get; set; }
        public string? source { get; set; }
        public string? type { get; set; }
        public string? time { get; set; }
        public string? subject { get; set; }
        public EmailDataModel? data { get; set; }

    }
    public class EmailDataModel
    {
        public string? Id { get; set; }
        public string? EmailAddress { get; set; }
    }
}
