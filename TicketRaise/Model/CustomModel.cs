namespace TicketRaise.Model
{
    public class TicketNotification
    {
        public required string UserId { get; set; }
        public required string Priority { get; set; }
        public required string Module { get; set; }
        public required string Title { get; set; }
        public required string OrderId { get; set; }
        public required string Description { get; set; }
    }

    public class TicketResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class Ticket
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Priority { get; set; }
        public string Module { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
