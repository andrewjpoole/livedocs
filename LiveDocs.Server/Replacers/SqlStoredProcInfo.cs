using System;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace LiveDocs.Server.Replacers
{
    public class SqlStoredProcInfo : IReplacer
    {
        public string Render(string dbAndStoredProcName)
        {
            try
            {
                var dbAndStoredProcNameParts = dbAndStoredProcName.Split(".");
                var dbName = dbAndStoredProcNameParts[0];
                var sprocName = dbAndStoredProcNameParts[1];

                using var conn = new SqlConnection($"Server=localhost\\SQLEXPRESS;Database={dbName};Trusted_Connection=True;");
                conn.Open();

                var command = new SqlCommand(sprocName, conn);

                command.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add(new SqlParameter("@CustomerID", custId));

                var sbColumnNames = new StringBuilder("|");
                var sbColumnAlignment = new StringBuilder("|");
                var sbRows = new StringBuilder();

                using var reader = command.ExecuteReader();
                var columns = reader.GetColumnSchema();
                foreach (var dbColumn in columns)
                {
                    sbColumnNames.Append($" {dbColumn.ColumnName}&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;|");
                    sbColumnAlignment.Append(" --- |");
                }

                while (reader.Read())
                {
                    sbRows.Append("\n|");
                    foreach (var dbColumn in columns)
                    {
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