namespace DarnTheLuck.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        /*
         * This table could be replaced with a Ticket field but is included to show use of related tables
         * 
         * Using a table is usually better than using an Enum
         * 
         * Tables allow users to modify status codes
         * 
         * This table can expanded with additional information such as if a Status has been retired and should not be used for new Tickets
         */
    }
}
