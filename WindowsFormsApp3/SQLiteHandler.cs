using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace WindowsFormsApp3
{
    internal class SQLiteHandler
    {
        SQLiteConnection connection;
        public void ConnectToDb()
        {
            connection = new SQLiteConnection("Data Source=EventKeywords.db;Version=3;");
            connection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS Keywords (Id INTEGER PRIMARY KEY AUTOINCREMENT, Keyword TEXT)";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void DisconnectFromDb()
        {
            if(connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        public void InsertKeyword(string keyword)
        {
            if (connection == null) return;
            string sql = "INSERT INTO Keywords (Keyword) VALUES (@keyword)";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@keyword", keyword);
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetAllKeywords()
        {
            List<string> keywords = new List<string>();
            if (connection == null) return keywords;
            string sql = "SELECT Keyword FROM Keywords";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        keywords.Add(reader["Keyword"].ToString());
                    }
                }
            }
            return keywords;
        }
    }
}
