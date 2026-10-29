using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public class KeywordStore
    {
        private string connectionString;

        public KeywordStore()
        {
            string path = Path.Combine(Application.StartupPath, "keywords.db");
            connectionString = "Data Source=" + path + ";Version=3;";
        }

        // megnyitja a kapcsolatot, es letrehozza a tablat, ha meg nincs
        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection con = new SQLiteConnection(connectionString);
            con.Open();

            string sql = "CREATE TABLE IF NOT EXISTS Keywords (" +
                         "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                         "Word TEXT NOT NULL UNIQUE)";
            using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
            return con;
        }

        public List<string> GetAll()
        {
            List<string> list = new List<string>();
            using (SQLiteConnection con = OpenConnection())
            {
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT Word FROM Keywords ORDER BY Word", con))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return list;
        }

        public void Insert(string word)
        {
            using (SQLiteConnection con = OpenConnection())
            {
                using (SQLiteCommand cmd = new SQLiteCommand("INSERT OR IGNORE INTO Keywords (Word) VALUES (@w)", con))
                {
                    cmd.Parameters.AddWithValue("@w", word);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(string word)
        {
            using (SQLiteConnection con = OpenConnection())
            {
                using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Keywords WHERE Word = @w", con))
                {
                    cmd.Parameters.AddWithValue("@w", word);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}