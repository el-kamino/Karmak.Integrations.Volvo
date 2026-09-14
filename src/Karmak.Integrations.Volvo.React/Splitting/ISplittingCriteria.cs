namespace Karmak.Integrations.Volvo.React.Splitting
{
    public interface ISplittingCriteria<in TMessage> {
        bool IsComplete(TMessage message);
    }
}