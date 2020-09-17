using Fahrkartenautomat.Model;
using System.Collections.Generic;

namespace Fahrkartenautomat
{
    public static class Definitions
    {
        public enum TicketType 
        { 
            AB, AB_Erm,
            BC, BC_Erm,
            ABC, ABC_Erm,
            Kurzstr, Kurzstr_Erm,
        }
        public static List<Ticket> GetPossibleTickets()
        {
            return new List<Ticket>()
            {
                new Ticket() {TicketName = "AB", Type = TicketType.AB,  Price = 2.9m },
                new Ticket() {TicketName = "AB (Ermäßigt)", Type = TicketType.AB_Erm,  Price = 1.8m },
                new Ticket() {TicketName = "BC", Type = TicketType.BC,  Price = 3.3m },
                new Ticket() {TicketName = "BC (Ermäßigt)", Type = TicketType.BC_Erm,  Price = 2.3m },
                new Ticket() {TicketName = "ABC", Type = TicketType.ABC, Price = 3.6m },
                new Ticket() {TicketName = "ABC (Ermäßigt)", Type = TicketType.ABC_Erm, Price = 2.6m },
                new Ticket() {TicketName = "Kurzstrecke", Type = TicketType.Kurzstr, Price = 1.9m },
                new Ticket() {TicketName = "Kurz (Ermäßigt)", Type = TicketType.Kurzstr_Erm, Price = 1.4m },

            };
        }

        /// <summary>
        /// All possible coins and bills to insert.
        /// </summary>
        /// <returns>List of possble coins and bills</returns>
        public static List<decimal> GetPossibleMoney()
        {
            return new List<decimal>()
            {
                0.1m, 0.2m, 0.5m, 1, 2, // coins
                5, 10, 20, 50           // bills
            };
        }
    }
}
