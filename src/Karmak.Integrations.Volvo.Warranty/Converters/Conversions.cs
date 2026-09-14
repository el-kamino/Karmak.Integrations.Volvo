using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public static class Conversions
    {
        private static readonly IDictionary<CurrencyCode, DecimalToAmountType> DecimalToAmountCache;
        private static readonly IDictionary<CurrencyCode, MoneyToAmountType> MoneyToAmountCache;

        static Conversions()
        {
            AmountTypeToMoney = new AmountTypeToMoney();
            StringToCodeType = new StringToCodeType();
            StringToCodeTypes = new StringToCodeTypes();
            StringToIdentifierType = new StringToIdentifierType();
            MeasurementToMeasurementLengthType = new MeasurementToMeasurementLengthType();
            StringToNameType = new StringToNameType();
            StringToTextType = new StringToTextType();
            DecimalToQuantityType = new DecimalToQuantityType();
            UnitOfMeasureTypeToLengthUnitsContentType = new UnitOfMeasureTypeToLengthUnitsContentType();
            StringToLanguageEnumeratedType = new StringToLanguageEnumeratedType();
            CountryCodeToCountryEnumeratedType = new CountryCodeToCountryEnumeratedType();
            StringToCountryEnumeratedType = new StringToCountryEnumeratedType();
            PriceComponentTypeToDescriptionTextType = new PriceComponentTypeToDescriptionTextType();
            EnumToString = new EnumToString();
            StringToProcessType = new StringToProcessType();
            ClaimTypeToJobType = new ClaimTypeToJobType();
            CurrencyIdToCurrencyCode = new CurrencyIdToCurrencyCode();

            DecimalToAmountCache = new Dictionary<CurrencyCode, DecimalToAmountType>();
            MoneyToAmountCache = new Dictionary<CurrencyCode, MoneyToAmountType>();

            foreach (CurrencyCode currencyCode in Enum.GetValues(typeof(CurrencyCode)))
            {
                DecimalToAmountCache[currencyCode] = new DecimalToAmountType(currencyCode);
                MoneyToAmountCache[currencyCode] = new MoneyToAmountType(currencyCode);
            }

            NormalizeTimeOfDateTime = new NormalizeTimeOfDateTime();
        }

        public static AmountTypeToMoney AmountTypeToMoney { get; }
        public static ClaimTypeToJobType ClaimTypeToJobType { get; }
        public static StringToCodeType StringToCodeType { get; }
        public static StringToCodeTypes StringToCodeTypes { get; }
        public static StringToIdentifierType StringToIdentifierType { get; }
        public static MeasurementToMeasurementLengthType MeasurementToMeasurementLengthType { get; }
        public static StringToNameType StringToNameType { get; }
        public static StringToTextType StringToTextType { get; }
        public static DecimalToQuantityType DecimalToQuantityType { get; }
        public static UnitOfMeasureTypeToLengthUnitsContentType UnitOfMeasureTypeToLengthUnitsContentType { get; }
        public static StringToLanguageEnumeratedType StringToLanguageEnumeratedType { get; }
        public static CountryCodeToCountryEnumeratedType CountryCodeToCountryEnumeratedType { get; }
        public static StringToCountryEnumeratedType StringToCountryEnumeratedType { get; }
        public static PriceComponentTypeToDescriptionTextType PriceComponentTypeToDescriptionTextType { get; }
        public static EnumToString EnumToString { get; }
        public static StringToProcessType StringToProcessType { get; }
        public static CurrencyIdToCurrencyCode CurrencyIdToCurrencyCode { get; }
        public static NormalizeTimeOfDateTime NormalizeTimeOfDateTime { get; }
        public static DecimalToAmountType DecimalToAmountType(CurrencyCode currencyCode) =>
            DecimalToAmountCache[currencyCode];
        public static MoneyToAmountType MoneyToAmountType(CurrencyCode currencyCode) =>
            MoneyToAmountCache[currencyCode];

        public static TDestination GetOrNull<TSource, TDestination>(TSource source, Func<TSource, TDestination> getFunc) where TDestination : class =>
            GetOrNull(source, value => value != null, getFunc);

        public static TDestination GetOrNull<TSource, TDestination>(TSource source, Predicate<TSource> shouldGet, Func<TSource, TDestination> getFunc) where TDestination : class =>
            shouldGet.Invoke(source) ? getFunc.Invoke(source) : null;
    }
}
