using System.Linq;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public class LogicalOr<T> : ISplittingCriteria<T> {
        private readonly ISplittingCriteria<T>[] _criteria;

        public LogicalOr(params ISplittingCriteria<T>[] criteria) {
            _criteria = criteria;
        }

        public bool IsComplete(T message) {
            return _criteria.Any(c => c.IsComplete(message));
        }
    }
}