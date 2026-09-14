namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public static class RepairOrderTaskStatus {
        public const string OPEN = "open";
        public const string CONTRACT = "contract";
        public const string CONTRACT_INVOICE = "contract invoice";
        public const string DEFERRED = "deferred";
        public const string HOLD = "hold";
        public const string INVOICED = "invoiced";
        public const string CLOSED = "closed";
        public const string QUOTE = "quote";
        public const string READY_TO_INVOICE = "ready to invoice";
        public const string SUBLET = "sublet";
        public const string VOIDED = "voided";
        public const string WAITING_FOR_PARTS = "waiting for parts";
        public const string QUOTE_DECLINED = "quote declined";
    }
}