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

        private SqlDataReader reader;
        private SqlCommand command;
        private SqlCommandBuilder builder;
        private SqlParameter parameter;
        private DataRow row;

        private DataSet dataset = new DataSet();

        private SqlDataAdapter adapterKalender;
        private SqlDataAdapter adapterAfspraak;

        #endregion

        #region Properties
        public SqlDataReader Reader
        {
            get { return reader; }
            set { reader = value; }
        }


        public SqlCommand Command
        {
            get { return command; }
            set { command = value; }
        }

        #endregion

        #region Methods
        public void conOpen()
        {
            conString = @"Data Source = (localdb)\mssqllocaldb; Initial Catalog = KalenderApplicatie; Integrated Security = True; Pooling = False";
            Con = new SqlConnection(conString);//
            try
            {
                Con.Open();
            }
            catch (Exception ex)
            {
                throw new DBConnectFailed("Sqlconnect class");
            }
        }

        public void Kalender()
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

                dataset.Tables["Kalender"].PrimaryKey = new DataColumn[] { dataset.Tables["Kalender"].Columns["Id"]};

            }
            else
            {
                MessageBox.Show("De database kan niet geladen worden, het programma sluit nu af");
                Environment.Exit(-1);
            }


        }

        public void Afspraak()
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
                DataRelation relation = new DataRelation("AfspraakKalender", dataset.Tables["Kalender"].Columns["id"], dataset.Tables["Afspraak"].Columns["KalenderId"]);
                dataset.Relations.Add(relation);
                relation.ChildKeyConstraint.UpdateRule = Rule.Cascade;
            }
            else
            {
                MessageBox.Show("De database kan niet geladen worden, het programma sluit nu af");
                Environment.Exit(-1);
            }
        }





        public void InsertKalender(IKalender kalender)
        {
            row = dataset.Tables["Kalender"].NewRow();
            row["Naam"] = kalender.Naam;
            dataset.Tables["Kalender"].Rows.Add(row);
            adapterKalender.Update(dataset, "Kalender");
        }

        public void InsertAfspraak(Afspraak afspraak)
        {

            row = dataset.Tables["Afspraak"].NewRow();

            row["startTime"] = (DateTime)afspraak.StartTime;
            row["endTime"] = (DateTime)afspraak.EndTime;
            row["subject"] = (string)afspraak.Subject;
            row["beschrijving"] = (string)afspraak.Beschrijving;


            dataset.Tables["Afspraak"].Rows.Add(row);

            adapterAfspraak.Update(dataset, "Afspraak");

        }

        public void DeleteAfspraak(Afspraak afspraak)
        {
            DataRow row = dataset.Tables["Afspraak"].Rows[0];
            row.Delete();
            adapterAfspraak.Update(dataset, "Afspraak");
        }

        public void DeleteKalender(IKalender kalender)
        {

            //ObservableCollection<IKalender> kalenderlijst = SelectKalender(kalender);

            //IEnumerable<IKalender> results = kalenderlijst.Where(x => x.Id == kalender.Id);

            DataRow row = dataset.Tables["Kalender"].Rows.Find((int)kalender.Id);
            row.Delete();
            adapterKalender.Update(dataset, "Kalender");


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
                        //if((DateTime)row["startTime"] == afspraak.StartTime)
                        //{


                        //}

                        KalenderLijst.Add(new Kalender((int)row["Id"],(string)row["Naam"], SelectAfspraak()));

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan");

            }

            return KalenderLijst;
        }

        public List<Afspraak> SelectAfspraak(Afspraak afspraak)
        {

            List<Afspraak> returnlist = new List<Afspraak>();
            try
            {
                if (dataset.Tables["Afspraak"].Rows.Count > 0)
                {
                    foreach (DataRow row in dataset.Tables["Afspraak"].Rows)
                    {
                        //if((DateTime)row["startTime"] == afspraak.StartTime)
                        //{


                        //}

                        returnlist.Add(new Afspraak((DateTime)row["startTime"], (DateTime)row["endTime"], (string)row["subject"], (string)row["beschrijving"]));

                    }

                }
            }catch(Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan");
            }
            return returnlist;
        }

        public List<Afspraak> SelectAfspraak()
        {
            List<Afspraak> returnlist = new List<Afspraak>();

            if (dataset.Tables["Afspraak"].Rows.Count > 0)
            {
                foreach (DataRow row in dataset.Tables["Afspraak"].Rows)
                {
                    //if((DateTime)row["startTime"] == afspraak.StartTime)
                    //{


                    //}

                    returnlist.Add(new Afspraak((DateTime)row["startTime"], (DateTime)row["endTime"], (string)row["subject"], (string)row["beschrijving"]));

                }

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
                        //if((DateTime)row["startTime"] == afspraak.StartTime)
                        //{


                        //}

                        KalenderLijst.Add(new Kalender((int)row["id"],(string)row["Naam"], SelectAfspraak()));

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Een error heeft zich voorgedaan!");

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
            Con.Close();//close the connection
        }

        #endregion
    }
}
