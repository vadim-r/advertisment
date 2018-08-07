using Autodoc.Marketing.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Autodoc.Marketing.Data.Interfaces
{
    public interface IMarketingService
    {
        Task<ApplicationUser> Auth(AuthRequest authRequest);
		Task<IList<Token>> TokensByClientForUser(string clientId, string userId);
		Task DeleteUserTokens(int userId);
		Task AddToken(string clientId, int userId, string value);
    }
}
