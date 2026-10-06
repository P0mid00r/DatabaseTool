using DatabaseTool;
using DatabaseTool.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleTestProjectWpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DatabaseManager _db = new DatabaseManager("ServerName", "DatabaseName");

        public MainWindow()
        {
            InitializeComponent();
            UpdateData();
        }

        private async void UpdateData()
        {
            _dataGrid.ItemsSource = (await _db.GetTable("Table_1")).DefaultView;
        }

        private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = await _db.ShowAddItemForm("Table_1");

            MessageBox.Show($"Успех операции: {isSuccess}");
            UpdateData();
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_dataGrid.SelectedIndex != -1)
            {
                int id = Convert.ToInt32(((DataRowView)_dataGrid.SelectedItem)[0]);

                await _db.DeleteItem("Table_1", id);
                MessageBox.Show("Вы успешно удалили строку!");
                UpdateData();
            }
        }
    }
}
