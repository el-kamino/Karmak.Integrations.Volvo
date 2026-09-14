using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Karmak.Integrations.Volvo.Common.Bridge;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class FusionBridgeClient : IFusionClient
    {
        private const string ErrorCode = "Error";
        private const string ContractName = "warr_pmt";
        private readonly IBridgeClient _client;
        private readonly IValidator<WarrantyPaymentInformation> _validator;

        public FusionBridgeClient(IBridgeClient client, IValidator<WarrantyPaymentInformation> validator)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Task ProcessWarrantyPaymentAsync(WarrantyPaymentInformation payment, CancellationToken cancellationToken)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var validationResult = _validator.Validate(payment);

            if (validationResult.IsValid == false)
            {
                throw new InvalidWarrantyPaymentException();
            }

            return ProcessWarrantyPaymentInternalAsync(payment, cancellationToken);
        }

        private async Task ProcessWarrantyPaymentInternalAsync(WarrantyPaymentInformation payment, CancellationToken cancellationToken)
        {
            var response = await _client.SubmitMessageAsync<WarrantyPaymentInformation, ResponseMessage>(ContractName, payment, cancellationToken);

            if (response is null)
                throw new FusionRpcException("the request completed with a null response");

            var errorMessage = response.Code == ErrorCode ? response : null;
            if (errorMessage != null)
            {
                throw new FusionRpcException("Processing the payment in Fusion returned an error", new Exception(errorMessage.BriefMessage));
            }
        }

        public sealed class ResponseMessage
        {
            public string Code { get; set; }
            public int ResponseCode { get; set; }
            public int ResponseMessageType { get; set; }
            public string BriefMessage { get; set; }
            public string FullMessage { get; set; }
            public string Reference { get; set; }
        }
    }
}
