using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Calender.Exceptions;
using System.Data;

namespace Calender
{
    public class Sqlconnect
    {
        public Sqlconnect()
        {
            Kalender();
            Afspraak();
        }


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


            }
            else
            {

                throw new Exception();
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


                DataRelation relation = new DataRelation("AfspraakKalender", dataset.Tables["Kalender"].Columns["id"], dataset.Tables["Afspraak"].Columns["KalenderId"]);
                dataset.Relations.Add(relation);
                relation.ChildKeyConstraint.UpdateRule = Rule.Cascade;
            }
            else
            {

                throw new Exception();
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
            DataRow row = dataset.Tables["Kalender"].Rows[0];
            row.Delete();
            adapterKalender.Update(dataset, "Afspraak");


        }

        public void SelectAfspraak(Afspraak afspraak)
        {
            DataSet returnDataset = new DataSet();
            if (dataset.Tables["Afspraak"].Rows.Count > 0)
            {
                foreach (DataRow row in dataset.Tables["Afspraak"].Rows)
                {
                    if((DateTime)row["startTime"] == afspraak.StartTime)
                    {

                        //Adding it to the list
                    }

                }
            }
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


    }
}
