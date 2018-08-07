using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Autodoc.Marketing.Web.Models
{
    public class TokenResponseModel 
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public int Expiration { get; set; }
        public string Message { get; set; }
		public string Name { get; set; }
		public string Id { get; set; }
		public string Email { get; set; }
		public string RefreshToken { get; set; }
	}
}
