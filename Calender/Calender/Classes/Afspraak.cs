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
        public Afspraak(int id, DateTime startTime, DateTime endTime, string subject, string beschrijving, Boolean bezet) : this(id, startTime, endTime, subject, beschrijving, null, bezet) { }
        public Afspraak(int id, DateTime startTime, DateTime endTime, string subject, string beschrijving, Locatie locatie, Boolean bezet)
        {
            Id = id;
            StartTime = startTime;
            EndTime = endTime;
            Subject = subject;
            Beschrijving = beschrijving;
            Locatie = locatie;
            Bezet = bezet;
        }
        private DateTime _startTime;
        private DateTime _endTime;
        private string _subject;
        private string _beschrijving;
        private int _id;
        private Boolean _bezet;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value >= 0)
                {
                    _id = value;
                }
                else
                {

                    throw new NameIsEmpty("Afspraak ID");
                }

            }
        }


        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                if (value != DateTime.MinValue)
                {
                    _startTime = value;
                }
                else
                {
                    throw new NameIsEmpty("Afspraak StartTijd");

                }

            }
        }
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                if (value != DateTime.MinValue)
                {
                    _endTime = value;
                }
                else
                {
                    throw new NameIsEmpty("Afspraak EindTijd");

                }

            }
        }
        public string Subject
        {
            get
            {
                return _subject;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _subject = value;
                }
                else
                {
                    throw new NameIsEmpty("Afspraak Naam");

                }

            }
        }
        public string Beschrijving
        {
            get
            {
                return _beschrijving;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _beschrijving = value;
                }
                else
                {
                    throw new NameIsEmpty("Afspraak Beschrijving");

                }

            }
        }

        public Boolean Bezet
        {
            get
            {
                return _bezet;
            }
            set
            {
                _bezet = value;
            }
        }

        /// <summary>
        /// Locatie van de afspraak, wordt momenteel nog niet gebruikt
        /// </summary>
        public Locatie Locatie { get; set; }


        public override string ToString()
        {
            if (Locatie == null && Beschrijving != null) return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving}";
            else if (Locatie != null && Beschrijving != null) return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving} in {Locatie}";
            else return $"{StartTime}, {EndTime}, {Subject}: {Beschrijving} in {Locatie}";
        }

    }
}