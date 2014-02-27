using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data; 
using System.Data.SqlClient;
using System.IO;
using System.Xml; 
using MySql.Data.MySqlClient; 


namespace SharpMule.Automation.Framework.Essentials
{
  

    public class DataBaseTasks
    {
        Logger Log; 
        SqlConnection sqlConnection;
        MySqlConnection mySqlConnection; 

        public bool IsConnected { get; set; }
        public enum DataBaseType { MS_SQL, MY_SQL };
        public DataBaseType DBType { get; set; }
        public DataBaseTasks(bool noLogging)
        {
            InstantiateSqlObjects();
            if (!noLogging)
            {
                Log = TestInterfaceEngine.Log;
                Log.Level = Logger.Levels.DEBUG;
            }
                
        }

        public DataBaseTasks()
        {
            InstantiateSqlObjects();

            Log = TestInterfaceEngine.Log; 
            Log.Level = Logger.Levels.DEBUG; 
            
        }

        public DataBaseTasks(Logger existingLoggerObject)
        {
            InstantiateSqlObjects();
            Log = existingLoggerObject; 
        }

        private void InstantiateSqlObjects()
        {
            sqlConnection = new SqlConnection();
            mySqlConnection = new MySqlConnection(); 
        }


        public bool Connect(string connectionString)
        {

            switch (DBType)
            {
                case DataBaseType.MS_SQL:
                    sqlConnection.ConnectionString = connectionString;
                    try { sqlConnection.Open(); IsConnected = true; }
                    catch { return false; }
                    break;
                case DataBaseType.MY_SQL:
                    mySqlConnection.ConnectionString = connectionString;
                    try { mySqlConnection.Open(); IsConnected = true; }
                    catch { return false; }
                    break;
            }

            return true; 
        }

        public bool Connect(string server,string database)
        {
            string connectionString = "Data Source= " + server + ";Initial Catalog= " + database + ";Integrated Security=true";

            return Connect(connectionString); 
        }

        public bool CloseConnection()
        {
            if (sqlConnection.State == ConnectionState.Open)
                sqlConnection.Close();
            if (mySqlConnection.State == ConnectionState.Open)
                mySqlConnection.Close(); 

            return false;
        }


        

        public void SendQuery(string querystring, out string sqlresponse, bool noLogging=true)
        {
            sqlresponse = String.Empty;
            try
            {
                SqlDataAdapter msda = new SqlDataAdapter(querystring, sqlConnection);
                MySqlDataAdapter myda = new MySqlDataAdapter(querystring, mySqlConnection);


                DataTable dataTable = new DataTable();
                if (DBType == DataBaseType.MS_SQL)
                    msda.Fill(dataTable);
                else if (DBType == DataBaseType.MY_SQL)
                    myda.Fill(dataTable);

                DataSet ds = new DataSet();
                ds.Tables.Add(dataTable);

                StringWriter writer = new StringWriter();
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);

                sqlresponse = writer.ToString();

                if (!noLogging)
                    Log.LogDebug("Response:\n" + sqlresponse);
            }
            catch (EvaluateException ex)
            {
                Log.LogError("An error has occured trying to execute following query:\n" + querystring);
                Log.LogDebug(ex.Message);

            }
            
            
            
        }
     
        public void SendQuery2(string querystring, out string sqlresponse)
        {
            sqlresponse = String.Empty;
            try
            {
                SqlCommand cmd = new SqlCommand();
                MySqlCommand mycmd = new MySqlCommand(); 
                SqlDataReader reader;
                MySqlDataReader myreader;

                if (DBType == DataBaseType.MS_SQL)
                {
                    cmd.CommandText = querystring;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = sqlConnection;

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        sqlresponse = reader[0].ToString();

                    }
                }
                else if (DBType == DataBaseType.MY_SQL)
                {
                    mycmd.CommandText = querystring;
                    mycmd.CommandType = CommandType.Text;
                    mycmd.Connection = mySqlConnection;

                    myreader = mycmd.ExecuteReader();

                    while (myreader.Read())
                    {
                        sqlresponse = myreader[0].ToString();

                    }
                }
                
                
                Log.LogDebug("Response:\n" + sqlresponse);
                
                

            }
            catch (EvaluateException ex)
            {
                Log.LogError("An error has occured trying to execute following query:\n" + querystring);
                Log.LogDebug(ex.Message);

            }

        }

        public static class Static
        {
            static SqlConnection staticConnection = new SqlConnection();
            public static bool IsConnected { get; set; }

            public static bool Connect(string server, string database)
            {

                staticConnection.ConnectionString = "Data Source= " + server + ";Initial Catalog= " + database + ";Integrated Security=true";

                try
                {
                    staticConnection.Open();
                    IsConnected = true;
                }
                catch
                {
                    return false;
                }


                return true;

            }

            public static bool SendQuery(string querystring, out string sqlresponse)
            {
                sqlresponse = String.Empty;
                try
                {


                    SqlDataAdapter da = new SqlDataAdapter(querystring, staticConnection);
                    DataTable dataTable = new DataTable();
                    da.Fill(dataTable);

                    DataSet ds = new DataSet();
                    ds.Tables.Add(dataTable);

                    StringWriter writer = new StringWriter();
                    ds.WriteXml(writer, XmlWriteMode.WriteSchema);

                    sqlresponse = writer.ToString();
                }
                catch
                {
                    return false;
                }


                return true;
            }
        }
        
    }
}
