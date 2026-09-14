using Karmak.Integrations.Volvo.Oasis.Models;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public interface ISymptomCodeProvider
    {
        Task<List<CodeLookupRest>> GetSymptomCodesAsync();
    }
}
