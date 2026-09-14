using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class TelephoneMapper
    {
        private const string DefaultPhoneNumber = "0";
        private const int COMPLETE_NUMBER_MAX_LENGTH = 20;
        private static readonly PhoneType[] AllPhoneTypes =
            Enum
                .GetNames(typeof(PhoneType))
                .Select(Enum.Parse<PhoneType>)
                .ToArray();

        public static CommunicationABIETypeStar[] GetTelephonesFor(IEnumerable<Phone> phones, params PhoneType[] validTypes)
        {
            return HasValidPhone(phones)
                ? ResolveTypeList(validTypes)
                    .Select(type => new { type, number = GetNumberByType(phones, type) }) //create type/number pairs
                    .Where(pair => !string.IsNullOrWhiteSpace(pair.number)) //filter for valid numbers
                    .Select(x => FormattedPhone(x.type, x.number)) //create for valid pairs
                    .ToArray()
                : null;
        }

        private static bool HasValidPhone(IEnumerable<Phone> phones) =>
            phones?.Any(phone => !string.IsNullOrWhiteSpace(phone.Number)) ?? false;

        private static PhoneType[] ResolveTypeList(PhoneType[] validTypes) =>
            validTypes.IsEmpty() ? AllPhoneTypes : validTypes;

        private static string GetNumberByType(IEnumerable<Phone> phones, PhoneType type) =>
            phones.FirstOrDefault(phone => phone.Type == type)?.Number;

        private static CommunicationABIETypeStar FormattedPhone(PhoneType type, string completeNumber)
        {
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
