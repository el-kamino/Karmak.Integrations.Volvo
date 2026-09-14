using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Persistence;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    /// <summary>
    /// Reads a search request and turns it into a query the claim store can answer. This is the only
    /// place that sees the request as the caller wrote it, so it is where a field name, an operator
    /// and a value stop being text and where a bad one is reported back.
    /// </summary>
    public class ClaimSearchService : IClaimSearchService
    {
        /// <summary>
        /// Page size when a request does not ask for one.
        /// </summary>
        public const int DefaultTop = 50;

        /// <summary>
        /// Largest page a request can ask for. A search reads whole rows, so an unbounded page is a
        /// way to pull the store down one request at a time.
        /// </summary>
        public const int MaximumTop = 200;

        private const string InvalidClaimContextMessageTemplate = "Invalid claim context: {0}";

        private static readonly string FieldNames = string.Join(", ", Enum
            .GetValues<WarrantyClaimField>()
            .Select(ToCamelCase));

        private static readonly string OperatorNames = string.Join(", ", Enum
            .GetValues<WarrantyClaimSearchOperator>()
            .Select(ToCamelCase));

        private readonly IClaimRepository _repository;

        public ClaimSearchService(IClaimRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<ClaimSearchResponse> Search(ClaimSearchRequest request)
        {
            if (request == null)
            {
                throw new InvalidClaimSearchException("A search needs a request.");
            }

            var terms = (request.Terms ?? new List<ClaimSearchTerm>())
                .Where(term => term != null)
                .ToList();

            var keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();

            if (keyword != null && terms.Count > 0)
            {
                throw new InvalidClaimSearchException(
                    "A search is either a keyword across every field or a set of field terms, not both.");
            }

            var query = new WarrantyClaimSearchQuery
            {
                //Never from the request: a caller searches the dealer and branch they signed in as.
                InstanceIdentifier = GetInstanceIdentifier(),
                BranchIdentifier = GetBranchIdentifier(),
                IncludeDeleted = request.IncludeDeleted,
                NewOnly = request.NewOnly,
                Keyword = keyword,
                Terms = terms.Select(ParseTerm).ToList(),
                Skip = ParseSkip(request.Skip),
                Take = ParseTop(request.Top)
            };

            ApplySort(query, request);

            return _repository.Search(query);
        }

        private static WarrantyClaimSearchTerm ParseTerm(ClaimSearchTerm term)
        {
            var field = ParseField(term.Field);
            var @operator = ParseOperator(term.Operator);
            var type = WarrantyClaimSearchSqlBuilder.TypeOf(field);

            if (@operator is WarrantyClaimSearchOperator.BeginsWith or WarrantyClaimSearchOperator.Contains
                && type != WarrantyClaimFieldType.Text)
            {
                throw new InvalidClaimSearchException(
                    $"'{ToCamelCase(@operator)}' matches text, and '{ToCamelCase(field)}' holds " +
                    $"{Describe(type)}. Use equalTo, notEqualTo, greaterThan or lessThan.");
            }

            return new WarrantyClaimSearchTerm
            {
                Field = field,
                Operator = @operator,
                Value = ParseValue(field, type, term.Value),
                DateOnly = type == WarrantyClaimFieldType.Date && NamesADay(term.Value)
            };
        }

        private static object ParseValue(WarrantyClaimField field, WarrantyClaimFieldType type, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidClaimSearchException($"'{ToCamelCase(field)}' was given nothing to match.");
            }

            switch (type)
            {
                case WarrantyClaimFieldType.Number:
                    if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                    {
                        throw new InvalidClaimSearchException(
                            $"'{ToCamelCase(field)}' holds a number and '{value}' is not one.");
                    }

                    return number;

                //RoundtripKind rather than a conversion: a date arriving as 2026-03-02T14:30:00Z stays
                //the instant it names rather than being shifted into another day.
                case WarrantyClaimFieldType.Date:
                    if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
                    {
                        throw new InvalidClaimSearchException(
                            $"'{ToCamelCase(field)}' holds a date and '{value}' is not one.");
                    }

                    return date;

                default:
                    return value;
            }
        }

        /// <summary>
        /// Whether a value names a day rather than a moment within one. Anyone filtering on a date
        /// writes the day, and means all of it; a caller who wants one moment writes the time too and
        /// gets that moment. DateOnly is the arbiter because it is the type that cannot hold a time:
        /// it reads "2026-03-02", and turns away "2026-03-02T14:30:00Z".
        /// </summary>
        private static bool NamesADay(string value) =>
            DateOnly.TryParse(value?.Trim(), CultureInfo.InvariantCulture, out _);

        private static void ApplySort(WarrantyClaimSearchQuery query, ClaimSearchRequest request)
        {
            //Asking for no order leaves the store's own default in place, which is the newest work
            //first. Naming a field is what makes the direction the caller's to choose.
            if (string.IsNullOrWhiteSpace(request.OrderBy))
            {
                return;
            }

            query.SortField = ParseField(request.OrderBy);
            query.SortDescending = request.Descending;
        }

        private static int ParseSkip(int skip)
        {
            if (skip < 0)
            {
                throw new InvalidClaimSearchException($"Cannot skip {skip} results.");
            }

            return skip;
        }

        private static int ParseTop(int top)
        {
            if (top < 0)
            {
                throw new InvalidClaimSearchException($"Cannot return {top} results.");
            }

            //An unasked-for page size is the common case, and a page larger than the cap is trimmed
            //rather than refused so a caller reaching for everything still gets a usable answer.
            return top == 0 ? DefaultTop : Math.Min(top, MaximumTop);
        }

        private static WarrantyClaimField ParseField(string name)
        {
            if (TryParseName<WarrantyClaimField>(name, out var field))
            {
                return field;
            }

            throw new InvalidClaimSearchException(
                $"'{name}' is not a field claims can be searched by. Try one of: {FieldNames}.");
        }

        private static WarrantyClaimSearchOperator ParseOperator(string name)
        {
            if (TryParseName<WarrantyClaimSearchOperator>(name, out var @operator))
            {
                return @operator;
            }

            throw new InvalidClaimSearchException(
                $"'{name}' is not a way to match a field. Try one of: {OperatorNames}.");
        }

        /// <summary>
        /// Reads a name the caller wrote, in whatever casing they wrote it. Digits are turned away
        /// before parsing: Enum.TryParse would otherwise read "3" as whichever member happens to sit
        /// at 3 today, which is a field the caller never named.
        /// </summary>
        private static bool TryParseName<TEnum>(string name, out TEnum parsed) where TEnum : struct, Enum
        {
            parsed = default;

            if (string.IsNullOrWhiteSpace(name) || !char.IsLetter(name.Trim()[0]))
            {
                return false;
            }

            return Enum.TryParse(name.Trim(), ignoreCase: true, out parsed) && Enum.IsDefined(parsed);
        }

        private static string Describe(WarrantyClaimFieldType type) =>
            type == WarrantyClaimFieldType.Number ? "a number" : "a date";

        private static string ToCamelCase<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var name = value.ToString();

            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private static string GetBranchIdentifier()
        {
            return ImplicitElkContext.Current.ApplicationContext?.Branch?.ToString() ??
                   throw new InvalidClaimContextException(
                       string.Format(InvalidClaimContextMessageTemplate, "Elk Branch is missing."));
        }

        private static string GetInstanceIdentifier()
        {
            return ImplicitElkContext.Current.ApplicationContext?.Instance?.ToString() ??
                   throw new InvalidClaimContextException(
                       string.Format(InvalidClaimContextMessageTemplate, "Elk Instance is missing."));
        }
    }
}
