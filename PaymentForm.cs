using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AlbazKursachC_
{
    public partial class PaymentForm : Form
    {
        public string CardName => textBoxCardName.Text;
        public string CardNumber => textBoxCardNumber.Text;
        public string ExpiryDate => textBoxExpiryDate.Text;
        public string CVV => textBoxCVV.Text;

        public PaymentForm()
        {
            InitializeComponent();
        }

        private void buttonPay_Click(object sender, EventArgs e)
        {
            if (ValidatePaymentDetails())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите все данные корректно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidatePaymentDetails()
        {
            return !string.IsNullOrWhiteSpace(CardName) &&
                   ValidateCardNumber(CardNumber) &&
                   ValidateExpiryDate(ExpiryDate) &&
                   ValidateCVV(CVV);
        }

        private bool ValidateCardNumber(string cardNumber)
        {
            return Regex.IsMatch(cardNumber, @"^\d{16}$"); // 16 цифр
        }

        private bool ValidateExpiryDate(string expiryDate)
        {
            return Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])\/?([0-9]{2})$"); // MM/YY
        }

        private bool ValidateCVV(string cvv)
        {
            return Regex.IsMatch(cvv, @"^\d{3}$"); // 3 цифры
        }
    }
}
