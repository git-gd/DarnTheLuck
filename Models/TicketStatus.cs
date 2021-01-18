namespace DarnTheLuck.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        /*
         * Better than using an Enum
         * 
         * Enables the option to allow users to modify status codes
         * 
         * Can be expanded with additional information such as optional HTML/CSS color codes or if a Status has been retired and should not be used for new Tickets
         */
    }
}
