using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpawmetDatabase.Model;
using SpawmetDatabaseWPF.CommonWindows;

namespace SpawmetDatabaseWPF.Windows
{
    /// <summary>
    /// Interaction logic for SendConfirmationWindow.xaml
    /// </summary>
    public partial class SendConfirmationWindow : Window
    {
        public event EventHandler WorkStarted;
        public event EventHandler WorkCompleted;

        private Order _order;

        private string _subject;
        private string _body;

        public SendConfirmationWindow(Order order)
        {
            InitializeComponent();

            _order = order;

            ReceiverTextBlock.Text = _order.Client.Email;

            _subject = "Potwierdzenie zamówienia (" + _order.Id + ") | Confirmation of an order (" + _order.Id + ")";

            //var machineName = char.ToUpper(_order.Machine.Name[0]) + order.Machine.Name.Substring(1);
            //var clientAddress = order.Client.Address != "" ? order.Client.Address : "";

            // html
            GenerateHtmlBody();
            
            PreviewWebBrowser.NavigateToString(_body);

            // plain text
            //_body = "";
            //_body += "Szanowny kliencie,\nuprzejmie informujemy, że Twoje zamówienie (id: " + order.Id +
            //         ") zostało przyjęte do realizacji.\n";
            //_body += "\nSzczegóły:\n";
            //_body += " - zamówienie: " + machineName + "\n";
            //_body += " - identyfikator: " + order.Id + "\n";
            //_body += " - data przyjęcia: " + order.StartDate.Value.ToShortDateString() + "\n";
            //_body += " - przewidywana data wysyłki: " + order.SendDate.Value.ToShortDateString() + "\n";
            //_body += " - zamawiający: " + order.Client.Name + "\n";
            //if (clientAddress != "")
            //{
            //    _body += " - adres: " + clientAddress + "\n";
            //}
            //_body += "\nZ poważaniem, SPAW-MET\n";

            //_body += "\n--------------------------------\n\n";

            //_body += "Dear customer,\nwe would like to inform you that your order (id: " + order.Id +
            //         ") was received and accepted.\n";
            //_body += "\nDetails:\n";
            //_body += " - order: " + machineName + "\n";
            //_body += " - identifier: " + _order.Id + "\n";
            //_body += " - date of acceptance: " + order.StartDate.Value.ToShortDateString() + "\n";
            //_body += " - predicted date of shipping: " + order.SendDate.Value.ToShortDateString() + "\n"; // completion
            //_body += " - client: " + order.Client.Name + "\n";
            //if (clientAddress != "")
            //{
            //    _body += " - address: " + clientAddress + "\n";
            //}
            //_body += "\nRegards, SPAW-MET\n";

            //PreviewTextBox.Text = _body;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SendingProgressBar.IsIndeterminate = true;
            IsEnabled = false;
            await Task.Run(() =>
            {
                var fromAddress = new MailAddress("ninjah187@gmail.com");
                var toAddress = new MailAddress("karolhnz@gmail.com");
                throw new Exception("ukryj hasło");
                var fromPassword = "";

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
                message.Subject = _subject;
                message.Body = _body;
                message.IsBodyHtml = true;

                smtp.Send(message);

                smtp.Dispose();
                message.Dispose();
            });
            SendingProgressBar.IsIndeterminate = false;

            Close();

            OnWorkCompleted();
        }

