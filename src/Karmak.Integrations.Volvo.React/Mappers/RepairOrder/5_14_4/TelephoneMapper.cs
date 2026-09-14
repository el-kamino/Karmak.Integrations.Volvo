using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4
{
    public static class TelephoneMapper
    {
        private const int COMPLETE_NUMBER_MAX_LENGTH = 20;
        private static readonly PhoneType[] AllPhoneTypes =
            Enum
                .GetNames(typeof(PhoneType))
                .Select(Enum.Parse<PhoneType>)
                .ToArray();

        public static CommunicationABIETypeStar[] GetTelephonesFor(Contact contact, params PhoneType[] validTypes)
        {
            if (validTypes.IsEmpty())
                validTypes = AllPhoneTypes;

            var phoneNumbers = new List<CommunicationABIETypeStar>();
            foreach (var type in validTypes)
            {
                var completeNumber = contact?.Phones.FirstOrDefault(phone => phone.Type == type)?.Number;
                CommunicationABIETypeStar entry = FormattedPhone(type, completeNumber);
                if (entry != null)
                    phoneNumbers.Add(entry);
            }

            return phoneNumbers.Count > 0
                ? phoneNumbers.ToArray()
                : null;
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
