using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Calender.Exceptions;
using System.Data;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;


namespace Calender
{

    public class Sqlconnect
    {

        #region Constructor
        public Sqlconnect()
        {
            Kalender();
            Afspraak();
        }
        #endregion

        #region fields
        public SqlConnection Con { get; set; }
        private string conString { get; set; }

        private SqlCommand command;
        private SqlCommandBuilder builder;
        private SqlParameter parameter;
        private DataRow row;

        private DataSet dataset = new DataSet();

        private SqlDataAdapter adapterKalender;
        private SqlDataAdapter adapterAfspraak;

        #endregion

        #region Properties



        public SqlCommand Command
        {
            get { return command; }
            set { command = value; }
        }

        #endregion

        #region Methods
        public void conOpen()
        {
            conString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CalenderDb.mdf;Integrated Security=True";
            //Dit is wanneer je met de .mdf file wilt werken
            //conString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = 'D:\Odisee\3TI\Application development 3\KalenderApplicatie\Calender\Calender\bin\Debug\AD3_Kalender_TijsDeBelie.mdf'; Integrated Security = True; Connect Timeout = 30";

            
            
            Con = new SqlConnection(conString);
            try
            {
                Con.Open();
            }
            catch (Exception ex)
            {
                throw new DBConnectFailed("Sqlconnect class\n" + ex.Message);
            }
        }

