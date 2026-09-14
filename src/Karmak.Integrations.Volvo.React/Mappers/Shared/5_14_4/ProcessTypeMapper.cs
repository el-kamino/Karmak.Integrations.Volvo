using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4
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