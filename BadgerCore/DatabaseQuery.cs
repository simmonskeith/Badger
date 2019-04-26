using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Collections;
using System.Text;

namespace Badger.Core
{
    public interface IDatabaseQueryService
    {
        string Username { get; set; }
        string Password { get; set; }
        string Server { get; set; }
        int UpdateDatabase(String cmdString);
        List<Dictionary<String, String>> QueryDatabase(String queryString, Dictionary<string, object> parameters);
    }

    /// <summary>
    /// A class to query/update databases
    /// </summary>
    public class DatabaseQueryService : IDatabaseQueryService
    {
        public string username;
        public string password;
        public string server;

        public DatabaseQueryService() { }
        public DatabaseQueryService(string server) { this.server = server; }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        /// <summary>
        /// Queries the database
        /// </summary>
        /// <param name="queryString">The database query string</param>
        public List<Dictionary<String, String>> QueryDatabase(String queryString, Dictionary<string, object> parameters = null)
        {
            List<Dictionary<String, String>> dbRows = new List<Dictionary<String, string>>();
            string connectionString;


            if (String.IsNullOrEmpty(username))
            {
                connectionString = String.Format("server={0};Trusted_Connection=yes;connection timeout=30", server);
            }
            else
            {
                connectionString = String.Format(
                "user id={0};password={1};server={2};Trusted_Connection=no;connection timeout=30", username, password, server);
            }
            SqlConnection myConnection = new SqlConnection(connectionString);
            try
            {
                myConnection.Open();
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand(queryString, myConnection);

                if (parameters != null)
                {
                    foreach (var i in parameters)
                    {
                        myCommand.Parameters.AddWithValue(i.Key, i.Value);
                    }
                }

                myReader = myCommand.ExecuteReader();
                if (myReader.HasRows)
                {
                    ArrayList tableColumns = new ArrayList();
                    for (int i = 0; i < myReader.FieldCount; i++)
                    {
                        tableColumns.Add(myReader.GetName(i));
                    }
                    while (myReader.Read())
                    {
                        Dictionary<String, String> tempRow = new Dictionary<String, String>();
                        foreach (String column in tableColumns)
                        {
                            var x = myReader.GetDataTypeName(tableColumns.IndexOf(column));
                            if (x == "varbinary")
                            {
                                if (myReader[column] == null)
                                {
                                    tempRow[column] = "";
                                }
                                else
                                {
                                    tempRow[column] = Convert.ToBase64String((byte[])myReader[column]);
                                }
                            }
                            else
                            {
                                tempRow[column] = myReader[column].ToString();
                            }

                        }
                        dbRows.Add(tempRow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (myConnection.State == System.Data.ConnectionState.Open)
                myConnection.Close();
            return dbRows;
        }

        /// <summary>
        /// Updates the database
        /// </summary>
        /// <param name="cmdString">The sql command string</param>
        /// <returns>number of rows updated</returns>
        public int UpdateDatabase(String cmdString)
        {
            int rowsAffected = 0;
            String connectionString = String.Format(
                "user id={0};password={1};server={2};Trusted_Connection=yes;connection timeout=30", username, password, server);
            SqlConnection myConnection = new SqlConnection(connectionString);
            try
            {
                myConnection.Open();
                SqlCommand myCommand = new SqlCommand(cmdString, myConnection);
                rowsAffected = myCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (myConnection.State == System.Data.ConnectionState.Open)
                myConnection.Close();

            return rowsAffected;
        }

        protected Dictionary<String, String> GetFirstReturnedRecord(String query)
        {
            Dictionary<String, String> record = new Dictionary<String, String>();
            List<Dictionary<String, String>> records = QueryDatabase(query, null);
            if (records.Count > 0)
                record = records[0];
            return record;
        }


    }
}

