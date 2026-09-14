using FluentValidation;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using System.Text;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn;

internal class PartsReturnProcessor : IPartsReturnProcessor
{
    private const string APPLICATION_IDENTIFIER = "K";
    private const string PARTS_RETURN_FILE_EXT = ".F176";
    private const string SEND_TYPE = "ProcessPartsReturn";
    private const string SEND_TYPE_VERSION = "3.00";

    private readonly ISendFileProcessor _sendFileProcessor;

    public PartsReturnProcessor(ISendFileProcessor sendFileProcessor)
    {
        _sendFileProcessor = sendFileProcessor;
    }

    public async Task ProcessAsync(PartsReturnRequest message)
    {
        var validator = new PartsReturnRequestValidator();
        var result = validator.Validate(message);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        var request = new ProcessPartsReturnRequest
        {
            DealerId = message.PACode,
            OrderNumber = message.OrderNumber,
            PACode = message.PACode,
            FileName = BuildFileName(message),
            SendType = SEND_TYPE,
            SendTypeVersion = SEND_TYPE_VERSION,
            Detail = BuildRequestDetails(message),
            KarmakAccountNumber = message.KarmakAccountNumber
        };

        await _sendFileProcessor.ProcessAsync(request);
    }

    private string BuildFileName(PartsReturnRequest message)
    {
        var fileName = new StringBuilder();
        fileName.Append(APPLICATION_IDENTIFIER);

        var localTime = Utility.AtOffsetInHours(message.RequestDate, message.CreatedTimeZone);
        fileName.AppendFixedWidthPadLeft(localTime.DayOfYear.ToString(), 3, '0');
        fileName.AppendFixedWidthPadLeft(localTime.Hour.ToString(), 2, '0');
        fileName.AppendFixedWidthPadLeft(localTime.Minute.ToString(), 2, '0');

        fileName.Append(PARTS_RETURN_FILE_EXT);
        return fileName.ToString();
    }

    private string BuildJulianDate(PartsReturnRequest message)
    {
        var julianDate = new StringBuilder();
        var localTime = Utility.AtOffsetInHours(message.RequestDate, message.CreatedTimeZone);
        julianDate.AppendFixedWidthPadLeft(localTime.DayOfYear.ToString(), 3, '0');
        julianDate.AppendFixedWidthPadLeft(localTime.Hour.ToString(), 2, '0');
        julianDate.AppendFixedWidthPadLeft(localTime.Minute.ToString(), 2, '0');
        julianDate.AppendFixedWidthPadLeft(localTime.Second.ToString(), 2, '0');
        return julianDate.ToString();
    }

    private string BuildRequestDetails(PartsReturnRequest message)
    {
        var sb = new StringBuilder();

        if (message.PartsToReturn == null)
        {
            message.PartsToReturn = new List<ReturnPartItem>();
        }

        var partsToReturnCount = message.PartsToReturn.Count();
        var fileDate = Utility.AtOffsetInHours(message.RequestDate, message.CreatedTimeZone).ToString("MMddyy");

        BuildHeaderLine(message, sb, partsToReturnCount, fileDate);
        var lineNumber = 10;
        foreach (var line in message.PartsToReturn)
        {
            BuildFixedWidthDetailLine(message, sb, line, lineNumber);
            lineNumber += 10;
        }

        BuildTrailerLine(message, sb, partsToReturnCount, fileDate);

        return sb.ToString();
    }

    private void BuildHeaderLine(PartsReturnRequest message, StringBuilder sb, int partsToReturnCount, string fileDate)
    {
        sb.Append(PartsReturnHeaderFieldConstants.Field1_HDR); // Field 1
        sb.AppendFixedWidthPadRight(message.PACode, 5); // Field 2
        sb.Append(PartsReturnHeaderFieldConstants.Field3_FileType); // Field 3
        sb.Append(message.DistributionCode); // Field 4
        sb.Append(PartsReturnHeaderFieldConstants.Field5_COMBAT); // Field 5
        sb.Append(PartsReturnHeaderFieldConstants.Field6_Spaces); // Field 6
        sb.Append(PartsReturnHeaderFieldConstants.Field7); // Field 7
        sb.Append(fileDate); // Field 8
        sb.Append(PartsReturnHeaderFieldConstants.Field9_FileNumber); // Field 9
        sb.Append(PartsReturnHeaderFieldConstants.Field10_SysGend); // Field 10
        sb.Append(PartsReturnHeaderFieldConstants.Field11_LMDistributionCode); // Field 11
        sb.Append(PartsReturnHeaderFieldConstants.Field12_Spaces); // Field 12
        sb.Append(PartsReturnHeaderFieldConstants.Field13); // Field 13
        sb.Append(PartsReturnHeaderFieldConstants.Field14_Spaces); // Field 14
        sb.Append(PartsReturnHeaderFieldConstants.Field15_EP230); // Field 15
        sb.Append(fileDate); // Field 16
        sb.Append(PartsReturnHeaderFieldConstants.Field17_FileNumber); // Field 17
        sb.AppendFixedWidthPadLeft(partsToReturnCount.ToString(), 8, '0'); // Field 18
        sb.AppendFixedWidthPadRight(message.PACode, 5); // Field 19
        sb.Append(PartsReturnHeaderFieldConstants.Field20); // Field 20
        sb.Append(PartsReturnHeaderFieldConstants.Field21); // Field 21
        sb.Append(PartsReturnHeaderFieldConstants.Field22); // Field 22
        sb.Append(message.DistributionCode); // Field 23
        sb.Append(PartsReturnHeaderFieldConstants.Field24_COMBAT); // Field 24
        sb.Append(PartsReturnHeaderFieldConstants.Field25_Spaces); // Field 25
        sb.Append(PartsReturnHeaderFieldConstants.Field26_LMDistributionCode); // Field 26
        sb.Append(PartsReturnHeaderFieldConstants.Field27); // Field 27
        sb.AppendLine(message.VendorId); // Field 28
    }

    private void BuildFixedWidthDetailLine(PartsReturnRequest message, StringBuilder sb, ReturnPartItem line, int lineNumber)
    {
        sb.AppendFixedWidthPadRight(message.PACode, 5);
        sb.AppendFixedWidthPadRight(line.ReturnType, 4);
        sb.AppendFixedWidthPadLeft(lineNumber.ToString(), 6, '0');
        sb.AppendFixedWidthPadRight(line.PartNumber, 18);
        sb.AppendFixedWidthPadLeft(line.QuantityToReturn.ToString(), 5, '0');
        sb.AppendFixedWidthPadRight(line.BinNumber, 7);
        sb.Append(PartsReturnDetailConstants.Field7_PackingCode);
        sb.Append(PartsReturnDetailConstants.Field8_BoxNumber);
        sb.AppendLine(PartsReturnDetailConstants.Field9_EolDelimiter);
    }

    private void BuildTrailerLine(PartsReturnRequest message, StringBuilder sb, int partsToReturnCount, string fileDate)
    {
        sb.Append(PartsReturnTrailerConstants.Field1_EOFEP);
        sb.Append(fileDate);
        sb.Append(PartsReturnTrailerConstants.Field3_FileNumber);
        sb.Append(PartsReturnTrailerConstants.Field4);
        sb.AppendFixedWidthPadLeft(partsToReturnCount.ToString(), 5, '0');
        sb.Append(PartsReturnTrailerConstants.Field6);
        sb.Append(BuildJulianDate(message));
        sb.Append(PartsReturnTrailerConstants.Field8_FileType);
    }
}
