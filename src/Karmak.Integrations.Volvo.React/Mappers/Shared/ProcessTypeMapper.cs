using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared
{
    public static class ProcessTypeMapper
    {
        private const string REQUIRED_ACTION_CODE = "Add";
        private const string REQUIRED_ACTION_EXPRESSION = "S";

        public static ProcessType Map()
        {
            return new ProcessType
            {
                ActionCriteria = new[] {
                    new ActionCriteriaType {
                        ActionExpression = new[] {
                            new ActionExpressionType {
                                actionCode = REQUIRED_ACTION_CODE,
                                Value = REQUIRED_ACTION_EXPRESSION
                            }
                        }
                    }
                }
            };
        }
    }
}