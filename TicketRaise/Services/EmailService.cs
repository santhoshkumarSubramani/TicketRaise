using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using TicketRaise.Model;
using TicketRaise.Repository.Interface;

namespace TicketRaise.Repository
{
    public class EmailService : IEmailServices
    {
        public IConfiguration _configuration { get; }
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Ticket>> SendEmailAdmin(List<Ticket> ticket)
        {
            var accessKey = _configuration["aws:AccessKey"];
            var secretKey = _configuration["aws:SecretKey"];
            var senderEmail = _configuration["Email:AdminEmail"];
            var recipientEmail = _configuration["Email:ReceipientEmail"]; // admin

            foreach (var ticketItem in ticket)
            {
                var subject = $"Ticket Alert: {ticketItem.Title}";
                var body = $@"<html>
                                <head></head>
                                <body>
                                    <h1>New Ticket Alert</h1>
                                    <p><strong>Ticket ID:</strong> {ticketItem.Id}</p>
                                    <p><strong>User ID:</strong> {ticketItem.UserId}</p>
                                    <p><strong>Priority:</strong> {ticketItem.Priority}</p>
                                    <p><strong>Module:</strong> {ticketItem.Module}</p>
                                    <p><strong>Title:</strong>{ticketItem.Title}</p>
                                    <p><strong>Description:</strong> {ticketItem.Description}</p>
                                </body>
                            </html>";

                using (var client = new AmazonSimpleEmailServiceClient(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1))
                {
                    var toMail = new List<string> { recipientEmail };

                    if (ticketItem.Priority == "HIGH")
                    {
                        toMail.Add(senderEmail);
                    }

                    var sendRequest = new SendEmailRequest
                    {
                        Source = senderEmail,
                        Destination = new Destination
                        {
                            ToAddresses = toMail
                        },
                        Message = new Message
                        {
                            Subject = new Content(subject),
                            Body = new Body
                            {
                                Text = new Content(body)
                            }
                        }
                    };

                    try
                    {
                        var response = await client.SendEmailAsync(sendRequest);
                        Console.WriteLine($"Email sent! Message ID: {response.MessageId}");
                        ticketItem.Status = "Read";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email: {ex.Message}");
                    }
                }
            }

            return ticket;
        }
    }


}
