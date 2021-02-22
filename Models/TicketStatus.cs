using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DarnTheLuck.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static async Task<TicketStatus> CreateAsync(DarnTheLuck.Data.ApplicationDbContext source)
        {

            int count = await source.TicketStatuses.CountAsync();

            if (count < 1)
            {
                /*
                 * Ticket Status is stored in a related table so every ticket must have a status
                 *
                 * Storing ticket status in a related table is completely unnecessary.
                 * 
                 * On first run, set the statuses:
                 */

                string[] statuses =
                {
                        "Created",
                        "Limbo",
                        "Ready",
                        "Shipped"
                    };

                foreach (string status in statuses)
                {
                    await source.TicketStatuses.AddAsync(new TicketStatus() { Name = status });
                }

                await source.SaveChangesAsync();
            }

            return await source.TicketStatuses.FirstOrDefaultAsync(ts => ts.Name == "Created");

        }
    }
}
