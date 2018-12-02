using Calender.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calender
{
    public class Afspraak : IAfspraak
    {
        public Afspraak(DateTime startTime, DateTime endTime, string subject, string beschrijving) : this(startTime, endTime, subject, beschrijving, null){}
        public Afspraak(DateTime startTime, DateTime endTime, string subject, string beschrijving, Persoon persoon) : this(startTime, endTime, subject, beschrijving, persoon, null){}
        public Afspraak(DateTime startTime, DateTime endTime, string subject, string beschrijving, Persoon persoon, Locatie locatie)
        {
            StartTime = startTime;
            EndTime = endTime;
            Subject = subject;
            Beschrijving = beschrijving;
            Persoon = persoon;
            Locatie = locatie;
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Subject { get; set; }

        public Persoon Persoon { get; set; }

        public string Beschrijving { get; set; }

        public Locatie Locatie { get; set; }


        public override string ToString()
        {
            if (Locatie == null && Persoon != null) return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving} met {Persoon}";
            else if (Locatie != null && Persoon == null) return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving} in {Locatie}";
            else return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving} in {Locatie} met {Persoon}";
        }

    }
}
