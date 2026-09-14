using FluentValidation;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using System.Text;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement
{
    internal class FinancialStatementReceivedProcessor : IFinancialStatementReceivedProcessor
    {
        private const string APPLICATION_IDENTIFIER = "K";
        private const string FIN_STATEMENT_FILE_EXT = ".F465";
        private const string SEND_TYPE = "ProcessFinancialStatement";
        private const string SEND_TYPE_VERSION = "3.00";

        private readonly ISendFileProcessor _sendFileProcessor;

        public FinancialStatementReceivedProcessor(ISendFileProcessor sendFileProcessor)
        {
            _sendFileProcessor = sendFileProcessor;
        }

        public async Task ProcessAsync(FinancialStatementRequest message)
        {
            var validator = new SubmitFinancialStatementRequestValidator();
            var result = validator.Validate(message);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            var request = new SubmitVolvoFinancialStatementRequest
            {
                DealerId = message.PACode,
                FileName = BuildFileName(message),
                SendType = SEND_TYPE,
                SendTypeVersion = SEND_TYPE_VERSION,
                Detail = message.FormattedFileContents,
                KarmakAccountNumber = message.KarmakAccountNumber
            };

            await _sendFileProcessor.ProcessAsync(request);
        }

        private string BuildFileName(FinancialStatementRequest message)
        {
            var requestDate = message.RequestDate ?? DateTime.Now;

            var fileName = new StringBuilder();
            fileName.Append(APPLICATION_IDENTIFIER);
            fileName.AppendFixedWidthPadLeft(requestDate.DayOfYear.ToString(), 3, '0');
            fileName.AppendFixedWidthPadLeft(requestDate.Hour.ToString(), 2, '0');
            fileName.AppendFixedWidthPadLeft(requestDate.Minute.ToString(), 2, '0');
            fileName.Append(FIN_STATEMENT_FILE_EXT);
            return fileName.ToString();
        }
    }
}
