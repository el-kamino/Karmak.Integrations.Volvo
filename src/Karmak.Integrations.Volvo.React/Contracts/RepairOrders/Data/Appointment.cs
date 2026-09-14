using System;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class Appointment {
        public DateTime? AppointmentMadeDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
    }
}