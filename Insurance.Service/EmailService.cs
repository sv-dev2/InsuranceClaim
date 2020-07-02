using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using InsuranceClaim.Models;
using System.IO;

namespace Insurance.Service
{
    public class EmailService
    {

        public void SendAttachedEmail(string pTo, string pCc, string pBcc, string pSubject, string pBody, List<AttachmentModel> pAttachments)
        {
            try
            {
               // Debug.WriteLine("*********Portnumber*************");
                var portNumber = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SendEmailPortNo"]);
                var enableSSL = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["SendEmailEnableSSL"]);
                var smtpAddress = Convert.ToString(ConfigurationManager.AppSettings["SendEmailSMTP"]);

              //  Debug.WriteLine("*************from*********");
                var FromMailAddress = System.Configuration.ConfigurationManager.AppSettings["SendEmailFrom"].ToString();
                var password = System.Configuration.ConfigurationManager.AppSettings["SendEmailFromPassword"].ToString();

                //SmtpClient _client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"]);
                var client = new SmtpClient(smtpAddress, portNumber) //Port 8025, 587 and 25 can also be used.
                {
                    Credentials = new NetworkCredential(FromMailAddress, password),
                };
                //Debug.WriteLine("*************Network*********");
                client.UseDefaultCredentials = false;
                MailMessage _mailMessage = new MailMessage();
                _mailMessage.To.Add(new MailAddress(pTo));
                _mailMessage.From = new MailAddress(FromMailAddress, "GeneInsure");
                _mailMessage.Subject = pSubject;
                _mailMessage.IsBodyHtml = true;




                if (pAttachments != null && pAttachments.Count > 0)
                {
                   // Debug.WriteLine("*************attachments*********");

                    if (pAttachments[0] != null)
                    {
                        foreach (var item in pAttachments)
                        {
                        //    Debug.WriteLine("*************for each*********");
                            try
                            {
                                _mailMessage.Attachments.Add(new System.Net.Mail.Attachment(item.Attachment, item.Name));
                            }
                            catch (Exception ex)
                            {
                              //  Debug.WriteLine(ex);
                            }
                        }

                    }
                }


                AlternateView plainView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/plain");
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/html");
                _mailMessage.AlternateViews.Add(plainView);
                _mailMessage.AlternateViews.Add(htmlView);
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                   // Debug.WriteLine("*************smtp*********");

                    smtp.Credentials = new NetworkCredential(FromMailAddress, password);
                    smtp.EnableSsl = enableSSL;
                    try
                    {
                        smtp.Send(_mailMessage);
                       // Debug.WriteLine("*********************");
                      //  Debug.WriteLine("**************Email Sent*************");
                    }
                    catch (Exception ex)
                    {
                       // Debug.WriteLine("*********************");
                        WriteLog(ex.ToString());
                      //  Debug.WriteLine("*********************");

                    }
                }
                //populateMailAddresses(pTo, _message.To);

                //if (pCc != null && pCc != "")
                //    populateMailAddresses(pCc, _message.CC);
                //if (pBcc != null && pBcc != "")
                //    populateMailAddresses(pBcc, _message.Bcc);
                //_message.Body = pBody;
                //_message.BodyEncoding = System.Text.Encoding.UTF8;
                //_message.Subject = pSubject;
                //_message.SubjectEncoding = System.Text.Encoding.UTF8;
                //_message.IsBodyHtml = pIsHTML;
                //if (pAttachments != null)
                //{
                //    foreach (string _str in pAttachments)
                //        _message.Attachments.Add(new Attachment(_str));
                //}
                //client.Send(_message);
                //_message.Dispose();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }





