using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Common.V5_14_4
{
    public class FormattedPartNumber
    {
        private const int PART_NUMBER_MAX_LENGTH = 22;
        private const int PREFIX_LENGTH = 6;
        private const int BASE_LENGTH = 14;
        protected const string PART_SUFFIX = "Part Suffix";
        protected const string PART_PREFIX = "Part Prefix";
        protected const string PART_BASE = "Part Base";

        protected readonly (string prefix, string base_, string suffix) _splitPartNumber;

        public string Prefix => _splitPartNumber.prefix;

        public string Base => _splitPartNumber.base_;
        protected bool HasBase => !string.IsNullOrEmpty(Base);

        public string Suffix => _splitPartNumber.suffix;
        protected bool HasSuffix => !string.IsNullOrEmpty(Suffix);

        public FormattedPartNumber(string partNumber)
        {
            _splitPartNumber = Split(partNumber.RemoveSpecialCharacters().MaxLength(PART_NUMBER_MAX_LENGTH));
        }

        public virtual ItemIdentificationType[] ToItemIdentificationGroup()
        {
            var partNumbers = new List<ItemIdentificationType> {
                new ItemIdentificationType {
                    ItemID = new IdentifierType {
                        schemeName = PART_PREFIX,
                        Value = Prefix
                    }
                }
            };

            if (HasBase)
            {
                partNumbers.Add(new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeName = PART_BASE,
                        Value = Base
                    }
                });
            }

            if (HasSuffix)
            {
                partNumbers.Add(new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeName = PART_SUFFIX,
                        Value = Suffix
                    }
                });
            }

            return partNumbers.ToArray();
        }

        protected static (string prefix, string base_, string suffix) Split(string partNumber)
        {
            if (partNumber.Length <= PREFIX_LENGTH)
            {
                return (
                    partNumber,
                    string.Empty,
                    string.Empty
                );
            }

            if (partNumber.Length <= BASE_LENGTH)
            {
                return (
                    partNumber.Substring(0, PREFIX_LENGTH),
                    partNumber.Substring(PREFIX_LENGTH),
                    string.Empty
                );
            }

            return (
                partNumber.Substring(0, PREFIX_LENGTH),
                partNumber.Substring(PREFIX_LENGTH, PART_NUMBER_MAX_LENGTH - BASE_LENGTH),
                partNumber.Substring(BASE_LENGTH)
            );
        }
    }
}
