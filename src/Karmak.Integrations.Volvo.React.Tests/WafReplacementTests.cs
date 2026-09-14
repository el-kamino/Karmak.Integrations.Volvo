using Karmak.Integrations.Volvo.React.Utils;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.Utils
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("\t\n", "\t\n")]
        public void WafSanitize_NullOrWhiteSpace_ReturnsInput(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("..", "")]
        [InlineData("...", "")]
        [InlineData("....", "")]
        [InlineData("../", "")]
        [InlineData("..\\", "")]
        [InlineData("%2e%2e", "")]
        [InlineData("%2E%2E", "")]
        [InlineData("  ..  ", "")]
        [InlineData(".", "")]
        [InlineData("-", "")]
        [InlineData("---", "")]
        [InlineData("...", "")]
        [InlineData(" - . - ", "")]
        public void WafSanitize_PathTraversalOrPlaceholder_ReturnsEmpty(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test\x00 data", "test data")]
        [InlineData("test\x01\x02\x03", "test")]
        [InlineData("\x08hello\x1F", "hello")]
        [InlineData("normal\x0Btext\x0C", "normaltext")]
        [InlineData("clean text", "clean text")]
        public void WafSanitize_ControlCharacters_StripsControlChars(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("SELECT * FROM users", "SELECT * FROM users")]
        [InlineData("UNION SELECT password", "UNION SELECT password")]
        [InlineData("DROP TABLE users", "DROP TABLE users")]
        [InlineData("OR 1=1", "OR 1=1")]
        [InlineData("'; DROP TABLE users--", "' DROP TABLE users--")]
        [InlineData("test;--comment", "test--comment")]
        [InlineData("xp_cmdshell", "xp_cmdshell")]
        public void WafSanitize_SqlInjectionPatterns_StripsDangerousChars(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("<script>alert('xss')</script>", "scriptalert('xss')/script")]
        [InlineData("<img src=x onerror=alert(1)>", "img src=x onerror=alert(1)")]
        [InlineData("<iframe src='evil.com'></iframe>", "iframe src='evil.com'/iframe")]
        [InlineData("  <  script  >  ", "  script  ")]
        public void WafSanitize_XssPatterns_StripsDangerousChars(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("normal text", "normal text")]
        [InlineData("user@example.com", "user@example.com")]
        [InlineData("123-456-7890", "123-456-7890")]
        [InlineData("Product Name (2024)", "Product Name (2024)")]
        [InlineData("  trimmed  ", "trimmed")]
        public void WafSanitize_CleanInput_ReturnsCleanTrimmed(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("'; UNION SELECT <script>", "' UNION SELECT script")]
        [InlineData("../../../etc/passwd\x00", "")]
        [InlineData("<img onerror=alert(1)>'; DROP TABLE users--", "img onerror=alert(1)' DROP TABLE users--")]
        public void WafSanitize_CombinedAttacks_SanitizesAll(string input, string expected)
        {
            // Act
            var result = input.WafSanitize();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}