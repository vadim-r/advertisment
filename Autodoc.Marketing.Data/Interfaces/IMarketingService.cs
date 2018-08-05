using Autodoc.Marketing.Data.Models;
using System.Threading.Tasks;

namespace Autodoc.Marketing.Data.Interfaces
{
    public interface IMarketingService
    {
        Task<ApplicationUser> Auth(AuthRequest authRequest);
    }
}
