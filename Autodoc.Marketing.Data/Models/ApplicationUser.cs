using System.Collections.Generic;

namespace Autodoc.Marketing.Data.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
		public IList<Token> Tokens { get; set; }
        public bool Authenticated => Id > 0;
    }
}
