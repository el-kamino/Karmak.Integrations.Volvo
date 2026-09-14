namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class LaborExpense : Expense
    {
        public User Technician { get; set; }
        public bool IsSublet { get; set; }
    }
}
