using MySqlConnector;
using System;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AlbazKursachC_
{
    public partial class MainPage : Form
    {
        private int userId;
        private MySqlConnection connection;

        public MainPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            string connectionString = "Host=localhost;User=root;Password=;Database=gamecoin;SslMode=None";
            connection = new MySqlConnection(connectionString);
        }

        private async void MainPage_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadItems(); // Загрузка данных при загрузке формы
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data from database: " + ex.Message);
            }
        }

        private async Task LoadItems()
        {
            string selectQuery = "SELECT * FROM items";
            DataTable itemsTable = await MySQL.QueryRead(selectQuery);
            if (itemsTable != null)
            {
                dataGridView1.DataSource = itemsTable;
            }
            else
            {
                MessageBox.Show("No items found.");
            }
        }

        private async void buttonAddToBasket_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    await connection.OpenAsync();

                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        var gameName = row.Cells["game"].Value?.ToString() ?? "Unknown";
                        var itemName = row.Cells["name"].Value?.ToString() ?? "Unknown";
                        var price = row.Cells["price"].Value != null ? Convert.ToDecimal(row.Cells["price"].Value) : 0;
                        var description = row.Cells["text"].Value?.ToString() ?? "No description";
                        var itemId = Convert.ToInt32(row.Cells["idkey"].Value);

                        string selectQuery = "SELECT quantity FROM basket WHERE item_id = @itemId";
                        MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection);
                        selectCommand.Parameters.AddWithValue("@itemId", itemId);

                        var result = await selectCommand.ExecuteScalarAsync();

                        if (result != null)
                        {
                            int currentQuantity = Convert.ToInt32(result);
                            string updateQuery = "UPDATE basket SET quantity = @quantity, price = @price WHERE item_id = @itemId";
                            MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                            updateCommand.Parameters.AddWithValue("@quantity", currentQuantity + 1);
                            updateCommand.Parameters.AddWithValue("@price", price * (currentQuantity + 1));
                            updateCommand.Parameters.AddWithValue("@itemId", itemId);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                        else
                        {
                            string insertQuery = "INSERT INTO basket (game, name, price, text, item_id, quantity) " +
                                                 "VALUES (@game, @name, @price, @text, @itemId, @quantity)";

                            MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                            insertCommand.Parameters.AddWithValue("@game", gameName);
                            insertCommand.Parameters.AddWithValue("@name", itemName);
                            insertCommand.Parameters.AddWithValue("@price", price);
                            insertCommand.Parameters.AddWithValue("@text", description);
                            insertCommand.Parameters.AddWithValue("@itemId", itemId);
                            insertCommand.Parameters.AddWithValue("@quantity", 1);

                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }

                    MessageBox.Show("Selected items have been added to the basket.");
                }
                else
                {
                    MessageBox.Show("Select items to add to the basket.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding items to basket: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void buttonViewBasket_Click(object sender, EventArgs e)
        {
            Basket basketForm = new Basket(userId);
            basketForm.Show();
            this.Hide();
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            Autentification authentification = new Autentification(); // Используйте правильное имя вашей формы аутентификации
            this.Hide();
            authentification.ShowDialog();
        }

        private void buttonHistory_Click(object sender, EventArgs e)
        {
            History historyForm = new History(userId);
            historyForm.Show();
            this.Hide();
        }

        private void buttonSpravka_Click(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\user\Desktop\Справка.jpg"; // Укажите путь к вашему изображению
            Spravka spravkaForm = new Spravka(imagePath);
            spravkaForm.ShowDialog();
        }
    }
}
