using Calender.Exceptions;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Calender
{
    public class Persoon
    {
        #region Constructor
        public Persoon(string voornaam, string achternaam)
        {
            Voornaam = voornaam;
            Achternaam = achternaam;
        }
        #endregion

        #region properties


        public string Voornaam { get; set; }
        public string Achternaam { get; set; }

        #endregion

        #region override
        public override string ToString()
        {
            return $"{Voornaam} {Achternaam}";
        }

        #endregion

    }
}