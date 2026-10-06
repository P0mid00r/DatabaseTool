using DatabaseTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleTestProject
{
    public partial class Form1 : Form
    {
        private readonly DatabaseManager _db = new DatabaseManager("ServerName", "DatabaseName");

        public Form1()
        {
            InitializeComponent();
        }

        protected async override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            comboBox1.Items.AddRange((await _db.GetNameTables()).ToArray());
            comboBox1.SelectedIndex = 1;

            _db.DataGridView = dataGridView1;
            await _db.UpdateDataGridView(comboBox1.SelectedItem.ToString());
        }

        private async void ButtonAdd_Click(object sender, EventArgs e)
        {
            bool isSuccess = await _db.ShowAddItemForm(comboBox1.SelectedItem.ToString());

            if (isSuccess)
            {
                MessageBox.Show($"Вы успешно добавили элемент");
            } 
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                var id = (int)dataGridView1.SelectedCells[0].Value;

                await _db.DeleteItem(comboBox1.SelectedItem.ToString(), id);
                MessageBox.Show("Вы успешно удалили строку!");
            }
        }

        private async void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                var id = (int)dataGridView1.SelectedCells[0].Value;
                await _db.ShowEditItemForm(comboBox1.SelectedItem.ToString(), id);
            }
        }

        private async void ButtonUpdate_Click(object sender, EventArgs e)
        {
            await _db.UpdateDataGridView(comboBox1.SelectedItem.ToString());
        }
    }
}
