using System.Diagnostics;
using System.Text;
using static Base62.Base62Encoder.Base62Error;

namespace Base62;

/// <summary>
/// Provides utility methods to encode signed 64-bit integers into
/// short, URL-safe Base62 strings and decode Base62 strings back
/// into long integers.
/// </summary>

public static class Base62Encoder
{
    public class Base62Error(ErrorCode code, params object[] parameters)
    {
        public object[] Parameters { get; } = parameters;
        public ErrorCode Code { get; } = code;
        public enum ErrorCode
        {
            None = 0,
            InvalidAlphabetLength = 1,
            DuplicatedAlphabetCharacter = 2,
            InvalidAlphabetCharacter = 3,
            NullBase62String = 4,
            NoSignificantDigitsFoundForBase62Number = 5,
            InvalidBase62Digit = 6,
            Base62NumberIsOutOfRangeAsALongInteger = 7,
            LongMinValueIsNotSupported = 8,
            NegativeSignIsNotAllowedForPositiveBase62Number = 9
        }
        public string Message
        {
            get
            {
                return Code switch
                {
                    ErrorCode.None => "",
                    ErrorCode.InvalidAlphabetLength => $"Critical Error: Alphabet length is {GetParameter(0)}, but it must be exactly {GetParameter(1)}.",
                    ErrorCode.DuplicatedAlphabetCharacter => "Critical Error: Alphabet contains duplicate characters. Every character must be unique.",
                    ErrorCode.InvalidAlphabetCharacter => $"Critical Error: Alphabet contains invalid character '{GetParameter(0)}'.",
                    ErrorCode.NullBase62String => "Base62 text is null.",
                    ErrorCode.NoSignificantDigitsFoundForBase62Number => "Base62 text is empty or Base62 number has no significant digits.",
                    ErrorCode.InvalidBase62Digit => $"Invalid character '{GetParameter(0)}' encountered in Base62 string.",
                    ErrorCode.Base62NumberIsOutOfRangeAsALongInteger => "Base62 number is out of range for a long integer.",
                    ErrorCode.LongMinValueIsNotSupported => $"long.MinValue is not supported in '{OWNER_CLASS_NAME}' class.",
                    ErrorCode.NegativeSignIsNotAllowedForPositiveBase62Number => "Negative sign is not allowed for positive Base62 number",
                    _ => "",
                };
            }
        }
        
        public const string OWNER_CLASS_NAME = 
            nameof(Base62Encoder);
 
        public static readonly Base62Error OK =
            new(ErrorCode.None);

        private const string UNKNOWN_PARAMETER = "?";

        protected string GetParameter(int parameterIndex)
        {
            // If null is used as parameter,
            // For example: new Base62Error(ErrorCode.InvalidAlphabetCharacter, null)

            if (Parameters == null)
                return UNKNOWN_PARAMETER;

            bool isOutOfRangeIndex = parameterIndex < 0 || parameterIndex >= Parameters.Length;
            return isOutOfRangeIndex ? UNKNOWN_PARAMETER : Parameters[parameterIndex].ToString() ?? UNKNOWN_PARAMETER;
        }
        public bool IsOK
        {
            get
            {
                return Code == ErrorCode.None;
            }
        }
        public bool HasError
        {
            get
            {
                return Code != ErrorCode.None;
            }
        }
        [StackTraceHidden]
        public void ThrowException()
        {
            if (HasError)
            {
                throw Code switch
                {
                    ErrorCode.LongMinValueIsNotSupported => new ArgumentOutOfRangeException("number", long.MinValue, Message),
                    ErrorCode.NullBase62String => new ArgumentNullException("base62Number", Message),
                    _ => new FormatException(Message),
                };
            }
        }
    }

    /*
        **** Base62 alphabet containing URL-safe characters ****

        The Base62 alphabet is intentionally fixed and not configurable. ValidateAlphabet() acts as a defensive invariant check 
        to ensure that future modifications to the source code cannot silently introduce unexpected encoding or decoding behavior. 
        The library fails fast if the alphabet is changed incorrectly.    
    */

