using Calender.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Calender
{
    public class Kalender : IKalender
    {

        public Kalender(string naam, List<Afspraak> afsprakenLijst)
        {
            Naam = naam;
            AfsprakenLijst = afsprakenLijst;

        }
        public Kalender(string naam, string beschrijving, List<Afspraak> afsprakenLijst)
        {
            Naam = naam;
            AfsprakenLijst = afsprakenLijst;
            Beschrijving = beschrijving;

        }


        private string naam;
        

        public string Naam
        {
            get
            {
                return naam;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    naam = value;
                }
                else { throw new NameIsEmpty("Kalender"); }
            }
        }

        public string Beschrijving { get; set; }

        public IList<Afspraak> AfsprakenLijst { get; set; }
        public void VoegAfspraakToe(Afspraak afspraak)
        {
            AfsprakenLijst.Add(afspraak);

        }

        public void Delete()
        {
            AfsprakenLijst.Clear();
        }


        public override string ToString()
        {
            return Naam;
        }



    }

}
