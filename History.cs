using MySqlConnector;
using System;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AlbazKursachC_
{
    public partial class History : Form
    {
        private int userId;
        private MySqlConnection connection;

        public History(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            string connectionString = "Server=localhost;User=root;Password=;Database=gamecoin;SslMode=None";
            connection = new MySqlConnection(connectionString);
        }

        private async void History_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadHistory(); // Загрузка истории заказов при загрузке формы
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private async Task LoadHistory()
        {
            string selectQuery = $"SELECT * FROM orders WHERE user_id = {userId} AND status = 'выполнен'";
            DataTable historyTable = await MySQL.QueryRead(selectQuery);
            if (historyTable != null)
            {
                dataGridViewHistory.DataSource = historyTable;
            }
            else
            {
                MessageBox.Show("Не удалось загрузить историю заказов.");
            }
        }

        private void buttonBackToMain_Click(object sender, EventArgs e)
        {
            MainPage mainPageForm = new MainPage(userId);
            mainPageForm.Show();
            this.Hide();
        }
    }
}
