using System;

namespace Calender
{
    public interface IAfspraak
    {
        string Beschrijving { get; set; }
        DateTime EndTime { get; set; }
        int Id { get; set; }
        Locatie Locatie { get; set; }
        DateTime StartTime { get; set; }
        string Subject { get; set; }

        string ToString();
    }
}