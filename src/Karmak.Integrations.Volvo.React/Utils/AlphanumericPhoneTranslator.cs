using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class AlphanumericPhoneTranslator
    {
        private static readonly Dictionary<char, char> keypadDictionary = new Dictionary<char, char>()
        {
            { 'a', '2' }, { 'b', '2' }, { 'c', '2' },
            { 'd', '3' }, { 'e', '3' }, { 'f', '3' },
            { 'g', '4' }, { 'h', '4' }, { 'i', '4' },
            { 'j', '5' }, { 'k', '5' }, { 'l', '5' },
            { 'm', '6' }, { 'n', '6' }, { 'o', '6' },
            { 'p', '7' }, { 'q', '7' }, { 'r', '7' }, { 's', '7' },
            { 't', '8' }, { 'u', '8' }, { 'v', '8' },
            { 'w', '9' }, { 'x', '9' }, { 'y', '9' }, { 'z', '9' },
        };

        public static string ToNumeric(string alphaNumericPhoneNumber)
        {
            return string.IsNullOrEmpty(alphaNumericPhoneNumber) ? alphaNumericPhoneNumber : FormatPhone(alphaNumericPhoneNumber)
                .Select(c => char.IsDigit(c)
                            ? c
                            : ToDigit(c))
                .Aggregate(string.Empty, (accumulator, digit) => $"{accumulator}{digit}");
        }

        private static char ToDigit(char letter)
        {
            var foundDigit = keypadDictionary.TryGetValue(letter, out var digit);
            return foundDigit
                ? digit
                : letter;
        }

        private static string FormatPhone(string inputPhoneNumber)
        {
            return inputPhoneNumber?.RemoveSpecialCharacters().ToLower();
        }
    }
}
