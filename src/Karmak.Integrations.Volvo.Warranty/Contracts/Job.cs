using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Job
    {
        private IList<PartExpense> _partExpenses;
        private IList<LaborExpense> _laborExpenses;
        private IList<MiscellaneousExpense> _miscellaneousExpenses;
        private IList<Expense> _expenses;
        public Job()
        {
            _partExpenses = new List<PartExpense>();
            _laborExpenses = new List<LaborExpense>();
            _miscellaneousExpenses = new List<MiscellaneousExpense>();
        }
        public string Identifier { get; set; }
        public string ComplaintCode { get; set; }
        public string CustomerNotes { get; set; }
        public string TechnicianNotes { get; set; }
        public string InternalDealerNotes { get; set; }
        public string CampaignOptionCode { get; set; }

        public IList<Expense> Expenses
        {
            get => _expenses;
            set => _expenses = value ?? new List<Expense>();
        }

        public IList<PartExpense> PartExpenses
        {
            get => _partExpenses;
            set => _partExpenses = value ?? new List<PartExpense>();
        }

        public IList<LaborExpense> LaborExpenses
        {
            get => _laborExpenses;
            set => _laborExpenses = value ?? new List<LaborExpense>();
        }
        public IList<MiscellaneousExpense> MiscellaneousExpenses
        {
            get => _miscellaneousExpenses;
            set => _miscellaneousExpenses = value ?? new List<MiscellaneousExpense>();
        }
        public PreviousPartClaim PreviousPartClaim { get; set; }
        public Diagnostics Diagnostics { get; set; }
        public Transportation Transportation { get; set; }
        public string AppealReasonCode { get; set; }
        public string AppealComments { get; set; }
    }
}