        public void SendEmail(string pTo, string pCc, string pBcc, string pSubject, string pBody, List<string> pAttachments)
        {
            try
            {
                var portNumber = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SendEmailPortNo"]);
                var enableSSL = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["SendEmailEnableSSL"]);
                var smtpAddress = Convert.ToString(ConfigurationManager.AppSettings["SendEmailSMTP"]);

                var FromMailAddress = System.Configuration.ConfigurationManager.AppSettings["SendEmailFrom"].ToString();
                var password = System.Configuration.ConfigurationManager.AppSettings["SendEmailFromPassword"].ToString();

                //SmtpClient _client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"]);
                var client = new SmtpClient(smtpAddress, portNumber) //Port 8025, 587 and 25 can also be used.
                {
                    Credentials = new NetworkCredential(FromMailAddress, password),
                };
                client.UseDefaultCredentials = false;
                MailMessage _mailMessage = new MailMessage();
                _mailMessage.To.Add(new MailAddress(pTo));
                _mailMessage.From = new MailAddress(FromMailAddress, "GeneInsure");
                _mailMessage.Subject = pSubject;
                _mailMessage.IsBodyHtml = true;

                if (pAttachments != null && pAttachments.Count > 0)
                {
                    if (pAttachments[0] != "")
                    {
                        foreach (var item in pAttachments)
                        {
                            System.Net.Mail.Attachment attachment;
                            attachment = new System.Net.Mail.Attachment(System.Web.HttpContext.Current.Server.MapPath(item.ToString()));
                            _mailMessage.Attachments.Add(attachment);
                        }
                    }
                }

                AlternateView plainView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/plain");
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(pBody, null, "text/html");
                _mailMessage.AlternateViews.Add(plainView);
                _mailMessage.AlternateViews.Add(htmlView);
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(FromMailAddress, password);
                    smtp.EnableSsl = enableSSL;
                    try
                    {
                        smtp.Send(_mailMessage);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message);
                    }
                }
                //populateMailAddresses(pTo, _message.To);

                //if (pCc != null && pCc != "")
                //    populateMailAddresses(pCc, _message.CC);
                //if (pBcc != null && pBcc != "")
                //    populateMailAddresses(pBcc, _message.Bcc);
                //_message.Body = pBody;
                //_message.BodyEncoding = System.Text.Encoding.UTF8;
                //_message.Subject = pSubject;
                //_message.SubjectEncoding = System.Text.Encoding.UTF8;
                //_message.IsBodyHtml = pIsHTML;
                //if (pAttachments != null)
                //{
                //    foreach (string _str in pAttachments)
                //        _message.Attachments.Add(new Attachment(_str));
                //}
                //client.Send(_message);
                //_message.Dispose();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }                   


        public void WriteLog(string error)
        {
            string message = string.Format("Error Time: {0}", DateTime.Now);
            message += error;
            message += "-----------------------------------------------------------";

            message += Environment.NewLine;
            string path = System.Web.HttpContext.Current.Server.MapPath("~/LogFile.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        private void populateMailAddresses(string pAddresses, MailAddressCollection pObj)
        {
            if (pAddresses != "")
            {
                string[] _addresses = pAddresses.Split(new char[] { ';' });
                foreach (string _addr in _addresses)
                {
                    pObj.Add(_addr);
                }
            }

        }

        //public Int32 SendPaymentConfirmationEmail(string body, string subject, string to, string cc, bool isbodyHTML = false, bool enableSSL = true)
        //{
        //    try
        //    {

        //        var fromAddress = new MailAddress("testing1.kindlebit@gmail.com", "testing1kindlebit");
        //        var toAddress = new MailAddress(to, to);
        //        string fromPassword = "testing1@kindlebit123";
        //        string _subject = subject;
        //        string _body = body;

        //        var smtp = new SmtpClient
        //        {
        //            Host = "smtp.gmail.com",
        //            Port = 587,
        //            EnableSsl = true,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        //        };

        //        using (var message = new MailMessage(fromAddress, toAddress)
        //        {
        //            Subject = _subject,
        //            Body = _body
        //        })
        //        {

        //            smtp.Send(message);
        //        }

        //        //MailMessage mail = new MailMessage();
        //        //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

        //        //mail.From = new MailAddress("testing1.kindlebit@gmail.com");
        //        //mail.To.Add(to);
        //        //mail.Subject = subject;
        //        //mail.Body = body;

        //        //SmtpServer.Port = 587;
        //        //SmtpServer.Credentials = new System.Net.NetworkCredential("testing1.kindlebit@gmail.com", "testing@kindlebit123");
        //        //SmtpServer.EnableSsl = enableSSL;

        //        //SmtpServer.Send(mail);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    return 0;
        //}

        //public Int32 SendAccountCreationEmail(string body, string subject, string to, string cc, bool isbodyHTML = false, bool enableSSL = true)
        //{
        //    try
        //    {
        //        var fromAddress = new MailAddress("testing1.kindlebit@gmail.com", "testing1kindlebit");
        //        var toAddress = new MailAddress(to, to);
        //        string fromPassword = "testing1@kindlebit123";
        //        string _subject = subject;
        //        string _body = body;

        //        var smtp = new SmtpClient
        //        {
        //            Host = "smtp.gmail.com",
        //            Port = 587,
        //            EnableSsl = true,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        //        };

        //        using (var message = new MailMessage(fromAddress, toAddress)
        //        {
        //            Subject = _subject,
        //            Body = _body
        //        })
        //        {

        //            smtp.Send(message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    return 0;
        //}

    }
}
