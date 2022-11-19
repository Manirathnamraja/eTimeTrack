using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using Elmah;

namespace eTimeTrack.Helpers
{
    public static class EmailHelper
    {
        public static bool SendEmail(string emailTo, string title, string body, string attachmentPath = null, List<string> cc = null, List<string> bcc = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailTo))
                    return false;

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(body))
                {
                    using (MailMessage message = new MailMessage
                    {
                        Subject = title,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        if (attachmentPath != null && File.Exists(attachmentPath))
                        {
                            message.Attachments.Add(new Attachment(attachmentPath));
                        }

                        // add CC addresses
                        if (cc != null)
                        {
                            foreach (string address in cc)
                            {
                                message.CC.Add(address);
                            }
                        }

                        // add BCC addresses
                        if (bcc != null)
                        {
                            foreach (string address in bcc)
                            {
                                message.Bcc.Add(address);
                            }
                        }

                        string[] recip = emailTo.Split(';');
                        foreach (string rec in recip)
                        {
                            try
                            {
                                message.To.Add(rec.Trim());
                            }
                            catch (FormatException e)
                            {
                                //Logger.Error(e, "The email address {0} in the list {1} is in the incorrect format!", rec, emailTo);
                            }
                        }

                        if (message.To.Count > 0)
                            using (SmtpClient mail = new SmtpClient())
                                mail.Send(message);
                    }
                    return true;
                }

                //Logger.Error("Cannot send email: subject or content is not complete");
                return false;
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return false;
            }
        }
    }
}