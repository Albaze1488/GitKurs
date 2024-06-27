using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlbazKursachC_
{
    public partial class OrderAdminForm : Form
    {
        private MySqlConnection connection;

        public OrderAdminForm()
        {
            InitializeComponent();
            string connectionString = "Host=localhost;Port=3306;User=root;Password=;Database=gamecoin;SslMode=None";
            connection = new MySqlConnection(connectionString);
        }

        private async void OrderAdminForm_Load(object sender, EventArgs e)
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT idkey, user_id, item_id, status, price FROM orders";
                MySqlCommand command = new MySqlCommand(selectQuery, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable ordersTable = new DataTable();
                adapter.Fill(ordersTable);

                dataGridViewOrders.DataSource = ordersTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private async void buttonConfirmOrder_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOrders.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Выберите одну строку для подтверждения.");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewOrders.SelectedRows[0];
                int orderId = Convert.ToInt32(selectedRow.Cells["idkey"].Value);

                await connection.OpenAsync();

                string updateQuery = "UPDATE orders SET status = 'выполнен' WHERE idkey = @orderId";
                MySqlCommand command = new MySqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@orderId", orderId);

                await command.ExecuteNonQueryAsync();

                MessageBox.Show("Заказ успешно подтвержден.");

                // Удалить строку из DataGridView
                dataGridViewOrders.Rows.Remove(selectedRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подтверждении заказа: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            AdminForm adminForm = new AdminForm();
            adminForm.Show();
            this.Hide();
        }
    }
}
