using Appointment.Models;
using Appointment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Diagnostics;

namespace Appointment.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppointmentContext _context;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger,IConfiguration config, AppointmentContext context)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

        public IActionResult Index()
        {
            var spesialis = _context.Spesialis.Where(i => i.Status == "A").ToList();
            ViewData["DataSpesialis"] = spesialis;
            return View(spesialis);
        }

        public async Task SentEmail(string subject, string status, string from, bool show)
        {
            //send Email Here
            string FromAddress = _config["EmailSettings:SenderEmail"];
            string FromAdressTitle = _config["EmailSettings:SenderName"];
            string ToAddress = "";
            string ToAddressTitle = "";

            var bodyBuilder = new BodyBuilder();
            var mimeMessage = new MimeMessage();
            //show = false;
            ToAddress = "";
            ToAddressTitle = "";

            string Subject = subject;
            string htmlBody = "System Info:<br/>";
            htmlBody += "<style>\r\n    table {\r\n      border-collapse: collapse;\r\n      width: 30%;\r\n    }\r\n    table td, table th {\r\n      border: 1px solid black;\r\n      padding: 8px;\r\n      text-align: left;\r\n    }\r\n    table th {\r\n      background-color: #f2f2f2;\r\n    }\r\n  </style>";
            htmlBody += status + " getting new data from " + from + ".<br/><br/>";

            if (show == false)
            {
                htmlBody += "Last Try: " + DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss tt") + "<br/><br/>";
            }
            else if (show == true)
            {

            }

            htmlBody += "Here's the current data in eSelling:<br/><br/>";            
            htmlBody += "<center><small><b><i>This email is generated automatically by system.<br/>Please do not reply to this email.</i></b></small></center>";
            bodyBuilder.HtmlBody = htmlBody;
            mimeMessage.From.Add(new MailboxAddress(FromAdressTitle, FromAddress));
            mimeMessage.Subject = Subject;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            ToAddressTitle = "febrian@mpm-insurance.com";
            ToAddress = "febrian@mpm-insurance.com";
            mimeMessage.To.Add(new MailboxAddress(ToAddressTitle, ToAddress));

            //Check configuration
            var serverAddress = _config["EmailSettings:SmtpServer"];
            var emailPort = _config["EmailSettings:Port"];
            var emailUsername = _config["EmailSettings:Username"];
            var emailPass = _config["EmailSettings:Password"];

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.Connect(serverAddress, Convert.ToInt32(emailPort), false);
                    client.Authenticate(emailUsername, emailPass);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (client.IsConnected == true)
                    {
                        client.Disconnect(true);
                    }
                }
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> TestEmail()
        {
            string from = "BI";
            string subject = "[INFO] Successfully Getting New Exchange Rate from BI (eSelling)";
            string status = "Success";
            await SentEmail(subject, status, from, true);
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> PrivacyAsync()
        {
            string from = "BI";
            string subject = "[INFO] Successfully Getting New Exchange Rate from BI (eSelling)";
            string status = "Success";
            await SentEmail(subject, status, from, true);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}