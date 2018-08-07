using System;

namespace Autodoc.Marketing.Data.Models
{
	public class Token
    {
		public int Id { get; set; }

		public string ClientId { get; set; }

		public string UserId { get; set; }

		public int Type { get; set; }

		public string Value { get; set; }

		public DateTime CreatedDate { get; set; }
    }
}
