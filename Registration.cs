using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AlbazKursachC_
{
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Autentification autentificationForm = new Autentification();
            this.Hide();
            autentificationForm.ShowDialog();        
        }
        private void Registration_Load(object sender, EventArgs e)
        {

        }
        private async void button1_Click(object sender, EventArgs e)
        {
            string login = logintxt.Text;
            string password = passwordtxt.Text;
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.");
                return; 
            }
            string insertQuery = $"INSERT INTO users (name, password, status) VALUES ('{login}', '{password}', 'common')";
            await MySQL.QueryAsync(insertQuery);
            MessageBox.Show("Аккаунт успешно создан.");
        }

        
    }
}
