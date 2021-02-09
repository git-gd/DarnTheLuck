namespace DarnTheLuck.Models
{
    public class UserGroup
    {
        public string UserId { get; set; }
        public string GrantId { get; set; }
        public bool Authorized { get; set; }
        public UserGroup(string userId, string grantId) 
        {
            UserId = userId;
            GrantId = grantId;
            Authorized = false;
        }
    }
}
