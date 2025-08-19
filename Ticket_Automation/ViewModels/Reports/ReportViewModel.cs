namespace Ticket_Automation.ViewModels.Reports
{
    public class ReportViewModel
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int SolvedTickets { get; set; }
        public List<TopPersonnelReportViewModel> TopPersonnel { get; set; }
    }
}
