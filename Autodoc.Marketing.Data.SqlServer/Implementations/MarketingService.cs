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

		public async Task AddToken(string clientId, int userId, string value)
		{
			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using (var cmd =
						new SqlCommand(
							"insert autodoc_banner.dbo.tokens (ClientId, UserId, Type, Value, CreatedDate) values (@clientId, @userId, @type, @value, @dt)",
							conn))
					{
						cmd.Parameters.AddWithValue("@userId", userId);
						cmd.Parameters.AddWithValue("@clientId", clientId);
						cmd.Parameters.AddWithValue("@dt", DateTime.Now);
						cmd.Parameters.AddWithValue("@type", 0);
						cmd.Parameters.AddWithValue("@value", value);
						await conn.OpenAsync();
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
			catch(Exception ex)
			{
			}
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
									          Id = r["id_customers"] != DBNull.Value
										          ? int.Parse(r["id_customers"].ToString())
										          : 0,
									          Name = r["obj_name"].ToString(),
									          Email = r["mail"].ToString(),
									          Message = r["msg"].ToString()
								          };
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				appUser = new UnAuthorizedApplicationUser(ex.Message);
			}

			return appUser;
		}

		public async Task DeleteUserTokens(int userId)
		{
			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using (var cmd = new SqlCommand("delete * from autodoc_banner.dbo.tokens where UserId=@userId",
						conn))
					{
						cmd.Parameters.AddWithValue("userId", userId);
						await conn.OpenAsync();
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
			catch(Exception ex)
			{
			}
		}

		public async Task<IList<Token>> TokensByClientForUser(string clientId, string userId)
		{
			List<Token> tokens = null;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using (var cmd =
						new SqlCommand(
							"select * from autodoc_banner.dbo.tokens where ClientId=@clientId and UserId=@userId",
							conn))
					{
						cmd.Parameters.AddWithValue("@clientId", clientId);
						cmd.Parameters.AddWithValue("userId", userId);

						await conn.OpenAsync();

						using (var r = await cmd.ExecuteReaderAsync())
						{
							if (await r.ReadAsync())
							{
								var token = new Token
								            {
									            Id = r["Id"] != DBNull.Value ? int.Parse(r["Id"].ToString()) : 0,
									            ClientId = r["ClientId"].ToString(),
									            UserId = r["UserId"].ToString(),
									            Value = r["Value"].ToString(),
									            CreatedDate = (DateTime) r["CreatedDate"]
								            };

								tokens.Add(token);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				tokens = null;
			}

			return tokens;
		}
	}
}
