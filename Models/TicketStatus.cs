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
                 * Every ticket MUST have a status
                 * If there are no valid ticket statuses, create them
                 *
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
