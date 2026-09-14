namespace Karmak.Integrations.Volvo.Dcds.FileUpload;

public static class ValidationConstants
{
    public const string REQUIRED_FIELD = "Field is required.";
    public const string MAX_LENGTH_5 = "Field cannot exceed 5 alphanumeric characters";
    public const string MAX_LENGTH_7 = "Field cannot exceed 7 alphanumeric characters";
    public const string MAX_LENGTH_18 = "Field cannot exceed 18 alphanumeric characters";
    public const string REQUIRED_FOR_CORRELATION = "Required for message correlation.";
    public const string REQUIRED_FOR_PARTITIONING = "Required for message partitioning.";
    public const string MUST_HAVE_DETAIL_OR_CLAIM_CHECK = "Message must have either Detail or ClaimCheck";
}