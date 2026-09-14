using Karmak.Integrations.Volvo.Dcds.Contracts;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement;

public interface IFinancialStatementReceivedProcessor
{
    Task ProcessAsync(FinancialStatementRequest message);
}