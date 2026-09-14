using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.CustomerUpdate
{
    public static class TelephoneMapper
    {
        private const int COMPLETE_NUMBER_MAX_LENGTH = 20;
        private static readonly PhoneType[] AllPhoneTypes =
            Enum
                .GetNames(typeof(PhoneType))
                .Select(Enum.Parse<PhoneType>)
                .ToArray();

        public static CommunicationABIETypeStar[] Map(this IEnumerable<Phone> phones, params PhoneType[] validTypes)
        {
            if (validTypes.IsEmpty())
            {
                validTypes = AllPhoneTypes;
            }

            return validTypes
                .Select(type => FormattedPhone(type, phones.FirstOrDefault(phone => phone.Type == type)?.Number))
                .RemoveNulls()
                .ToArray();
        }

        private static CommunicationABIETypeStar FormattedPhone(PhoneType type, string completeNumber)
        {
            if (string.IsNullOrWhiteSpace(completeNumber))
                return null;

            return new CommunicationABIETypeStar
            {
                ChannelCode = new CodeType
                {
                    Value = type.ToString().ToLower()
                },
                CompleteNumber = new TextType
                {
                    Value = AlphanumericPhoneTranslator.ToNumeric(completeNumber).MaxLength(COMPLETE_NUMBER_MAX_LENGTH)
                }
            };
        }
    }
}