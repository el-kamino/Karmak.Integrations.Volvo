using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared
{
    public static class DocumentIdentificationGroupMapper
    {
        private const int DOCUMENT_ID_MAX_LENGTH = 16;
        private const int ALTERNATE_DOCUMENT_ID_MAX_LENGTH = 20;
        private const string ALTERNATE_DOCUMENT_ID_ROLE_CODE = "Customer Purchase Order Number";

        public static DocumentIdentificationGroupType Map(string value, string alternativeId = null)
        {
            return new DocumentIdentificationGroupType
            {
                DocumentIdentification = new DocumentIdentificationType
                {
                    DocumentID = new IdentifierType
                    {
                        Value = value.MaxLength(DOCUMENT_ID_MAX_LENGTH)
                    }
                },
                AlternateDocumentIdentification = string.IsNullOrWhiteSpace(alternativeId)
                    ? null
                    : new[] {
                        new DocumentIdentificationType {
                            DocumentID = new IdentifierType {
                                Value = alternativeId.MaxLength(ALTERNATE_DOCUMENT_ID_MAX_LENGTH)
                            },
                            AgencyRoleCode = ALTERNATE_DOCUMENT_ID_ROLE_CODE
                        }
                    }
            };
        }
    }
}