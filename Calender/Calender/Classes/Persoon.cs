using Calender.Exceptions;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Calender
{
    public class Persoon
    {
        public Persoon(string voornaam, string achternaam)
        {
            Voornaam = voornaam;
            Achternaam = achternaam;
        }


        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public override string ToString()
        {
            return $"{Voornaam} {Achternaam}";
        }


        public void InsertPersoon()
        {
            Sqlconnect con = new Sqlconnect();
            con.conOpen();
            if (con != null && con.Con.State == ConnectionState.Open)
            {
                try
                {
                    con.Command = new SqlCommand($"INSERT INTO tblPersoon (VOORNAAM, ACHTERNAAM) values ('{this.Voornaam}', '{this.Achternaam}')", con.Con);
                    con.Command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new DBInsertFailed();
                }
            }
            else
            {
                throw new DBConnectFailed("Insert Persoon");
            }
            con.conClose();
        }
    }
}