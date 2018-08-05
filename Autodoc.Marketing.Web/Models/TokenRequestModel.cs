using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Autodoc.Marketing.Web.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class TokenRequestModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public string ClientSecret { get; set; }

        public string ClientId { get; set; }

        public string GrantType { get; set; }
    }
}
