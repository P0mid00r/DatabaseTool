using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DatabaseToolNET.Windows
{
    internal class AddWindow : Window
    {
        public Dictionary<string, string> Result { get; private set; } = new Dictionary<string, string>();

        private List<(string nameColumn, TextBox textBox)> _values = new List<(string nameColumn, TextBox textBox)>();

        public AddWindow(string tableName, string[] nameColumns)
        {
            Initialize(tableName);
            LoadDesigne(nameColumns);
        }

        private void Initialize(string tableName)
        {
            //this.Icon = BitmapSource.Create(1, 1, 0, 0, PixelFormats.Bgra32, null, new byte[4], 4);
            this.Title = $"Добавление нового элемента в {tableName}";

            this.WindowStyle = WindowStyle.ToolWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;

            this.Width = 400;
            this.SizeToContent = SizeToContent.Height;
        }

        private void LoadDesigne(string[] nameColumns)
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

            foreach (var columnName in nameColumns)
            {
                if (columnName.ToLower() != "[id]")
                {
                    var textBox = new TextBox();

                    stackPanelItems.Children.Add(new TextBlock { Text = columnName.Trim('[', ']') });
                    stackPanelItems.Children.Add(textBox);

                    _values.Add((columnName, textBox));
                }
            }

            var buttonAdd = new Button { Content = "Добавить" };
            buttonAdd.Click += delegate 
            { 
                foreach(var value in _values)
                {
                    string columnName = value.nameColumn;
                    string columnValue = value.textBox.Text.Trim('[', ']');

                    if (int.TryParse(columnValue, out int columnValueInt))
                    {
                        Result.Add(columnName, columnValueInt.ToString());
                    } 
                    else
                    {
                        Result.Add(columnName, $"'{columnValue}'");
                    }

                }

                DialogResult = true;
            };

            var buttonCancel = new Button { Content = "Отмена" };
            buttonCancel.Click += delegate { DialogResult = false; };

            stackPanelBottom.Children.Add(buttonAdd);
            stackPanelBottom.Children.Add(buttonCancel);
        }
    }
}
