using System.Windows;

namespace Exallon.ConfigurationManager
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Login { get { return _textBoxLogin.Text; } }
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get { return _textBoxPassword.Text; } }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
