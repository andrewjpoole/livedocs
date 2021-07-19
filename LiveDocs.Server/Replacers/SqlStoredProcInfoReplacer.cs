using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server.Replacers
{
    public class SqlStoredProcInfoReplacer : ISqlStoredProcInfoReplacer
    {
        private readonly IConfiguration _configuration;

        public SqlStoredProcInfoReplacer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Render(string dbAndStoredProcName)
        {
            try
            {
                var dbAndStoredProcNameParts = dbAndStoredProcName.Split(".");
                var dbName = dbAndStoredProcNameParts[0];
                var sprocName = dbAndStoredProcNameParts[1];

                var connectionString = _configuration.GetConnectionString(dbName);

                using var conn = new SqlConnection(connectionString);
                conn.Open();

                var command = new SqlCommand(sprocName, conn);

                command.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add(new SqlParameter("@CustomerID", custId));

                var sbColumnNames = new StringBuilder("|");
                var sbColumnAlignment = new StringBuilder("|");
                var sbRows = new StringBuilder();

                await using var reader = await command.ExecuteReaderAsync();
                var columns = await reader.GetColumnSchemaAsync();
                foreach (var dbColumn in columns)
                {
                    sbColumnNames.Append($" {dbColumn.ColumnName}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;|");
                    sbColumnAlignment.Append(" --- |");
                }

                while (await reader.ReadAsync())
                {
                    sbRows.Append("\n|");
                    foreach (var dbColumn in columns)
                    {
                        if(dbColumn.DataTypeName == "money")
                            sbRows.Append($" {((decimal)reader[dbColumn.ColumnName]):C} |");
                        else
                            sbRows.Append($" {reader[dbColumn.ColumnName]} |");
                    }
                }

                return $"{sbColumnNames}\n{sbColumnAlignment}{sbRows}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}