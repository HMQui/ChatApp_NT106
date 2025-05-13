using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatApp.Common.DAO
{
    public class DataProvider
    {
        private static DataProvider instance;
        public static DataProvider Instance
        {
            get { if (instance == null) instance = new DataProvider(); return instance; }
            private set { instance = value; }
        }
        private DataProvider() { }


        private string connectionString = "Data Source=HUYNHQUI\\SQLEXPRESS;Initial Catalog=CHAT_APP;Integrated Security=True;Trust Server Certificate=True";

        public DataTable ExcuteQuery(string query, object[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@param{i}", parameters[i]);
                    }
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }

            return dataTable;
        }

        public int ExcuteNonQuery(string query, object[] parameters = null)
        {
            int data = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@param{i}", parameters[i]);
                    }
                }

                data = command.ExecuteNonQuery();
            }

            return data;
        }

        public object ExcuteScalar(string query, object[] parameters = null)
        {
            object result = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@param{i}", parameters[i]);
                    }
                }

                result = command.ExecuteScalar();
            }

            return result;
        }

    }
}
