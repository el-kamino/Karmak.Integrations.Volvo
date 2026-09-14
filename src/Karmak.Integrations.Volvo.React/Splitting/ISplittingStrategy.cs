namespace Karmak.Integrations.Volvo.React.Splitting
{
    public interface ISplittingStrategy<TMessage> {
        (TMessage, TMessage) Split(TMessage message);
    }
}