// File: POS.Forms/LoginForm.cs
using System;
using System.Windows.Forms;
using POS.Controllers; // Assuming your user authentication logic is in a UserController
using POS.Models;

namespace POS.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthController _userController; // Inject UserController
        public User AuthenticatedUser { get; private set; }

        // Inject UserController (which in turn gets POSDbContext)
        public LoginForm(AuthController userController)
        {
            _userController = userController;
            InitializeManualComponents(); // If you build UI in code
        }

        private void InitializeManualComponents()
        {
            this.Text = "Login";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            // Add your UI elements like username/password textboxes and login button
            // For example:
            var lblUsername = new Label { Text = "Username:", Location = new Point(20, 20), AutoSize = true };
            var txtUsername = new TextBox { Location = new Point(100, 20), Width = 150 };
            var lblPassword = new Label { Text = "Password:", Location = new Point(20, 50), AutoSize = true };
            var txtPassword = new TextBox { Location = new Point(100, 50), Width = 150, PasswordChar = '*' };
            var btnLogin = new Button { Text = "Login", Location = new Point(100, 90), Width = 80, Height = 30 };
            btnLogin.Click += async (s, e) => await LoginButton_Click(txtUsername.Text, txtPassword.Text);

            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private async Task LoginButton_Click(string username, string password)
        {
            try
            {
                var user = await _userController.LoginAsync(username, password); // Assume this method exists
                if (user != null)
                {
                    AuthenticatedUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}