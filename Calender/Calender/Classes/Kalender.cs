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
        #region Fields
        private string naam;
        private string beschrijving;
        private int id;

        #endregion

        #region Properties

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
                else { throw new NameIsEmpty("KalenderNaam"); }
            }
        }

        public string Beschrijving
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
                else { throw new NameIsEmpty("KalenderBeschrijving"); }
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set { id = value; }
        }

        public DataRow row { get; }

        public IList<Afspraak> AfsprakenLijst { get; set; }

        #endregion

        #region Constructors

        public Kalender(int id, string naam, List<Afspraak> afsprakenLijst)
        {
            Id = id;
            Naam = naam;
            AfsprakenLijst = afsprakenLijst;

        }
        public Kalender(int id, string naam, string beschrijving, List<Afspraak> afsprakenLijst)
        {
            Id = id;
            Naam = naam;
            AfsprakenLijst = afsprakenLijst;
            Beschrijving = beschrijving;

        }
        #endregion

        #region Methods


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
    #endregion
}
