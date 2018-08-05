using Autodoc.Marketing.Data.Interfaces;
using Autodoc.Marketing.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Autodoc.Marketing.Data.SqlServer.Implementations
{
    public class MarketingService : IMarketingService
    {
        private readonly string _connectionString;
        public MarketingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<ApplicationUser> Auth(AuthRequest authRequest)
        {
            ApplicationUser appUser = null;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("autodoc_banner.dbo.web_banners_proc", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@action", "AUTH");
                        cmd.Parameters.AddWithValue("@lgn", authRequest.UserName);
                        cmd.Parameters.AddWithValue("@psw", authRequest.Password);

                        await conn.OpenAsync();

                        using (var r = await cmd.ExecuteReaderAsync())
                        {
                            if (await r.ReadAsync())
                            {
                                appUser = new ApplicationUser
                                {
                                    Id = r["id_customers"] != DBNull.Value ? int.Parse(r["id_customers"].ToString()) : 0,
                                    Name = r["obj_name"].ToString(),
                                    Email = r["mail"].ToString(),
                                    Message = r["msg"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                appUser = new UnAuthorizedApplicationUser(ex.Message);
            }

            return appUser;
        }
    }
}
