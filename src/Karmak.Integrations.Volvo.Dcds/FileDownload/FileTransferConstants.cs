namespace Karmak.Integrations.Volvo.Dcds.FileDownload
{
    public static class FileTransferConstants
    {
        public static readonly Dictionary<string, string> FILE_TYPES = new Dictionary<string, string>{
            {"001", "Parts Manager Alert"},
            {"064", "Allocation Schedule"},
            {"084", "Daily Vehicle Status Report"},
            {"093", "Car Status and Summary Counts"},
            {"122", "Projected Vehicle Flow Report"},
            {"133", "Sales Analysis And Reporting / DSE Transaction List - Volvo"},
            {"137", "Parts Entry and Return Register"},
            {"138", "ACES II Repair Register - Monthly"},
            {"139", "Direct Order Receipt Acknowledgement"},
            {"173", "Sales Analysis And Reporting / DSE Transaction List - Lincoln/Mercury"},
            {"221", "FSE - Financial Statement Submission Was Received"},
            {"222", "FSE - Data"},
            {"297", "EFT Confirmation / Reject Report"},
            {"411", "ESPS Cancellation Worksheet"},
            {"412", "ESP Register"},
            {"413", "Customer Memos"},
            {"414", "Dealer Parts Invoices"},
            {"460", "Miscellaneous Invoices"},
            {"461", "ELMS Summary"},
            {"463", "Warranty And Service Systems / ACES II Supporting Documentation Summary"},
            {"466", "Weekly EFT Settlement Statement"},
            {"471", "Warranty Account Receivable Detail from ACES II"},
            {"472", "ESPS – Daily Transactions"},
            {"474", "Packing Slip Receipt"},
            {"476", "Confirmed Return Line Info"},
            {"551", "Proactive Activity Report (MOORS)"},
            {"666", "Repair Order Monthly Summary Report"},
            {"667", "Centralized Invoicing - Dealer Invoice / COV Requests"},
            {"801", "Part Packing Slip"},
            {"903", "CDS Systems Coordinator Message"},
            {"906", "Volvo Credit Dealer EFT Detail"},
            {"909", "Standardized Training And Resource System"},
            {"921", "Dealer Bulletins and Training"}
        };
    }
}