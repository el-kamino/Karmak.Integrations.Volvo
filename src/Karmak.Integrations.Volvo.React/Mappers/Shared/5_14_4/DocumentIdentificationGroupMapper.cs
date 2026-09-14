using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4
{
    public static class DocumentIdentificationGroupMapper
    {
        private const int DOCUMENT_ID_MAX_LENGTH = 16;
        private const int ALTERNATE_DOCUMENT_ID_MAX_LENGTH = 20;
        private const string ALTERNATE_DOCUMENT_ID_ROLE_CODE = "Customer Purchase Order Number";

        public static DocumentIdentificationGroupType Map(string value, string alternativeId = null)
        {
            var formattedId = GetAlphanumericOnly(alternativeId);
            return new DocumentIdentificationGroupType
            {
                DocumentIdentification = new DocumentIdentificationType
                {
                    DocumentID = new IdentifierType
                    {
                        Value = value.MaxLength(DOCUMENT_ID_MAX_LENGTH)
                    }
                },
                AlternateDocumentIdentification = string.IsNullOrWhiteSpace(formattedId)
                    ? null
                    : new[] {
                        new DocumentIdentificationType {
                            DocumentID = new IdentifierType {
                                Value = formattedId.MaxLength(ALTERNATE_DOCUMENT_ID_MAX_LENGTH)
                            },
                            AgencyRoleCode = ALTERNATE_DOCUMENT_ID_ROLE_CODE
                        }
                    }
            };
        }

        private static string GetAlphanumericOnly(string s)
        {
            if (s is null)
                return null;

            string newStr = string.Empty;
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c)) 
                    newStr += c;
            }
            return newStr;
        }
    }
}