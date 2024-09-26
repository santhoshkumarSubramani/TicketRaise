using TicketRaise.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using TicketRaise.Repository.Interface;

namespace TicketRaise.Repository
{
    public class raiseTicketRepo : IRaiseTicket
    {
        public IConfiguration _configuration { get; }
        private static readonly string _tableName = "RaiseTickets";
        public raiseTicketRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TicketResponse> RaiseTicket(TicketNotification notification)
        {
            string ticketReference = GenerateTicketReference();


            bool result = await SaveDataDynamoDb(notification, ticketReference);

            return new TicketResponse
            {
                Success = true,
                Message = $"Ticket raised successfully. Ticket Reference #{ticketReference}"
            };
        }

        private string GenerateTicketReference()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }

        public async Task<bool> SaveDataDynamoDb(TicketNotification notification, string ticketReference)
        {
            var accessKey = _configuration["aws:AccessKey"];
            var secretKey = _configuration["aws:SecretKey"];

            var client = new AmazonDynamoDBClient(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);

            var item = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = ticketReference} },
                { "UserId", new AttributeValue { S = notification.UserId } },
                { "Module", new AttributeValue { S = notification.Module } },
                { "Title", new AttributeValue { S = notification.Title } },
                { "OrderId", new AttributeValue { S = notification.OrderId } },
                { "Description", new AttributeValue { S = notification.Description } },
                { "Priority", new AttributeValue { S = notification.Priority.ToUpper() } },
                { "Status" , new AttributeValue{ S = "Unread"} }
            };

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            };

            try
            {
                await client.PutItemAsync(putRequest);
                Console.WriteLine("Item inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting item: " + ex.Message);
                return false;
            }
            return true;
        }

        public async Task<List<Ticket>> GetUnreadTicketsAsync()
        {
            var accessKey = _configuration["aws:AccessKey"];
            var secretKey = _configuration["aws:SecretKey"];

            using var client = new AmazonDynamoDBClient(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
            var table = Table.LoadTable(client, _tableName);

            var tickets = new List<Ticket>();

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Status", ScanOperator.Equal, "Unread");

            // scanFilter.AddCondition("Priority", ScanOperator.Equal, "HIGH");

            ScanOperationConfig config = new ScanOperationConfig
            {
                Filter = scanFilter
            };

            var search = table.Scan(config);

            do
            {
                var documentBatch = await search.GetNextSetAsync();
                foreach (var document in documentBatch)
                {
                    tickets.Add(new Ticket
                    {
                        Id = document["Id"].AsString(),
                        UserId = document["UserId"].AsString(),
                        Priority = document["Priority"].AsString(),
                        Module = document["Module"].AsString(),
                        Title = document["Title"].AsString(),
                        Description = document["Description"].AsString(),
                        Status = document["Status"].AsString()
                    });
                }
            } while (!search.IsDone);

            return tickets;
        }


        public async Task<bool> UpdateTicketsAsync(List<Ticket> tickets)
        {
            var accessKey = _configuration["aws:AccessKey"];
            var secretKey = _configuration["aws:SecretKey"];

            var client = new AmazonDynamoDBClient(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
            var table = Table.LoadTable(client, _tableName);

            var transactItems = new List<TransactWriteItem>();

            foreach (var ticket in tickets.Where(x => x.Status.Equals("Read")).Select(x => x))
            {
                var updateItem = new Update
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = ticket.Id } }
                },
                    UpdateExpression = "SET #s = :newStatus",
                    ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#s", "Status" }
                },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":newStatus", new AttributeValue { S = "Read" } }
                }
                };

                transactItems.Add(new TransactWriteItem { Update = updateItem });
            }

            var transactWriteItemsRequest = new TransactWriteItemsRequest
            {
                TransactItems = transactItems
            };

            await client.TransactWriteItemsAsync(transactWriteItemsRequest);
            return true;
        }
    }
}
