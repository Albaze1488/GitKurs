using MySqlConnector;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlbazKursachC_
{
    public partial class Basket : Form
    {
        private MySqlConnection connection;
        private int userId;

        public Basket(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            string connectionString = "Host=localhost;User=root;Password=;Database=gamecoin;SslMode=None";
            connection = new MySqlConnection(connectionString);
        }

        private async void Basket_Load(object sender, EventArgs e)
        {
            await LoadBasket();
        }

        private async Task LoadBasket()
        {
            try
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT idkey, game, name, price, text, quantity FROM basket";
                MySqlCommand command = new MySqlCommand(selectQuery, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable basketTable = new DataTable();
                adapter.Fill(basketTable);

                dataGridView1.DataSource = basketTable;
                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void UpdateTotalAmount()
        {
            int totalAmount = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                int price = Convert.ToInt32(row.Cells["price"].Value);
                int quantity = Convert.ToInt32(row.Cells["quantity"].Value);
                totalAmount += price;
            }

            labelTotalAmount.Text = $"Итого: {totalAmount} руб.";
        }

        private async void buttonCheckout_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Выберите одну строку для покупки.");
                    return;
                }

                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int itemId = Convert.ToInt32(selectedRow.Cells["idkey"].Value);
                int price = Convert.ToInt32(selectedRow.Cells["price"].Value);
                int quantity = Convert.ToInt32(selectedRow.Cells["quantity"].Value);
                int totalPrice = price * quantity;

                PaymentForm paymentForm = new PaymentForm();
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    int discountPercent = await GetDiscountPercentForUser(userId);
                    int discountedAmount = CalculateDiscountedAmount(totalPrice, discountPercent);

                    string reportPath = CreateReport(selectedRow, paymentForm.CardName, paymentForm.CardNumber, discountedAmount);
                    if (!string.IsNullOrEmpty(reportPath))
                    {
                        MessageBox.Show("Заказ успешно размещен. Путь к отчету: " + reportPath);
                        await AddOrderToDatabase(userId, itemId, discountedAmount, "в обработке");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось создать отчет. Пожалуйста, попробуйте еще раз.");
                    }
                }
                else
                {
                    MessageBox.Show("Оплата не была произведена. Пожалуйста, попробуйте еще раз.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке заказа: " + ex.Message);
            }
        }

        private async Task AddOrderToDatabase(int userId, int itemId, int price, string status)
        {
            try
            {
                await connection.OpenAsync();

                string insertQuery = "INSERT INTO orders (user_id, item_id, price, status) " +
                                     "VALUES (@userId, @itemId, @price, @status)";
                MySqlCommand command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@itemId", itemId);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@status", status);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении заказа в базу данных: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private async Task<int> GetDiscountPercentForUser(int userId)
        {
            try
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT percent FROM discount WHERE user_id = @userId";
                MySqlCommand command = new MySqlCommand(selectQuery, connection);
                command.Parameters.AddWithValue("@userId", userId);

                object result = await command.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return 0; // Или другое значение по вашему выбору
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении скидки пользователя: " + ex.Message);
                return 0; // Возвращаем 0 в случае ошибки
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private int CalculateDiscountedAmount(int price, int discountPercent)
        {
            if (discountPercent > 0)
            {
                int discountedAmount = price * (100 - discountPercent) / 100;
                return discountedAmount;
            }
            else
            {
                return price;
            }
        }

        private string CreateReport(DataGridViewRow itemRow, string cardName, string cardNumber, int discountedAmount)
        {
            try
            {
                string reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OrderReport.txt");

                using (StreamWriter writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("Отчет о заказе");
                    writer.WriteLine("=================");
                    writer.WriteLine($"Название карты: {cardName}");
                    writer.WriteLine($"Номер карты: {cardNumber}");
                    writer.WriteLine();

                    string gameName = itemRow.Cells["game"].Value.ToString();
                    string itemName = itemRow.Cells["name"].Value.ToString();
                    int price = Convert.ToInt32(itemRow.Cells["price"].Value);
                    int quantity = Convert.ToInt32(itemRow.Cells["quantity"].Value);
                    string description = itemRow.Cells["text"].Value.ToString();

                    writer.WriteLine($"Игра: {gameName}");
                    writer.WriteLine($"Товар: {itemName}");
                    writer.WriteLine($"Цена за единицу: {price:C}");
                    writer.WriteLine($"Количество: {quantity}");
                    writer.WriteLine($"Описание: {description}");
                    writer.WriteLine();

                    writer.WriteLine($"Итоговая сумма (с учетом скидки): {discountedAmount:C}");
                }

                return reportPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании отчета: " + ex.Message);
                return null;
            }
        }

        private async void buttonClearBasket_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите очистить корзину?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await ClearBasket();
            }
        }

        private async Task ClearBasket()
        {
            try
            {
                await connection.OpenAsync();

                string deleteQuery = "DELETE FROM basket";
                MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                await command.ExecuteNonQueryAsync();

                MessageBox.Show("Корзина очищена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при очистке корзины: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            UpdateTotalAmount();
        }

        private void back_Click(object sender, EventArgs e)
        {
            ReturnToMainpage();
        }

        private void ReturnToMainpage()
        {
            MainPage mainpage = new MainPage(userId);
            mainpage.Show();
            this.Hide();
        }
    }
}
