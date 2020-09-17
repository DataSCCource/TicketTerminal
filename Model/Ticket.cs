using static Fahrkartenautomat.Definitions;

namespace Fahrkartenautomat.Model
{
    /// <summary>
    /// Represents a possible Ticket.
    /// </summary>
    public class Ticket
    {
        public decimal Price { get; set; }
        public TicketType Type { get; set; }
        public string TicketName { get; set; }
        public int Amount { get; set; }

        public override string ToString()
        {
            return $"{TicketName}\n{Price:c}";
        }
    }
}
