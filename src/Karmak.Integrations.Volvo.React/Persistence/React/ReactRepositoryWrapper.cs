using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using ReactCustomerUpdate = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class ReactRepositoryWrapper : IReactRepositoryWrapper
    {
        private readonly IReactDataRepository _sqlRepo;

        public ReactRepositoryWrapper(IReactDataRepository sqlRepo)
        {
            _sqlRepo = sqlRepo;
        }

        public async Task CreateRepairOrderAsync(string kan, string paCode, RepairOrderSnapshot repairOrder)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kan);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(repairOrder);

            var entity = new RepairOrderDataEntity
            {
                Entity = repairOrder,
                KAN = kan,
                PACode = paCode
            };

            await _sqlRepo.CreateAsync(entity);
        }

        public async Task CreateCustomerUpdateAsync(string kan, string paCode, ReactCustomerUpdate customerUpdate)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kan);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(customerUpdate);

            var entity = new CustomerUpdateDataEntity
            {
                Entity = customerUpdate,
                KAN = kan,
                PACode = paCode
            };

            await _sqlRepo.CreateAsync(entity);
        }

        public async Task CreatePartsSalesOrderAsync(string kan, string paCode, PartsSalesOrder partsSalesOrder)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kan);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(partsSalesOrder);

            var entity = new PartsSalesOrderDataEntity
            {
                Entity = partsSalesOrder,
                KAN = kan,
                PACode = paCode
            };

            await _sqlRepo.CreateAsync(entity);
        }

        public async Task CreateVehicleSalesOrderAsync(string kan, string paCode, VehicleSalesOrder vehicleSalesOrder)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(kan);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(vehicleSalesOrder);

            var entity = new VehicleSalesOrderDataEntity
            {
                Entity = vehicleSalesOrder,
                KAN = kan,
                PACode = paCode
            };

            await _sqlRepo.CreateAsync(entity);
        }

        public async Task<List<RepairOrderSnapshot>> QueryRepairOrdersAsync(string paCode, RepairOrderSnapshotCriteria criteria)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(criteria);

            List<RepairOrderSnapshot> snapshots;

            snapshots = await _sqlRepo.QueryAsync<RepairOrderSnapshot>(
                paCode,
                RepairOrderDataEntity.EntityTypeName,
                IdFilter(criteria.IsRepairOrderConstrained, criteria.RepairOrderNumbers),
                criteria.IsDateConstrained ? criteria.WindowStart : null,
                criteria.IsDateConstrained ? criteria.WindowEnd : null);

            if (criteria.OnlyLastSnapshotPerRepairOrder)
            {
                snapshots = snapshots
                    .GroupBy(s => s.RepairOrderNumber)
                    .Select(group => group.OrderByDescending(s => s.SnapshotSequenceNumber).First())
                    .ToList();
            }

            return snapshots;
        }

        public async Task<List<PartsSalesOrder>> QueryPartsSalesOrdersAsync(string paCode, PartsSalesOrderSearchCriteria criteria)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(criteria);

            return await _sqlRepo.QueryAsync<PartsSalesOrder>(
                paCode,
                PartsSalesOrderDataEntity.EntityTypeName,
                IdFilter(criteria.IsInvoiceConstrained, criteria.InvoiceNumbers),
                criteria.IsDateConstrained ? criteria.WindowStart : null,
                criteria.IsDateConstrained ? criteria.WindowEnd : null);
        }

        public async Task<List<VehicleSalesOrder>> QueryVehicleSalesOrdersAsync(string paCode, VehicleSalesOrderSearchCriteria criteria)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(criteria);

            return await _sqlRepo.QueryAsync<VehicleSalesOrder>(
                paCode,
                VehicleSalesOrderDataEntity.EntityTypeName,
                IdFilter(criteria.IsInvoiceConstrained, criteria.InvoiceNumbers),
                criteria.IsDateConstrained ? criteria.WindowStart : null,
                criteria.IsDateConstrained ? criteria.WindowEnd : null);
        }

        public async Task<List<ReactCustomerUpdate>> QueryCustomerUpdatesAsync(string paCode, CustomerUpdateSearchCriteria criteria)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(paCode);
            ArgumentNullException.ThrowIfNull(criteria);

            // Customer updates are keyed on the customer key, which no retransmit criterion maps to,
            // so only the date window narrows the result set.
            return await _sqlRepo.QueryAsync<ReactCustomerUpdate>(
                paCode,
                CustomerUpdateDataEntity.EntityTypeName,
                new List<string>(),
                criteria.IsDateConstrained ? criteria.WindowStart : null,
                criteria.IsDateConstrained ? criteria.WindowEnd : null);
        }

        private static List<string> IdFilter(bool isConstrained, IEnumerable<string> ids) =>
            isConstrained ? ids.ToList() : new List<string>();
    }
}
