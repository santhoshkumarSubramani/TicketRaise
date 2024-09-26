using System.Threading.Tasks;
using TicketRaise.Model;

namespace TicketRaise.Repository.Interface
{
    public interface IRaiseTicket
    {
        Task<TicketResponse> RaiseTicket(TicketNotification notification);
        Task<List<Ticket>> GetUnreadTicketsAsync();
        Task<bool> UpdateTicketsAsync(List<Ticket> tickets);

    }
}