    public const bool THROW_EXCEPTION = true;

    /// <summary>
    /// The fixed Base62 alphabet used by Base62Encoder.
    /// The alphabet is not configurable.
    /// </summary>
    public const string ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static readonly int BASE = ALPHABET.Length;
    private const int EXPECTED_LENGTH = 62;

    private static Base62Error ValidateAlphabet(bool throwException = !THROW_EXCEPTION)
    {
        var error = OK;

        if (ALPHABET.Length != EXPECTED_LENGTH)
            error = new Base62Error(ErrorCode.InvalidAlphabetLength, ALPHABET.Length, EXPECTED_LENGTH);
        else
        {
            // Detecting duplicate characters:

            var uniqueChars = new HashSet<char>(ALPHABET);
            if (uniqueChars.Count != EXPECTED_LENGTH)
                error = new Base62Error(ErrorCode.DuplicatedAlphabetCharacter);
        }
        if (error.IsOK)
        {
            foreach (var alphabetMember in ALPHABET)
            {
                if (!(
                    (alphabetMember >= '0' && alphabetMember <= '9') ||
                    (alphabetMember >= 'a' && alphabetMember <= 'z') ||
                    (alphabetMember >= 'A' && alphabetMember <= 'Z')
                    ))
                {
                    error = new Base62Error(ErrorCode.InvalidAlphabetCharacter, alphabetMember);
                    break;
                }
            }
        }
        if (throwException && error.HasError)
            error.ThrowException();
        return error;
    }

    static Base62Encoder()
    {
        ValidateAlphabet(THROW_EXCEPTION);
    }