        private void GenerateHtmlBody()
        {
            _body = "";
            _body += "<html>";
                _body += "<head>";
                    _body += "<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">";
                    _body += "<style type=\"text/css\">";
                        _body += "body { font: 1em arial, sans-serif; }";
                        _body += "p { margin: 5px; }";
                    _body += "</style>";
                _body += "</head>";
                _body += "<body style=\"width: 100%\">";
                    _body += "<p style=\"font-size: 18px\">Szanowny kliencie,</p>";
                    _body += "<p>uprzejmie informujemy, że Twoje zamówienie (id: " + _order.Id +
                             ") zostało przyjęte do realizacji.</p>";
                    _body += "<p>Szczegóły:</p>";
                    _body += "<table style=\"width: 80%\" align=\"center\">";
                        _body += "<tr>" +
                                 "<td>zamówienie:</td>" +
                                 "<td>" + _order.Machine.Name + "</td>" +
                                 "</tr>";
                        _body += "<tr>" +
                                 "<td>identyfikator:</td>" +
                                 "<td>" + _order.Id + "</td>" +
                                 "</tr>";
                        _body += "<tr>" +
                                 "<td>data przyjęcia:</td>" +
                                 "<td>" + _order.StartDate.Value.ToShortDateString() + "</td>" +
                                 "</tr>";
                        _body += "<tr>" +
                                 "<td>przewidywana data wysyłki:</td>" +
                                 "<td>" + _order.SendDate.Value.ToShortDateString() + "</td>" +
                                 "</tr>";
                        _body += "<tr>" +
                                 "<td>zamawiający:</td>" +
                                 "<td>" + _order.Client.Name + "</td>" +
                                 "</tr>";
                        if (_order.Client.Address != "")
                        {
                            _body += "<tr>" +
                                 "<td>adres:</td>" +
                                 "<td>" + _order.Client.Address + "</td>" +
                                 "</tr>";
                        }
                        if (_order.InPrice != 0)
                        {
                            _body += "<tr>" +
                                     "<td>do zapłaty:</td>" +
                                     "<td>" + _order.InPrice + "</td>" +
                                     "</tr>";
                        }
                    _body += "</table>";
                    _body += "<p style=\"font-size: 18px\">Z poważaniem, SPAW-MET</p>";

                    // ---------- ENG VERSION ------------------
                    _body += "<br /><br />";

                    _body += "<p style=\"font-size: 18px\">Dear customer,</p>";
                    _body += "<p>we would like to inform you that your order (id: " + _order.Id +
                             ") was received and accepted.</p>";
                    _body += "<p>Details:</p>";
                    _body += "<table style=\"width: 80%\" align=\"center\">";
                    _body += "<tr>" +
                             "<td>order:</td>" +
                             "<td>" + _order.Machine.Name + "</td>" +
                             "</tr>";
                    _body += "<tr>" +
                             "<td>identifier:</td>" +
                             "<td>" + _order.Id + "</td>" +
                             "</tr>";
                    _body += "<tr>" +
                             "<td>date of acceptance:</td>" +
                             "<td>" + _order.StartDate.Value.ToShortDateString() + "</td>" +
                             "</tr>";
                    _body += "<tr>" +
                             "<td>predicted date of shipping:</td>" +
                             "<td>" + _order.SendDate.Value.ToShortDateString() + "</td>" +
                             "</tr>";
                    _body += "<tr>" +
                             "<td>client:</td>" +
                             "<td>" + _order.Client.Name + "</td>" +
                             "</tr>";
                    if (_order.Client.Address != "")
                    {
                        _body += "<tr>" +
                             "<td>address:</td>" +
                             "<td>" + _order.Client.Address + "</td>" +
                             "</tr>";
                    }
                    if (_order.InPrice != 0)
                    {
                        _body += "<tr>" +
                                 "<td>payment:</td>" +
                                 "<td>" + _order.InPrice + "</td>" +
                                 "</tr>";
                    }
                    _body += "</table>";
                    _body += "<p style=\"font-size: 18px\">Regards, SPAW-MET</p>";

                _body += "</body>";
            _body += "</html>";
        }

        private void OnWorkStarted()
        {
            if (WorkStarted != null)
            {
                WorkStarted(this, EventArgs.Empty);
            }
        }

        private void OnWorkCompleted()
        {
            if (WorkCompleted != null)
            {
                WorkCompleted(this, EventArgs.Empty);
            }
        }

        // tunneling and bubbling events - http://www.codeproject.com/Articles/464926/To-bubble-or-tunnel-basic-WPF-events
        private void PreviewWebBrowser_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Right)
            //{
            //    e.Handled = true;
            //}
        }
    }
}
