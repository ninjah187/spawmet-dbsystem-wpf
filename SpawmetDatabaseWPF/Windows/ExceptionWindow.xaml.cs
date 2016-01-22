using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpawmetDatabaseWPF.Windows.Common
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        private Exception _exception;
        private string _message;

        private ExceptionWindow(Exception exception)
        {
            InitializeComponent();

            _exception = exception;

            var exc = exception;
            bool firstRun = true;
            while (exc != null)
            {
                //string msg = "";
                //msg = "Exception type: " + exc.GetType().ToString() + "\n";
                //msg += "Exception message: " + exc.Message + "\n";
                //msg += "To string: " + exc.ToString() + "\n";
                //msg += "\n";

                var msg = "";
                if (firstRun == false)
                {
                    msg += "Inner exception:\n";
                }
                else
                {
                    firstRun = false;
                }
                msg += exc.ToString();

                ExceptionInfoTextBox.Text = msg + "\n";

                exc = exc.InnerException;
            }

            _message = ExceptionInfoTextBox.Text;
        }

        public static void Show(Exception exception, Window owner = null)
        {
            var win = new ExceptionWindow(exception);
            win.Owner = owner;
            win.Focus();
            win.ShowDialog();
        }

        private void FillExceptionInfo(Exception exception)
        {
            
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private async void SendRaportButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            await Task.Run(() =>
            {
                var fromAddress = new MailAddress("ninjah187@gmail.com");
                var toAddress = new MailAddress("ninjah187@gmail.com");
                var fromPassword = ConfigurationManager.AppSettings["ConfirmationMailPassword"];

                var smtp = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                var message = new MailMessage(fromAddress, toAddress);
                string msgSubject = "Exception raport " + DateTime.Now.ToString("yyyy-MM-dd HH-mm");
                string msgBody = _message;
                message.Subject = msgSubject;
                message.Body = msgBody;

                smtp.Send(message);

                smtp.Dispose();
                message.Dispose();
            });
            IsEnabled = true;

            Close();
        }
    }
}
