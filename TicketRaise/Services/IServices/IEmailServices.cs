using TicketRaise.Model;

namespace TicketRaise.Repository.Interface
{
    public interface IEmailServices
    {
        Task<List<Ticket>> SendEmailAdmin(List<Ticket> ticket);
    }
}
