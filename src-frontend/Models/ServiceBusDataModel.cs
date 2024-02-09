namespace src_functions.Models
{
    public class ServiceBusDataModel
    {
        public string? type { get; set; }
        public Properties? properties { get; set; }
    }

    public class Properties
    {
        public Id? id { get; set; }
        public Source? source { get; set; }
        public Specversion? specversion { get; set; }
        public Type? type { get; set; }
        public Subject? subject { get; set; }
        public Time? time { get; set; }
        public Data? data { get; set; }
    }

    public class Id
    {
        public string? type { get; set; }
    }

    public class Source
    {
        public string? type { get; set; }
    }

    public class Specversion
    {
        public string? type { get; set; }
    }

    public class Type
    {
        public string? type { get; set; }
    }

    public class Subject
    {
        public string? type { get; set; }
    }

    public class Time
    {
        public string? type { get; set; }
    }

    public class Data
    {
        public string? Id { get; set; }
        public string? EmailAddress { get; set; }
    }
}