    /// <summary>
    /// Encodes a signed 64-bit integer into a URL-safe Base62 number.
    /// </summary>
    public static string ToBase62(this long number, out Base62Error error)
    {
        error = OK;
        if (number == 0)
            return ALPHABET[0].ToString();

        bool isNegative = number < 0;
        if (number == long.MinValue)
        {
            error = new Base62Error(ErrorCode.LongMinValueIsNotSupported);
            return "";
        }
        number = Math.Abs(number);
        var result = new StringBuilder();

        while (number != 0)
        {
            var charIndex = (int)(number % BASE);
            result.Append(ALPHABET[charIndex]);
            number /= BASE;
        }

        if (isNegative)
            result.Append('-');

        return ReverseString(result.ToString());
    }
    private static string ReverseString(string text)
    {
        char[] charArray = text.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
    public static string ToBase62(this long number)
    {
        var result = ToBase62(number, out Base62Error error);
        if (error.HasError)
            error.ThrowException();
        return result;
    }

    public static void DetectSignForBase62Number(string base62Number, out string positiveBase62Number, out bool isNegative)
    {
        isNegative = false;
        base62Number = base62Number?.Trim() ?? "";
        positiveBase62Number = base62Number;
        if (!string.IsNullOrWhiteSpace(base62Number))
        {
            isNegative = base62Number.StartsWith('-');
            var hasPlusSign = base62Number.StartsWith('+');
            if (isNegative || hasPlusSign)
                positiveBase62Number = base62Number[1..];
        }
    }
    public static char? GetInvalidDigitForBase62Number(string positiveBase62NumberWithoutSign)
    {
        positiveBase62NumberWithoutSign = positiveBase62NumberWithoutSign?.Trim() ?? "";
        for (int index = 0; index < positiveBase62NumberWithoutSign.Length; index++)
            if (ALPHABET.IndexOf(positiveBase62NumberWithoutSign[index]) < 0)
            return positiveBase62NumberWithoutSign[index];
        return null;
    }
    /// <summary>
    /// Validate a Base62 number
    /// </summary>
    public static Base62Error ValidateBase62Number(this string base62Number, out string positiveBase62Number, out bool isNegative, bool throwException = !THROW_EXCEPTION)
    {
        var result = OK;
        if (base62Number == null)
        {
            positiveBase62Number = string.Empty;
            isNegative = false;
            result = new Base62Error(ErrorCode.NullBase62String);
        }
        else
        {
            DetectSignForBase62Number(base62Number, out positiveBase62Number, out isNegative);

            if (string.IsNullOrEmpty(positiveBase62Number))
                result = new Base62Error(ErrorCode.NoSignificantDigitsFoundForBase62Number);
            else
            {
                // Only a leading sign is treated as a sign; any other sign character is treated as an invalid digit.
                // For example, "123-5" and "--12345" result in ErrorCode.InvalidBase62Digit.

                var invalidBase62Digit = GetInvalidDigitForBase62Number(positiveBase62Number);
                if (invalidBase62Digit != null)
                    result = new Base62Error(ErrorCode.InvalidBase62Digit, invalidBase62Digit);
            }
        }
        if (throwException && result.HasError)
            result.ThrowException();
        return result;
    }

    /// <summary>
    /// Decodes a Base62 string back into a 64-bit integer number.
    /// </summary>
    public static long FromBase62ToLong(this string base62Number)
    {
        base62Number.ValidateBase62Number(out string positiveBase62Number, out bool isNegative, THROW_EXCEPTION);

        // If there is an error, the exception thrown by the above call will prevent current method from continuing.

        if (TryDecodePositiveBase62Number(positiveBase62Number, out long number, out Base62Error error))
            return isNegative ? -number : number;
        error.ThrowException();
        return 0;
    }
     /// <summary>
    /// Decodes a Base62 string back into a 64-bit integer number.
    /// </summary>
    public static long FromBase62ToLong(string base62Number, out Base62Error error)
    {
        error = base62Number.ValidateBase62Number(out string positiveBase62Number, out bool isNegative, !THROW_EXCEPTION);
        if (error.IsOK && TryDecodePositiveBase62Number(positiveBase62Number, out long number))
            return isNegative ? -number : number;
        return 0;
    }

    /// <summary>
    /// Tries to decode a Base62 number into a 64-bit integer.
    /// </summary>
    public static bool TryDecodePositiveBase62Number(string positiveBase62Number, out long number)
    {
        return TryDecodePositiveBase62Number(positiveBase62Number, out number, out _);
    }
    /// <summary>
    /// Tries to decode a positive Base62 number into a 64-bit integer.
    /// </summary>
    public static bool TryDecodePositiveBase62Number(string positiveBase62Number, out long number, out Base62Error error)
    {
        error = OK;
        number = 0;
        if (positiveBase62Number == null)
        {
            error = new Base62Error(ErrorCode.NullBase62String);
            return false;
        }
        positiveBase62Number = positiveBase62Number.Trim();
        if (positiveBase62Number == string.Empty)
        {
            error = new Base62Error(ErrorCode.NoSignificantDigitsFoundForBase62Number);
            return false;
        }
        if (positiveBase62Number.StartsWith('-'))
        {
            error = new Base62Error(ErrorCode.NegativeSignIsNotAllowedForPositiveBase62Number);
            return false;
        }
        for (int i = 0; i < positiveBase62Number.Length; i++)
        {
            int charIndex = ALPHABET.IndexOf(positiveBase62Number[i]);
            if (charIndex < 0)
            {
                number = 0;
                error = new Base62Error(ErrorCode.InvalidBase62Digit, positiveBase62Number[i]);
                return false;
            }
            try
            {
                number = checked(number * BASE + charIndex);
            }
            catch (OverflowException)
            {
                number = 0;
                error = new Base62Error(ErrorCode.Base62NumberIsOutOfRangeAsALongInteger);
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Tries to decode a Base62 number into a 64-bit integer.
    /// </summary>
    public static bool TryDecode(string base62Number, out long number)
    {
        number = FromBase62ToLong(base62Number, out Base62Error error);
        return error.IsOK;
    }

    /// <summary>
    /// Tries to encode a signed 64-bit integer into a Base62 number.
    /// </summary>
    public static bool TryEncode(long number, out string base62Number)
    {
        base62Number = ToBase62(number, out Base62Error error);
        return error.IsOK;
    }
}
