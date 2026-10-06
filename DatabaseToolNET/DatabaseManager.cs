using DatabaseTool.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DatabaseTool
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DatabaseManager(string server, string database, string login, string password)
        {
            _connectionString = $"Server={server};Database={database};User Id={login};Password={password};";
        }

        public DatabaseManager(string server, string database)
        {
            _connectionString = $"Server={server};Database={database};Trusted_Connection=True;";
        }

        public bool IsConnected
        {
            get
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public async Task<int> SendCommand(string commandText)
        {
            System.Windows.Forms.MessageBox.Show(commandText);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(commandText, connection))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<DataSet> SendQuery(string commandText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var adapter = new SqlDataAdapter(commandText, connection))
                {
                    var dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    return dataSet;
                }
            }
        }

        public async Task InstertItem(string nameTable, string[] nameColumns, string[] values)
        {
            await SendCommand($"INSERT INTO {nameTable} ({string.Join(", ", nameColumns)}) VALUES ({string.Join(", ", values)});");
        }

        public async Task UpdateItem(string nameTable, int id, string[] updatedValues)
        {
            await SendCommand($"UPDATE {nameTable} SET {string.Join(", ", updatedValues)} WHERE id={id};");
        }

        public async Task DeleteItem(string nameTable, int id)
        {
            await SendCommand($"DELETE FROM {nameTable} WHERE id = {id};");
        }

        public async Task<DataSet> GetTable(string nameTable)
        {
            return await SendQuery($"SELECT * FROM {nameTable};");
        }

        private async Task<string[]> GetNameColumns(string tableName)
        {
            var nameColumns = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string result = string.Empty;

                using (var command = new SqlCommand($"SELECT * FROM {tableName};", connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            nameColumns.Add($"[{columnName}]");
                        }
                    }
                }
            }

            return nameColumns.ToArray();
        }

        public async Task<bool> ShowAddItemForm(string tableName)
        {
            var nameColumns = await GetNameColumns(tableName);
            var window = new AddWindow(tableName, nameColumns);

            bool isSuccess = window.ShowDialog() ?? false;

            if (isSuccess == true)
            {
                var result = window.Result;

                await InstertItem(tableName, result.Keys.ToArray(), result.Values.ToArray());
            }

            return isSuccess;
        }

        public async Task<bool> ShowEditItemForm(string tableName, int id)
        {
            var nameColumns = await GetNameColumns(tableName);
            var dataSet = await SendQuery($"SELECT * FROM {tableName} WHERE id = {id};");
            
            var window = new EditWindow(tableName, dataSet);

            bool isSuccess = window.ShowDialog() ?? false;


            if (isSuccess == true)
            {
                var result = window.Result;

                List<string> updatedValues = new List<string>();

                for (int i = 0; i < result.Tables[0].Columns.Count; i++)
                {
                    var columnName = result.Tables[0].Columns[i].ColumnName;
                    var value = result.Tables[0].Rows[0][i].ToString();

                    if (columnName.ToLower() == "[id]") continue;

                    updatedValues.Add($"{columnName} = {value}");
                }

                await UpdateItem(tableName, id, updatedValues.ToArray());
            }

            return isSuccess;
        }
    }
}
