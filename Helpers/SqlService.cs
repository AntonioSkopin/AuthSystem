using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSystem.Helpers
{
    public interface ISqlService
    {
        public Task<T> PostQuery<T>(string sql, object parameters = null);
        public Task PostQuery(string sql, object parameters = null);
        public Task<List<T>> GetManyQuery<T>(string sql, object parameters = null);
        public Task<T> GetQuery<T>(string sql, object parameters = null);
        public Task<T> DeleteQuery<T>(string sql, object parameters = null);
        public Task DeleteQuery(string sql, object parameters = null);
        public Task<T> PutQuery<T>(string sql, object parameters = null);
        public Task PutQuery(string sql, object parameters = null);
    }

    public class SqlService : ISqlService
    {
        private readonly IConfiguration _configuration;

        protected SqlConnection db
        {
            get
            {
                IConfigurationSection cs = _configuration.GetSection("ConnectionStrings");
                string s = cs["AuthSystemDB"];
                return new SqlConnection(s);
            }
        }

        public SqlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<T> DeleteQuery<T>(string sql, object parameters = null)
        {
            var deletedVal = await db.QueryAsync<T>(sql, parameters);
            return deletedVal.FirstOrDefault();
        }

        public async Task DeleteQuery(string sql, object parameters = null)
        {
            await db.QueryAsync(sql, parameters);
        }

        public async Task<List<T>> GetManyQuery<T>(string sql, object parameters = null)
        {
            var lst = await db.QueryAsync<T>(sql, parameters);
            return lst.ToList();
        }

        public async Task<T> GetQuery<T>(string sql, object parameters = null)
        {
            var getVal = await db.QueryAsync<T>(sql, parameters);
            return getVal.FirstOrDefault();
        }

        public async Task<T> PostQuery<T>(string sql, object parameters = null)
        {
            var postedval = await db.QueryAsync<T>(sql, parameters);
            return postedval.FirstOrDefault();
        }

        public async Task PostQuery(string sql, object parameters = null)
        {
            await db.QueryAsync(sql, parameters);
        }

        public async Task<T> PutQuery<T>(string sql, object parameters = null)
        {
            var updatedVal = await db.QueryAsync<T>(sql, parameters);
            return updatedVal.FirstOrDefault();
        }

        public async Task PutQuery(string sql, object parameters = null)
        {
            await db.QueryAsync(sql, parameters);
        }
    }
}