        public void Kalender()
        {
            try
            {
                conOpen();
                if (Con != null && Con.State == ConnectionState.Open)
                {
                    adapterKalender = new SqlDataAdapter("select * from tblKalender", Con);
                    builder = new SqlCommandBuilder(adapterKalender);
                    adapterKalender.InsertCommand = builder.GetInsertCommand().Clone();
                    adapterKalender.UpdateCommand = builder.GetUpdateCommand().Clone();
                    adapterKalender.DeleteCommand = builder.GetDeleteCommand().Clone();

                    builder.Dispose();

                    adapterKalender.InsertCommand.CommandText += " set @newid = scope_identity()";

                    parameter = new SqlParameter("@newid", 0);
                    parameter.SqlDbType = SqlDbType.Int;
                    parameter.Size = 4;
                    parameter.Direction = ParameterDirection.Output;

                    adapterKalender.InsertCommand.Parameters.Add(parameter);
                    adapterKalender.RowUpdated += AdapterKalender_RowUpdated;


                    adapterKalender.Fill(dataset, "Kalender");

                    dataset.Tables["Kalender"].Columns["Id"].AutoIncrement = true;
                    dataset.Tables["Kalender"].Columns["Id"].AutoIncrementSeed = -1;
                    dataset.Tables["Kalender"].Columns["Id"].AutoIncrementStep = -1;

                    dataset.Tables["Kalender"].PrimaryKey = new DataColumn[] { dataset.Tables["Kalender"].Columns["Id"] };



                    adapterKalender.Update(dataset, "Kalender");
                }
                else
                {
                    MessageBox.Show("De database kan niet geladen worden, het programma sluit nu af");
                    Environment.Exit(-1);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Er kwam een error voor bij het initialiseren van de database tabel kalender\nHet programma sluit nu af!");
                Environment.Exit(-1);
            }
        }

        public void Afspraak()
        {
            try
            {
                conOpen();
                if (Con != null && Con.State == ConnectionState.Open)
                {
                    adapterAfspraak = new SqlDataAdapter("select * from tblAfspraak", Con);
                    builder = new SqlCommandBuilder(adapterAfspraak);
                    adapterAfspraak.InsertCommand = builder.GetInsertCommand().Clone();
                    adapterAfspraak.UpdateCommand = builder.GetUpdateCommand().Clone();
                    adapterAfspraak.DeleteCommand = builder.GetDeleteCommand().Clone();

                    builder.Dispose();

                    adapterAfspraak.InsertCommand.CommandText += " set @id = scope_identity()";

                    SqlParameter parameterNewID = new SqlParameter("@id", 0);
                    parameterNewID.SqlDbType = SqlDbType.Int;
                    parameterNewID.Size = 4;
                    parameterNewID.Direction = ParameterDirection.Output;

                    adapterAfspraak.InsertCommand.Parameters.Add(parameterNewID);

                    adapterAfspraak.RowUpdated += AdapterAfspraak_RowUpdated;

                    adapterAfspraak.Fill(dataset, "Afspraak");

                    dataset.Tables["Afspraak"].Columns["Id"].AutoIncrement = true;
                    dataset.Tables["Afspraak"].Columns["Id"].AutoIncrementSeed = -1;
                    dataset.Tables["Afspraak"].Columns["Id"].AutoIncrementStep = -1;

                    dataset.Tables["Afspraak"].PrimaryKey = new DataColumn[] { dataset.Tables["Afspraak"].Columns["Id"] };
                    DataRelation relationAfspraak = new DataRelation("AfspraakKalender", dataset.Tables["Kalender"].Columns["Id"], dataset.Tables["Afspraak"].Columns["KalenderID"]);
                    dataset.Relations.Add(relationAfspraak);
                    relationAfspraak.ChildKeyConstraint.UpdateRule = Rule.Cascade;

                    dataset.Tables["Afspraak"].PrimaryKey = new DataColumn[] { dataset.Tables["Afspraak"].Columns["Id"] };

                    adapterAfspraak.Update(dataset, "Afspraak");
                }
                else
                {
                    MessageBox.Show("De database kan niet geladen worden, het programma sluit nu af");
                    Environment.Exit(-1);
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Er kwam een error voor bij de initialisatie van de database tabel Afspraak!\nHet programma sluit nu af!");
                Environment.Exit(1);
            }
        }





        public void InsertKalender(IKalender kalender)
        {
            try
            {
                row = dataset.Tables["Kalender"].NewRow();
                row["Naam"] = kalender.Naam;
                row["Beschrijving"] = kalender.Beschrijving;
                dataset.Tables["Kalender"].Rows.Add(row);
                adapterKalender.Update(dataset, "Kalender");
               
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon de nieuwe kalender niet invoegen\n" + ex.Message);
            }
        }

        public void BevestigAfspraak(int times)
        {

            MessageBox.Show($"Nieuwe Afspraak is {times} keer toegevoegd");

        }

        public void InsertAfspraak(Afspraak afspraak, IKalender kalender)
        {

            row = dataset.Tables["Afspraak"].NewRow();

            row["startTime"] = (DateTime)afspraak.StartTime;
            row["endTime"] = (DateTime)afspraak.EndTime;
            row["subject"] = (string)afspraak.Subject ?? "Lege naam";
            row["beschrijving"] = (string)afspraak.Beschrijving ?? "Lege beschrijving";
            row["KalenderID"] = (int)kalender.Id;
            row["Bezet"] = (bool)afspraak.Bezet;

            dataset.Tables["Afspraak"].Rows.Add(row);

            adapterAfspraak.Update(dataset, "Afspraak");

        }

        public void DeleteAfspraak(Afspraak afspraak)
        {
            try
            {

                DataRow row = dataset.Tables["Afspraak"].Rows.Find((int)afspraak.Id);
                row.Delete();
                adapterAfspraak.Update(dataset, "Afspraak");

                MessageBox.Show("Afspraak is verwijderd");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon afspraak niet verwijderen\n" + ex.Message);

            }
        }

        public void DeleteAfspraak(IKalender kalender)
        {
            try
            {

                foreach (DataRow rowAfspraak in dataset.Tables["Afspraak"].Rows)
                {
                    if ((int)rowAfspraak[5] == kalender.Id)
                    {
                        rowAfspraak.Delete();

                    }


                }

                adapterAfspraak.Update(dataset, "Afspraak");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon afspraak niet verwijderen\n" + ex.Message);

            }
        }

        public void DeleteKalender(IKalender kalender)
        {
            try
            {
                //ObservableCollection<IKalender> kalenderlijst = SelectKalender(kalender);

                //IEnumerable<IKalender> results = kalenderlijst.Where(x => x.Id == kalender.Id);

                foreach (DataRow rowAfspraak in dataset.Tables["Afspraak"].Rows)
                {
                    if ((int)rowAfspraak[5] == kalender.Id)
                    {
                        rowAfspraak.Delete();

                    }


                }
                adapterAfspraak.Update(dataset, "Afspraak");

                DataRow row = dataset.Tables["Kalender"].Rows.Find((int)kalender.Id);
                row.Delete();
                adapterKalender.Update(dataset, "Kalender");
                MessageBox.Show("Kalender is verwijderd");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Kon kalender niet verwijderen!\n" + ex.Message);
            }

        }
        public ObservableCollection<IKalender> SelectKalender(IKalender kalender)
        {
            ObservableCollection<IKalender> KalenderLijst = new ObservableCollection<IKalender>();
            try
            {

                if (dataset.Tables["Kalender"].Rows.Count > 0)
                {
                    foreach (DataRow row in dataset.Tables["Kalender"].Rows)
                    {
                        KalenderLijst.Add(new Kalender((int)row["Id"], (string)row["Naam"], (string)row["Beschrijving"], SelectAfspraak()));
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan bij het selecteren van de kalenders!\n" + ex.Message);

            }

            return KalenderLijst;
        }

        public void UpdateAfspraak(int Id, DateTime StartTime, DateTime EndTime, string Subject, string Beschrijving, IKalender kalender)
        {
            try
            {
                if (dataset.Tables["Afspraak"].Rows.Count > 0)
                {
                    DataRow row = dataset.Tables["Afspraak"].Rows.Find((int)Id);
                    row[1] = StartTime;
                    row[2] = EndTime;
                    row[3] = Subject ?? "Lege Naam";
                    row[4] = Beschrijving ?? "Lege Beschrijving";
                    row[5] = kalender.Id;

                    adapterAfspraak.Update(dataset, "Afspraak");
                    MessageBox.Show("Afspraak is gewijzigd");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kon Afspraak niet wijzigen!\n" + ex.Message);

            }
        }

        public void Updatekalender(int Id, string Naam, string Beschrijving, IKalender kalender)
        {
            try
            {

                if (dataset.Tables["Kalender"].Rows.Count > 0)
                {
                    DataRow row = dataset.Tables["Kalender"].Rows.Find((int)Id);

                    row[1] = Naam;
                    row[2] = Beschrijving;
                    adapterKalender.Update(dataset, "Kalender");
                    MessageBox.Show("Kalender is gewijzigd");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Kon Kalender niet wijzigen\n" + ex.Message);
            }

        }

        public List<IAfspraak> SelectAfspraak(IKalender kalender)
        {

            List<IAfspraak> returnlist = new List<IAfspraak>();
            try
            {   //TODO vind record in juiste kolom, niet via primary key value
                //DataRow results = dataset.Tables["Afspraak"].Rows.Find((int)kalender.Id);

                if (dataset.Tables["Afspraak"].Rows.Count > 0)
                {
                    foreach (DataRow row in dataset.Tables["Afspraak"].Rows)
                    {
                        if ((int)row[5] == kalender.Id) //ROW 5 IS NULL
                        {
                            returnlist.Add(new Afspraak((int)row["Id"], (DateTime)row["startTime"], (DateTime)row["endTime"], (string)row["subject"], (string)row["beschrijving"],(bool)row["Bezet"]));

                        }


                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan bij het selecteren van de afspraak\n" + ex.Message);
            }
            return returnlist;
        }

        public List<IAfspraak> SelectAfspraak()
        {
            List<IAfspraak> returnlist = new List<IAfspraak>();
            try
            {
                if (dataset.Tables["Afspraak"].Rows.Count > 0)
                {
                    foreach (DataRow row in dataset.Tables["Afspraak"].Rows)
                    {
                        returnlist.Add(new Afspraak((int)row["Id"], (DateTime)row["startTime"], (DateTime)row["endTime"], (string)row["Subject"], (string)row["Beschrijving"],(bool)row["Bezet"]));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan bij het selecteren van de afspraak\n" + ex.Message);

            }
            return returnlist;
        }
        public ObservableCollection<IKalender> SelectKalender()
        {
            ObservableCollection<IKalender> KalenderLijst = new ObservableCollection<IKalender>();
            try
            {

                if (dataset.Tables["Kalender"].Rows.Count > 0)
                {
                    foreach (DataRow row in dataset.Tables["Kalender"].Rows)
                    {
                        Kalender kalender = new Kalender((int)row[0], (string)row[1], (string)row[2], new List<IAfspraak>());
                        kalender.AfsprakenLijst = SelectAfspraak(kalender);
                        KalenderLijst.Add(kalender);

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan bij het selecteren van de kalenders!\n" + ex.Message);

            }

            return KalenderLijst;
        }



        private static void AdapterKalender_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                e.Row["id"] = e.Command.Parameters["@newid"].Value;
            }
        }
        private static void AdapterAfspraak_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                e.Row["Id"] = e.Command.Parameters["@id"].Value;
            }
        }




        public void conClose()
        {
            Con.Close();
        }

        #endregion
    }
}
