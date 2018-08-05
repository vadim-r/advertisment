namespace Autodoc.Marketing.Data.Models
{
    public class UnAuthorizedApplicationUser : ApplicationUser
    {
        public UnAuthorizedApplicationUser(string message)
        {
            Message = message;
        }
    }
}
