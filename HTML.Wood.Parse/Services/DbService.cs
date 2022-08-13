using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace HTML.Wood.Parse.Services.Services
{
    internal class DbService : IDisposable
    {
        private const string ConnectionStringConfig = "DefaultConnection";

        private readonly SqlConnection _sqlConnection;

        public DbService()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringConfig].ConnectionString;

            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
        }

        public async Task ExecuteNonQueryAsync(string sqlExpression, CancellationToken cancellationToken)
        {
            using (var command = new SqlCommand(sqlExpression, _sqlConnection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public void Dispose()
        {
            _sqlConnection?.Dispose();
        }
    }
}
