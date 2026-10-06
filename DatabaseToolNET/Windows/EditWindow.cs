using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseToolNET.Windows
{
    internal class EditWindow : Window
    {
        //public Dictionary<string, string> Result { get; private set; } = new Dictionary<string, string>();
        public DataSet Result { get; private set; }

        private readonly DataSet _currentDataSet;
        private readonly DataSet _editedDataSet;

        private List<(string nameColumn, TextBox textBox)> _values = new List<(string nameColumn, TextBox textBox)>();

        public EditWindow(string tableName, DataSet dataSet)
        {
            _currentDataSet = dataSet.Copy();
            _editedDataSet = dataSet.Copy();

            Initialize(tableName);
            LoadDesigne(dataSet);
        }

        private void Initialize(string tableName)
        {
            this.Title = $"Редактирование {tableName}";

            this.WindowStyle = WindowStyle.ToolWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;

            this.Width = 400;
            this.SizeToContent = SizeToContent.Height;
        }

        private void LoadDesigne(DataSet dataSet)
        {
            var stackPanelMain = new StackPanel
            {
                Margin = new Thickness(10)
            };

            var stackPanelItems = new StackPanel();
            var stackPanelBottom = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            stackPanelMain.Children.Add(stackPanelItems);
            stackPanelMain.Children.Add(stackPanelBottom);

            this.Content = stackPanelMain;

            foreach(DataColumn column in dataSet.Tables[0].Columns)
            {
                var columnName = column.ColumnName;

                var textBox = new TextBox 
                {
                    Text = dataSet.Tables[0].Rows[0][columnName].ToString(),
                };

                stackPanelItems.Children.Add(new TextBlock { Text = columnName.Trim('[', ']') });
                stackPanelItems.Children.Add(textBox);

                if (columnName.ToLower().Trim('[', ']') == "id")
                {
                    textBox.IsReadOnly = true;
                }

                _values.Add((columnName, textBox));
            }

            var buttonAdd = new Button { Content = "Сохранить" };
            buttonAdd.Click += delegate
            {
                foreach (var value in _values)
                {
                    string columnName = value.nameColumn;
                    string columnValue = value.textBox.Text.Trim('[', ']');

                    if (int.TryParse(columnValue, out int columnValueInt))
                    {
                        _editedDataSet.Tables[0].Rows[0][columnName] = columnValueInt.ToString();
                    }
                    else
                    {
                        _editedDataSet.Tables[0].Rows[0][columnName] = $"'{columnValue}'";
                    }

                }

                foreach (DataColumn column in _editedDataSet.Tables[0].Columns)
                {
                    column.ColumnName = $"[{column.ColumnName}]";
                }

                Result = _editedDataSet;

                DialogResult = true;
            };

            var buttonCancel = new Button { Content = "Отмена" };
            buttonCancel.Click += delegate { DialogResult = false; };

            stackPanelBottom.Children.Add(buttonAdd);
            stackPanelBottom.Children.Add(buttonCancel);
        }
    }
}
