using AutoBogus;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Handlers
{
    public class PushValidationHandlerTests
    {
        private readonly IValidator<UpdateSnapshotMessage> _validator = Substitute.For<IValidator<UpdateSnapshotMessage>>();
        private readonly IValidator<UpdateSnapshotMessage> _badValidator = Substitute.For<IValidator<UpdateSnapshotMessage>>();
        public PushValidationHandlerTests()
        {
            _validator.Validate(Arg.Any<UpdateSnapshotMessage>())
                .Returns(new ValidationResult());

            _badValidator.Validate(Arg.Any<UpdateSnapshotMessage>())
                .Returns(new ValidationResult(new List<ValidationFailure>()
                    {
                        new ValidationFailure("Field", "Field is invalid")
                    }));
        }

        [Fact]
        public void WhenCtorParametersAreNull_It_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new PushValidationHandler(null); });
        }

        [Fact]
        public void WhenValidating_It_CallsValidatesOnEachUpdate()
        {
            var updates = AutoFaker.Generate<UpdateSnapshotMessage>(3);
            var handler = new PushValidationHandler(_validator);

            var result = handler.Validate(updates);

            _validator.Received(3).Validate(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public void WhenValidating_It_FormatsValidationErrors()
        {
            var update = AutoFaker.Generate<UpdateSnapshotMessage>(1);
            var handler = new PushValidationHandler(_badValidator);

            var result = handler.Validate(update);

            Assert.Single(result);
            Assert.Single(result.First().Errors);
            Assert.Equal("Field is invalid", result.First().Errors.First());
        }

        [Fact]
        public void WhenValidatingWithMultipleValidationErrors_It_FormatsAllValidationErrors()
        {
            var update = AutoFaker.Generate<UpdateSnapshotMessage>();

            _badValidator.Validate(Arg.Any<UpdateSnapshotMessage>())
                .Returns(new ValidationResult(new List<ValidationFailure>()
                {
                        new ValidationFailure("Field1", "Field1 is invalid"),
                        new ValidationFailure("Field2", "Field2 is invalid")
                }));

            var handler = new PushValidationHandler(_badValidator);

            var result = handler.Validate(new[] { update });

            var convertedErrors = result.First().Errors.ToArray();

            Assert.Equal(2, convertedErrors.Length);
            Assert.Equal("Field1 is invalid", convertedErrors[0]);
            Assert.Equal("Field2 is invalid", convertedErrors[1]);
        }

        [Fact]
        public void WhenValidatingEmptyList_It_ReturnsEmptyList()
        {
            var emptyList = new List<UpdateSnapshotMessage>().ToArray();
            var handler = new PushValidationHandler(_validator);

            var result = handler.Validate(emptyList);

            Assert.Empty(result);
        }
    }
}
