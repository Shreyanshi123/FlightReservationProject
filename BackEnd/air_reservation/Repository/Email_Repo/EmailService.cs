using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string sendGridApiKey = "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";

    public async Task SendEmailAsync(string recipientEmail, string subject, string body, byte[] attachmentBytes)
    {
        var client = new SendGridClient(sendGridApiKey);
        var from = new EmailAddress("shreyanshimishra7689@gmail.com", "Evalueserve");
        var to = new EmailAddress(recipientEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);


        if (attachmentBytes != null && attachmentBytes.Length > 0)
        {
            var attachment = Convert.ToBase64String(attachmentBytes);
            msg.AddAttachment("ticket.pdf", attachment,"attachment/pdf"); // ✅ Attach PDF
        }


        var response = await client.SendEmailAsync(msg);
        Console.WriteLine($"✅ Email sent with status: {response.StatusCode}");
    }
}