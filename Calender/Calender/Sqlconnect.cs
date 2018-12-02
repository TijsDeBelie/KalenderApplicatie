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

                parameter = new SqlParameter();
                parameter.ParameterName = "@newid";
                parameter.SqlDbType = SqlDbType.Int;
                parameter.Size = 4;
                parameter.Direction = ParameterDirection.Output;

                adapterKalender.InsertCommand.Parameters.Add(parameter);
                adapterKalender.RowUpdated += Adapter_RowUpdated;

                
                adapterKalender.Fill(dataset, "Kalender");

                dataset.Tables["Kalender"].Columns["id"].AutoIncrement = true;
                dataset.Tables["Kalender"].Columns["id"].AutoIncrementSeed = -1;
                dataset.Tables["Kalender"].Columns["id"].AutoIncrementStep = -1;


            }


        }

        public void Afspraak()
        {
            conOpen();
            if (Con != null && Con.State == ConnectionState.Open)
            {
                adapterAfspraak = new SqlDataAdapter("select * from tblAfspraak", Con);
                builder = new SqlCommandBuilder(adapterKalender);
                adapterAfspraak.InsertCommand = builder.GetInsertCommand().Clone();
                adapterAfspraak.UpdateCommand = builder.GetUpdateCommand().Clone();
                adapterAfspraak.DeleteCommand = builder.GetDeleteCommand().Clone();

                builder.Dispose();

                adapterAfspraak.InsertCommand.CommandText += " set @newid = scope_identity()";

                parameter = new SqlParameter();
                parameter.ParameterName = "@newid";
                parameter.SqlDbType = SqlDbType.Int;
                parameter.Size = 4;
                parameter.Direction = ParameterDirection.Output;

                adapterAfspraak.InsertCommand.Parameters.Add(parameter);
                adapterAfspraak.RowUpdated += Adapter_RowUpdated;


                adapterAfspraak.Fill(dataset, "Afspraak");

                dataset.Tables["Afspraak"].Columns["id"].AutoIncrement = true;
                dataset.Tables["Afspraak"].Columns["id"].AutoIncrementSeed = -1;
                dataset.Tables["Afspraak"].Columns["id"].AutoIncrementStep = -1;


            }


        }

        public void InsertKalender(IKalender kalender)
        {
            row = dataset.Tables["Kalender"].NewRow();
            row["Naam"] = kalender.Naam;
            dataset.Tables["Kalender"].Rows.Add(row);

            adapterKalender.Update(dataset, "Kalender");
        }

        public void InsertAspraak(Afspraak afspraak)
        {
            row = dataset.Tables["Afspraak"].NewRow();
            row["startTime"] = afspraak.StartTime;
            row["endTime"] = afspraak.StartTime;
            row["subject"] = afspraak.Subject;
            row["beschrijving"] = afspraak.Beschrijving;
            
            dataset.Tables["Afspraak"].Rows.Add(row);

            adapterKalender.Update(dataset, "Afspraak");
        }



        private static void Adapter_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                e.Row["id"] = e.Command.Parameters["@newid"].Value;
            }
        }




        public void conClose()
        {
            Con.Close();//close the connection
        }


    }
}
