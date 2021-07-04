using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace LiveDocs.Server.Replacers
{
    public class TransactionTestData
    {
        private readonly Random _random = new Random();

        public async Task InsertSomeRandomTransactionRows()
        {
            try
            {
                var randomCount = _random.Next(5, 15);
                for (var i = 0; i < randomCount; i++)
                {
                    await InsertRandomRow();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task InsertRandomRow()
        {
            await using var connection = new SqlConnection($"Server=localhost\\SQLEXPRESS;Database=LiveDocsDb;Trusted_Connection=True;");
            var cmd = new SqlCommand("INSERT INTO [dbo].[transactions] ([TransactionId],[Type],[Amount],[Status]) VALUES (@id,@type,@amount,@state)", connection);
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@type", GetRandomType());
            cmd.Parameters.AddWithValue("@amount", GetRandomAmount());
            cmd.Parameters.AddWithValue("@state", GetRandomState());
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        private string GetRandomType() =>
            _random.Next(0, 3) switch
            {
                0 => "DC",
                1 => "DD",
                2 => "DC Return",
                _ => "DD Return"
            };

        private string GetRandomState() =>
            _random.Next(0, 1) switch
            {
                0 => "Processed",
                _ => "Pending"
            };

        private decimal GetRandomAmount() =>
            decimal.Parse($"{_random.Next(-500, 5_000)}.{_random.Next(0, 99)}");
    }
}