using System.Net;
using System.Net.Mail;
using System;
using UnityEngine;

public class EmailSender : MonoBehaviour
{
    public string senderEmail = ""; // Your email address
    public string senderPassword = "";     // Your email password
    public string smtpServer = "smtp.gmail.com";       // SMTP server for Gmail
    public int smtpPort = 587;                         // Port for Gmail
    public string recipientEmail = ""; // Recipient's email address
    public string subject = "Subject of the email";
    public string bodyText = "This is the body of the email.";

    public Texture2D textureToSend;

    private void Start()
    {
        SendEmail();
    }

    public void SendEmail()
    {
        try
        {
            // Create a mail message
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(senderEmail);
            mail.To.Add(recipientEmail);
            mail.Subject = subject;
            mail.Body = bodyText;

            // Convert Texture2D to PNG and attach it
            textureToSend = GeneralGameLibraries.TextureManipulationLibrary.DeCompress(textureToSend);
            byte[] imageBytes = textureToSend.EncodeToPNG();
            System.IO.MemoryStream imageStream = new System.IO.MemoryStream(imageBytes);
            Attachment imageAttachment = new Attachment(imageStream, "image.png", "image/png");
            mail.Attachments.Add(imageAttachment);

            // Setup the SMTP client
            SmtpClient smtpClient = new SmtpClient(smtpServer);
            smtpClient.Port = smtpPort;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true; // Enable SSL for secure connection

            // Send the email
            smtpClient.Send(mail);
            Debug.Log("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending email: " + ex.Message);
        }
    }
}
