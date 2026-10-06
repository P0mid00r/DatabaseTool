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
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public string ConnectionString { get; private set; }

        public DataGridView DataGridView { get; set; }

        public DatabaseManager(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DatabaseManager(string server, string database, string login, string password)
            : this($"Server={server};Database={database};User Id={login};Password={password};Trusted_Connection=True;")
        { }

        public DatabaseManager(string server, string database)
            : this($"Server={server};Database={database};Trusted_Connection=True;")
        { }

        /// <summary>
        /// Наличие подключения к базе данных
        /// </summary>
        public bool IsConnected
        {
            get
            {
                using (var connection = new SqlConnection(ConnectionString))
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

        /// <summary>
        /// Отправка запроса в базу данных
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns>Возвращает количество затронутых строк</returns>
        public async Task<int> SendCommand(string commandText)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(commandText, connection))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Отправка запроса в базу данных
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns>Получение таблицы по запросу</returns>
        public async Task<DataSet> SendQuery(string commandText)
        {
            using (var connection = new SqlConnection(ConnectionString))
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

        /// <summary>
        /// Получение списка таблиц подключенной базы данных
        /// </summary>
        /// <returns>Список названий таблиц</returns>
        public async Task<List<string>> GetNameTables()
        {
            //var dataSet = await SendQuery("SELECT table_name FROM information_schema.tables");
            var dataSet = await SendQuery("SELECT t.name AS table_name FROM sys.tables t INNER JOIN sys.objects o ON t.object_id = o.object_id WHERE o.is_ms_shipped = 0 AND t.is_ms_shipped = 0 AND t.type = 'U' AND t.name NOT LIKE 'sys%' AND t.name NOT LIKE 'MS%' AND t.name NOT LIKE 'syncobj_%' AND SCHEMA_NAME(t.schema_id) NOT IN ('sys', 'INFORMATION_SCHEMA', 'db_owner') ORDER BY t.name;");
            var tableNames = new List<string>();

            foreach (DataRow tableName in dataSet.Tables[0].Rows)
            {
                tableNames.Add(tableName.ItemArray[0].ToString());
            }

            return tableNames;
        }

        /// <summary>
        /// Вставка новой строки в таблицу
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="items"></param>
        /// <returns>Количество затронутых строк</returns>
        public async Task<int> InsertItem(string nameTable, string[] nameColumns, string[] values)
        {
            return await SendCommand($"INSERT INTO {nameTable} ({string.Join(", ", nameColumns)}) VALUES ({string.Join(", ", values)});");
        }

        /// <summary>
        /// Вставка новой строки в таблицу
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="items"></param>
        /// <returns>Количество затронутых строк</returns>
        public async Task<int> InsertItem(string nameTable, Dictionary<string, string> items)
        {
            return await SendCommand($"INSERT INTO {nameTable} ({string.Join(", ", items.Keys)}) VALUES ({string.Join(", ", items.Values)});");
        }

        /// <summary>
        /// Обновление данных в таблице
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="id"></param>
        /// <param name="updatedValues"></param>
        /// <returns>Количество затронутых строк</returns>
        public async Task<int> UpdateItem(string nameTable, int id, string[] updatedValues)
        {
            return await SendCommand($"UPDATE {nameTable} SET {string.Join(", ", updatedValues)} WHERE id={id};");
        }

        /// <summary>
        /// Удаление строки из таблицы
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="id"></param>
        /// <returns>Количество затронутых строк</returns>
        public async Task<int> DeleteItem(string nameTable, int id)
        {
            return await SendCommand($"DELETE FROM {nameTable} WHERE id = {id};");
        }

        /// <summary>
        /// Получение таблицы по названию
        /// </summary>
        /// <param name="nameTable"></param>
        /// <returns>Таблица</returns>
        public async Task<DataTable> GetTable(string nameTable)
        {
            var nameColumns = await GetNameColumns(nameTable);

            if (nameColumns.Select(x => x).Where(x => x.StartsWith("[id")).Count() > 1)
                return await GetJoinTable(nameTable);
            else
                return (await SendQuery($"SELECT * FROM {nameTable};")).Tables[0];
        }


        /*
         * SELECT pr.id, pt.name AS productType, mt.name AS materialType, pr.nameProduct, pr.articul, pr.minAmount, pr.width
         * FROM products pr
         * JOIN productType pt ON pr.idProductType = pt.id 
         * JOIN materialType mt ON pr.idMaterialType = mt.id
         */

        /// <summary>
        /// Получение таблицы со связями по названию
        /// </summary>
        /// <param name="nameTable"></param>
        /// <returns>Таблица</returns>
        public async Task<DataTable> GetJoinTable(string nameTable)
        {
            var nameColumns = await GetNameColumns(nameTable);
            var linkedColumns = new List<string>();
            var linkedTables = new List<string>();
            var queryColumns = new List<string>();
            var sb = new StringBuilder();

            foreach (var column in nameColumns)
            {
                if (column.StartsWith("[id"))
                {
                    if (column.Equals("[id]"))
                    {
                        queryColumns.Add($"{nameTable}.{column}");
                    }
                    else
                    {
                        var trimeName = column.Remove(1, 2);
                        var linkedTable = "[" + trimeName[1].ToString().ToLower() + trimeName.Remove(0, 2);
                        linkedColumns.Add(column);
                        linkedTables.Add(linkedTable);
                        queryColumns.Add($"{linkedTable}.name AS {linkedTable}");
                    }
                } 
                else
                {
                    queryColumns.Add($"{nameTable}.{column}");
                }
            }

            sb.AppendLine($"SELECT {string.Join(", ", queryColumns)}");
            sb.AppendLine($"FROM {nameTable} {nameTable}");

            for(int i = 0; i < linkedTables.Count; i++)
            {
                sb.AppendLine($"JOIN {linkedTables[i]} {linkedTables[i]} ON {nameTable}.{linkedColumns[i]} = {linkedTables[i]}.id");
            }

            return (await SendQuery(sb.ToString())).Tables[0];
        }

        /// <summary>
        /// Получение списка столбоц таблицы
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Список названий столбцов</returns>
        private async Task<string[]> GetNameColumns(string tableName)
        {
            var nameColumns = new List<string>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
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

        /// <summary>
        /// Показ окна добавления нового элемента в таблицу
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Успешность добавления</returns>
        public async Task<bool> ShowAddItemForm(string tableName)
        {
            var nameColumns = await GetNameColumns(tableName);
            var window = new AddWindow(tableName, nameColumns);

            bool isSuccess = window.ShowDialog() ?? false;

            if (isSuccess == true)
            {
                var result = window.Result;

                await InsertItem(tableName, result);
                await UpdateDataGridView(tableName);
            }

            return isSuccess;
        }

        /// <summary>
        /// Показ окна редатирования элемента в таблице
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ShowEditItemForm(string tableName, int id)
        {
            var dataSet = await SendQuery($"SELECT * FROM {tableName} WHERE id = {id};");
            
            var window = new EditWindow(tableName, dataSet);

            bool isSuccess = window.ShowDialog() ?? false;

            if (isSuccess == true)
            {
                var result = window.Result;
                var updatedValues = new List<string>();

                for (int i = 0; i < result.Tables[0].Columns.Count; i++)
                {
                    var columnName = result.Tables[0].Columns[i].ColumnName;
                    var value = result.Tables[0].Rows[0][i].ToString();

                    if (columnName.ToLower() == "[id]") continue;

                    updatedValues.Add($"{columnName} = {value}");
                }

                await UpdateItem(tableName, id, updatedValues.ToArray());
                await UpdateDataGridView(tableName);
            }

            return isSuccess;
        }

        public async Task UpdateDataGridView(string tableName)
        {
            if (DataGridView != null)
                DataGridView.DataSource = await GetTable(tableName);
        }
    }
}
