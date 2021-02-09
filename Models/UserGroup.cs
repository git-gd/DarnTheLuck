namespace DarnTheLuck.Models
{
    public class UserGroup
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string GrantId { get; set; }
        public string GrantEmail { get; set; }
        public bool Authorized { get; set; }
        public UserGroup() 
        {
            Authorized = false;
        }
    }
}
