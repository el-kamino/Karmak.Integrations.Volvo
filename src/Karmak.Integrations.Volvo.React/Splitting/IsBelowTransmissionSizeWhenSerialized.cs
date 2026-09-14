using Karmak.Integrations.Volvo.React.Utils;
using System.Text;
using System.Xml.Linq;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public class IsBelowTransmissionSizeWhenSerialized : ISplittingCriteria<XDocument>
    {
        private readonly int _maxTransmisionSize;

        public IsBelowTransmissionSizeWhenSerialized(int maxTransmisionSize)
        {
            _maxTransmisionSize = maxTransmisionSize;
        }

        public bool IsComplete(XDocument message)
        {
            var bytes = message.ToByteArray(Encoding.UTF8);
            return bytes.Length < _maxTransmisionSize;
        }
    }
}