using static Fahrkartenautomat.Definitions;

namespace Fahrkartenautomat.Model
{
    public class Ticket
    {
        
        public decimal Price { get; set; }
        public TicketType Type { get; set; }
        public int Amount { get; set; }

        public override string ToString()
        {
            return $"{Type} | {Price:c}";
        }
    }
}
