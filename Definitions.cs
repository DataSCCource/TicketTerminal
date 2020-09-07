using Fahrkartenautomat.Model;
using System.Collections.Generic;

namespace Fahrkartenautomat
{
    public static class Definitions
    {
        public static List<Ticket> GetPossibleTickets()
        {
            return new List<Ticket>()
            {
                new Ticket() {Type = Model.TicketType.Normal, Price = 2.9m },
                new Ticket() {Type = Model.TicketType.Ermaeßigt, Price = 1.8m },
                new Ticket() {Type = Model.TicketType.Kurzstrecke, Price = 1.9m },
                new Ticket() {Type = Model.TicketType.Fahrrad, Price = 2 },
                new Ticket() {Type = Model.TicketType.Monatskarte, Price = 60 },
            };
        }

        public static List<decimal> GetPossibleMoney()
        {
            return new List<decimal>()
            {
                0.01m, 0.02m, 0.05m, 0.1m, 0.2m, 0.5m, 1, 2,
                5, 10, 20, 50
            };
        }
    }
}
