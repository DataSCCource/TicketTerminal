using Fahrkartenautomat.Model;
using System.Collections.Generic;

namespace Fahrkartenautomat
{
    public static class Definitions
    {
        public enum TicketType { AB, BC, ABC, Kurzstrecke }
        public static List<Ticket> GetPossibleTickets()
        {
            return new List<Ticket>()
            {
                new Ticket() {Type = TicketType.AB,  Price = 2.9m },
                new Ticket() {Type = TicketType.BC,  Price = 3.3m },
                new Ticket() {Type = TicketType.ABC, Price = 3.6m },
                new Ticket() {Type = TicketType.Kurzstrecke, Price = 1.9m },
            };
        }

        public static List<decimal> GetPossibleMoney()
        {
            return new List<decimal>()
            {
                0.1m, 0.2m, 0.5m, 1, 2,
                5, 10, 20, 50
            };
        }
    }
}
