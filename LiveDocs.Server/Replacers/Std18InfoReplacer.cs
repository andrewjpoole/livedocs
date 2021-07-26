using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ClearBank.Bacs.Transforms.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Server.Replacers
{
    public class Std18InfoReplacer : IStd18InfoReplacer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Std18InfoReplacer> _logger;

        public Std18InfoReplacer(IConfiguration configuration, ILogger<Std18InfoReplacer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> Render(string replacementTokenMatch)
        {
            // Std18 files are stored in a blob by Swift Audit
            // storage account is the main environment one, name is cbukcoretestnarwhal3f3d in narwhal
            // container name is inbound-send-to-swift-bacs
            // looks like this {"FileLogicalName":"B3DOSLTCLRBGB22XXX2021051420210514133024030.092801_2367697_STD18_PAYMENT.gz","Attachment":"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"}
            
            var stringBuilder = new StringBuilder();
            try
            {

                var connectionString = _configuration.GetConnectionString("mainAzureStorageAccount");
                var blobServiceClient = new BlobServiceClient(connectionString);

                const string containerName = "inbound-send-to-swift-bacs";
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var dateToSearchFor = new DateTimeOffset(new DateTime(2021, 7, 22));
                //var dateToSearchFor = DateTimeOffset.Now.Date // ToDo put this back

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    try
                    {
                        if (blobItem.Properties.CreatedOn.Value.Date != dateToSearchFor.Date) continue;

                        var blobJson = await DownloadBlob(blobItem.Name, containerClient);

                        var (std18, swiftFileName) = GetStd18FromJsonContainingGzippedMessage(blobJson);

                        if (std18 == string.Empty)
                            continue;

                        var parsedStd18 = Std18OutputFile.Parse(std18);

                        var markdown = BuildMarkdownTable(swiftFileName, blobItem.Name, parsedStd18);

                        stringBuilder.AppendLine(markdown);
                        stringBuilder.AppendLine();

                        break; // TODO figure out how to handle the millions of std18 files in narwhal
                    }
                    catch (Exception e)
                    {
                        var message = $"Error thrown while rendering std18 file from blob {blobItem.Name}";
                        _logger.LogError(e, message);
                        stringBuilder.AppendLine(message);
                        stringBuilder.AppendLine();
                    }
                }

                return stringBuilder.ToString();

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error thrown while rendering std18 files. {e.Message}");
                return stringBuilder.ToString();
            }
        }

        private async Task<string> DownloadBlob(string blobName, BlobContainerClient containerClient)
        {
            var blobClient2 = containerClient.GetBlobClient(blobName);
            await using var memoryStream = new MemoryStream();
            await blobClient2.DownloadToAsync(memoryStream);
            var blobJson = Encoding.UTF8.GetString(memoryStream.ToArray());
            blobJson = RemoveUtf8ByteOrderMark(blobJson);
            return blobJson;
        }

        private (string Std18, string SwiftFileName) GetStd18FromJsonContainingGzippedMessage(string blobJson)
        {
            var request = JsonSerializer.Deserialize<BacsInboundPaymentFileRequest>(blobJson);

            if (request is null)
                return (string.Empty, string.Empty);

            var gzippedMessageBytes = Convert.FromBase64String(request.Attachment);

            var gzipCompressor = new GzipCompressor();
            var messageBytes = gzipCompressor.Decompress(gzippedMessageBytes);
            var message = Encoding.Default.GetString(messageBytes);

            var indexOfVol1 = message.IndexOf("VOL1", StringComparison.Ordinal);
            var indexOfLastTapeMark = message.LastIndexOf("*TM*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", StringComparison.Ordinal) + 120;
            var rawStd18 = message.Substring(indexOfVol1, indexOfLastTapeMark - indexOfVol1);

            // Normalise carriage returns otherwise the parser will fail as some lines maybe longer than 120 chars
            var normalizedStd18 = Regex.Replace(rawStd18, @"\r\n|\n\r|\n|\r", "\r\n");

            return (normalizedStd18, request.FileLogicalName);
        }

        private string BuildMarkdownTable(string filename, string blobName, Std18OutputFile std18)
        {
            var sb = new StringBuilder();

            sb.AppendLine("| Property | Value |");
            sb.AppendLine("| -------- | ----- |");
            sb.AppendLine($"| Swift Filename | {filename} |"); // ToDo Use swift filename parser to get more info
            sb.AppendLine($"| Blob Name | {blobName} |");
            sb.AppendLine($"| Hdr1Label.CreationDate (YYDDD)| {std18.Hdr1Label.CreationDate} -> {ConvertJulianDate(std18.Hdr1Label.CreationDate):dddd yy-MM-dd}|");
            sb.AppendLine($"| Hdr1Label.ExpirationDate (YYDDD) | {std18.Hdr1Label.ExpirationDate} -> {ConvertJulianDate(std18.Hdr1Label.ExpirationDate):dddd yy-MM-dd}|");
            sb.AppendLine($"| Hdr1Label.FileSectionNumber | {std18.Hdr1Label.FileSectionNumber} |");
            sb.AppendLine($"| Hdr1Label.FileSequenceNumber | {std18.Hdr1Label.FileSequenceNumber} |");
            sb.AppendLine($"| Eof1Label.FileSequenceNumber | {std18.Eof1Label.FileSequenceNumber} |");
            sb.AppendLine($"| Eof1Label.FileSectionNumber | {std18.Eof1Label.FileSectionNumber} |");
            sb.AppendLine($"| Uhl1Label.ProcessingDate (YYDDD) | {std18.Uhl1Label.ProcessingDate} -> {ConvertJulianDate(std18.Uhl1Label.ProcessingDate):dddd yy-MM-dd}|");
            sb.AppendLine($"| Uhl1Label.UHL | {std18.Uhl1Label.UHL} |");
            sb.AppendLine($"| Uhl1Label.WorkCode | {std18.Uhl1Label.WorkCode} |");
            sb.AppendLine($"| Utl1Label.CreditItemCount | {std18.Utl1Label.CreditItemCount} |");
            sb.AppendLine($"| Utl1Label.CreditValueTotal | {std18.Utl1Label.CreditValueTotal} |");
            sb.AppendLine($"| Utl1Label.DDIItemCount | {std18.Utl1Label.DDIItemCount} |");
            sb.AppendLine($"| Utl1Label.DebitItemCount | {std18.Utl1Label.DebitItemCount} |");
            sb.AppendLine($"| Utl1Label.DebitValueTotal | {std18.Utl1Label.DebitValueTotal} |");
            sb.AppendLine($"| Utl1Label.UTL | {std18.Utl1Label.UTL} |");

            sb.AppendLine();

            sb.AppendLine($"| Type | Count | Sum |");
            sb.AppendLine("|------ | ------ | ------ |");
            sb.AppendLine($"| Direct Credits | {std18.DirectCredits.Count()} | £{std18.DirectCredits.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| UnappliedDirectCredits | {std18.UnappliedDirectCredits.Count()} | £{std18.UnappliedDirectCredits.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| AutomatedSettlementCredits | {std18.AutomatedSettlementCredits.Count()} | £{std18.AutomatedSettlementCredits.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| Direct Debits | {std18.DirectDebits.Count()} | £{std18.DirectDebits.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| UnpaidDirectDebits | {std18.UnpaidDirectDebits.Count()} | £{std18.UnpaidDirectDebits.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| DirectDebitInstructions | {std18.DirectDebitInstructions.Count()} | n/a |");
            sb.AppendLine($"| Recalls | {std18.Recalls.Count()} | £{std18.Recalls.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| Tellers | {std18.Tellers.Count()} | £{std18.Tellers.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| ClaimForUnpaidCheques | {std18.ClaimForUnpaidCheques.Count()} | £{std18.ClaimForUnpaidCheques.Sum(x => int.Parse(x.AmountInPence)) / 100} |");
            sb.AppendLine($"| Contras | {std18.Contras.Count()} | £{std18.Contras.Sum(x => int.Parse(x.AmountInPence)) / 100} |");

            return sb.ToString();
        }

        private static string RemoveUtf8ByteOrderMark(string inputString)
        {
            var utf8ByteOrderMark = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (inputString.StartsWith(utf8ByteOrderMark, StringComparison.OrdinalIgnoreCase))
                inputString = inputString.Remove(0, utf8ByteOrderMark.Length);
            return inputString.Replace("\0", "");
        }

        private DateTime ConvertJulianDate(string julianDate)
        {
            int year = Convert.ToInt32(julianDate.Substring(0,2));
            int day = Convert.ToInt32(julianDate.Substring(2));
            DateTime dateTime = new DateTime(1999 + year, 12, 18, new JulianCalendar());

            dateTime = dateTime.AddDays(day);

            return dateTime;
        }
    }

    public class BacsInboundPaymentFileRequest
    {
        /// <summary>
        /// The filename specified in the DataPdu
        /// </summary>
        public string FileLogicalName { get; set; }

        /// <summary>
        /// Attachment stored as a base64 string.
        /// </summary>
        public string Attachment { get; set; }
    }

    public class GzipCompressor 
    {
        /// <summary>
        /// The default buffer size (64Kb).
        /// </summary>
        internal const int DefaultBufferSize = 64 * 1024;

        /// <summary>
        /// Compresses the given bytes.
        /// </summary>
        public byte[] Compress(byte[] bytes)
        {

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress), DefaultBufferSize))
                {
                    gzs.Write(bytes, 0, bytes.Length);
                }

                return compressIntoMs.ToArray();
            }
        }

        /// <summary>
        /// Decompresses bytes which were initially compressed by this utility.
        /// </summary>
        public byte[] Decompress(byte[] bytes)
        {
            using (var compressedMs = new MemoryStream(bytes))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var stream = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), DefaultBufferSize))
                    {
                        stream.CopyTo(decompressedMs);
                    }

                    return decompressedMs.ToArray();
                }
            }
        }
    }
}