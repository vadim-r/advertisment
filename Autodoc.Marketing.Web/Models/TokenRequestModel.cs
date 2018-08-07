using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Autodoc.Marketing.Web.Models
{
	[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class TokenRequestModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ClientSecret { get; set; }

		public string ClientId { get; set; }

        public string UserId { get; set; }

        public string GrantType { get; set; }

		public string RefreshToken { get; set; }
    }
}
