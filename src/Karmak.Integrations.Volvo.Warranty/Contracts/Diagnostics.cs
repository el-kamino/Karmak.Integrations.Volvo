using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Diagnostics
    {
        public bool IsCheckEngineLightOn { get; set; }
        public IEnumerable<string> PolicyRequiredMeasurementOrResults { get; set; }
        public IEnumerable<string> DiagnosticTroubleCodes { get; set; }
        public IEnumerable<string> BatteryCodes { get; set; }
        public IEnumerable<string> BodyCodes { get; set; }
        public IEnumerable<string> ChassisCodes { get; set; }
        public IEnumerable<string> UndefinedDiagnosticCodes { get; set; }
        public IEnumerable<string> KeyOnEngineOffCodes { get; set; }
        public IEnumerable<string> KeyOnEngineColdCodes { get; set; }
        public IEnumerable<string> KeyOnEngineRunningCodes { get; set; }
        public IEnumerable<string> ReplacedTireDepartmentOfTransportationCodes { get; set; }
        public IEnumerable<string> ReplacementTireDepartmentOfTransportationCodes { get; set; }
    }
}
