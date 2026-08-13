<b>Base62 Encoder for .NET 8.0 or above</b>
---------
A lightweight C# utility for encoding signed 64-bit integers (long) into compact, URL-safe Base62 strings and decoding Base62 strings back into long values.

The library uses a fixed 62-character alphabet:

0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ

If you find this project useful, consider giving it a ⭐ on GitHub.

Features
Encode signed 64-bit integers (long) to Base62.

Decode Base62 strings back to long.

Support for negative numbers using a leading - sign.

URL-safe alphabet containing only:

0-9

a-z

A-Z

Input validation with detailed error codes.

Two error-handling styles:

Exception-based API

Base62Error / Try... API

Detection of invalid Base62 characters.

Overflow detection when decoding.

Defensive validation of the internal Base62 alphabet.

Extension-method syntax for convenient usage.

Quick Start
Encoding a long
<details> <summary>➕ Show example</summary>

<pre><code>using Base62; long number = 123456789; string encoded = number.ToBase62(); Console.WriteLine(encoded);</code></pre>

</details>

Decoding a Base62 string
<details> <summary>➕ Show example</summary>

<pre><code>using Base62; string encoded = "8m0Kx"; long number = encoded.FromBase62ToLong(); Console.WriteLine(number);</code></pre>

</details>

Encoding Negative Numbers
Negative values are supported.

A negative number is encoded using a leading - sign.

<details> <summary>➕ Show example</summary>

<pre><code>long number = -123456789; string encoded = number.ToBase62(); Console.WriteLine(encoded);</code></pre>

</details>

The sign is not part of the Base62 alphabet. It is handled separately from the Base62 digits.

For example:

12345

-12345

represent positive and negative values respectively.

Zero
Zero is represented by the first character of the alphabet:

0

Example:

<pre><code>string encoded = 0L.ToBase62(); Console.WriteLine(encoded); // 0</code></pre>

<details> <summary>➕ API Reference</summary>

Encoding
ToBase62
Encodes a signed 64-bit integer into a Base62 string and throws an exception if encoding fails.

<pre><code>string encoded = 123456789L.ToBase62();</code></pre>

An overload is also available when detailed error information is required:

<pre><code>string encoded = number.ToBase62(out Base62Encoder.Base62Error error);</code></pre>

TryEncode
Attempts to encode a long without throwing an exception.

<pre><code>if (Base62Encoder.TryEncode(123456789, out string base62Number)) { Console.WriteLine(base62Number); } else { Console.WriteLine("Encoding failed."); }</code></pre>

Decoding
FromBase62ToLong
Decodes a Base62 string back into a 64-bit integer and throws an exception if decoding fails.

<pre><code>long number = "8m0Kx".FromBase62ToLong();</code></pre>

An overload returning detailed error information is also available:

<pre><code>long number = Base62Encoder.FromBase62ToLong( "8m0Kx", out Base62Encoder.Base62Error error);</code></pre>

TryDecode
Attempts to decode a Base62 string without throwing an exception.

<pre><code>if (Base62Encoder.TryDecode("8m0Kx", out long number)) { Console.WriteLine(number); } else { Console.WriteLine("Decoding failed."); }</code></pre>

Positive Base62 Decoding
The following methods are available when the input is expected to contain only positive Base62 digits.

<pre><code>Base62Encoder.TryDecodePositiveBase62Number( "8m0Kx", out long number);</code></pre>

An overload is available when detailed error information is required:

<pre><code>Base62Encoder.TryDecodePositiveBase62Number( "8m0Kx", out long number, out Base62Encoder.Base62Error error);</code></pre>

A negative sign is explicitly rejected by these methods.

This makes them useful when the caller wants to separate sign handling from numeric Base62 decoding.

Validation
Base62 strings can be validated independently.

<pre><code>Base62Encoder.Base62Error error = "8m0Kx".ValidateBase62Number( out string positiveNumber, out bool isNegative); if (error.IsOK) { Console.WriteLine("Valid Base62 number."); }</code></pre>

The validation process:

Trims surrounding whitespace.

Detects an optional leading + or -.

Ensures at least one significant digit exists.

Checks every character against the Base62 alphabet.

Only a leading sign is treated as a sign.

For example:

-12345 → valid

+12345 → valid

123-45 → invalid

--12345 → invalid

12+345 → invalid

Sign Detection
The library also provides a method for detecting the sign of a Base62 number:

<pre><code>Base62Encoder.DetectSignForBase62Number( base62Number, out string positiveBase62Number, out bool isNegative);</code></pre>

The method separates the optional leading sign from the Base62 digits.

A leading + or - is treated as a sign. Any sign character appearing later in the string is treated as an invalid Base62 digit during validation.

Invalid Digit Detection
A helper method is available for locating an invalid Base62 character:

<pre><code>char? invalidDigit = Base62Encoder.GetInvalidDigitForBase62Number( positiveBase62NumberWithoutSign);</code></pre>

If all characters are valid, the method returns null.

</details>

<details> <summary>➕ Error Handling</summary>

Detailed Error Handling
The library provides a Base62Error type for callers who need more information than a simple true / false result.

For example:

<pre><code>long number = Base62Encoder.FromBase62ToLong( "invalid@value", out Base62Encoder.Base62Error error); if (error.HasError) { Console.WriteLine(error.Code); Console.WriteLine(error.Message); }</code></pre>

Each error has:

Code

Message

IsOK

HasError

<b>Supported Error Codes</b>
-----------

| Value | Error Code                                      | Description                                                             |
| ----- | -----------                                     | -----------                                                             |
|   0   | None                                            | No error                                                                |
|   1   | InvalidAlphabetLength                           | The Base62 alphabet does not contain exactly 62 characters              |
|   2   | DuplicatedAlphabetCharacter                     | The alphabet contains duplicate characters                              |
|   3   | InvalidAlphabetCharacter                        | The alphabet contains a character outside the supported ranges          |
|   4   | NullBase62String                                |   The input Base62 string is null                                       |
|   5   | NoSignificantDigitsFoundForBase62Number         |  The input is empty or contains no Base62 digits                        |
|   6   | InvalidBase62Digit                              |  The input contains a character that is not part of the Base62 alphabet |
|   7   | Base62NumberIsOutOfRangeAsALongInteger          | The decoded value exceeds the range of long                             |
|   8   | LongMinValueIsNotSupported                      | long.MinValue cannot be encoded by this implementation                  |
|   9   | NegativeSignIsNotAllowedForPositiveBase62Number | A negative sign was supplied to the positive-number decoding method     |

<b>Exception-Based API</b>

The simplest API throws an exception when the input cannot be processed.

<pre><code>try { long number = "8m0Kx".FromBase62ToLong(); Console.WriteLine(number); } catch (Exception ex) { Console.WriteLine(ex.Message); }</code></pre>

For encoding:

<pre><code>try { string base62 = 123456789L.ToBase62(); Console.WriteLine(base62); } catch (Exception ex) { Console.WriteLine(ex.Message); }</code></pre>

The Base62Error.ThrowException() method maps specific errors to appropriate exception types.

Non-Exception-Based API
For applications that prefer explicit error handling, the out Base62Error overloads can be used instead.

<pre><code>long number = Base62Encoder.FromBase62ToLong( input, out Base62Encoder.Base62Error error); if (error.IsOK) { Console.WriteLine(number); } else { Console.WriteLine(error.Message); }</code></pre>

This allows the caller to inspect the error without using exceptions for normal validation failures.

</details>

<details> <summary>➕ Base62 Alphabet</summary>

Base62 Alphabet
This implementation uses the following fixed alphabet:

<pre><code>0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ</code></pre>

Therefore:

<pre><code>Base = 62</code></pre>

Each character represents a value from 0 to 61.

Alphabet Composition
The alphabet contains:

10 digits: 0-9

26 lowercase letters: a-z

26 uppercase letters: A-Z

Total:

62 unique characters

Fixed Alphabet
The alphabet is intentionally fixed and not configurable:

<pre><code>private const string ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";</code></pre>

This guarantees that the same input always produces the same Base62 representation within this implementation.

Defensive Alphabet Validation
Although the alphabet is a constant, the implementation validates it during static initialization.

The validation checks:

Exact alphabet length (62)

Character uniqueness

Allowed character ranges

This acts as a defensive invariant check against future source-code modifications.

If the alphabet is changed incorrectly (for example: accidentally changed by the developer), the library fails fast instead of silently producing incompatible encoding or decoding results.

</details>

<details> <summary>➕ Number Handling</summary>

Zero
Zero is represented by the first character of the alphabet:

<pre><code>0</code></pre>

Example:

<pre><code>string encoded = 0L.ToBase62(); Console.WriteLine(encoded); // 0</code></pre>

Negative Numbers
Negative values are supported.

A negative number is encoded using a leading - sign:

<pre><code>-12345</code></pre>

The sign is not part of the Base62 alphabet. It is handled separately from the Base62 digits.

Leading Signs
The validation logic supports an optional leading sign.

Examples:

12345 → positive

+12345 → positive

-12345 → negative

Only the leading sign is treated as a sign.

For example:

123-45 → invalid

--12345 → invalid

12+345 → invalid

The additional sign characters are treated as invalid Base62 digits.

</details>

<details> <summary>➕ long.MinValue Limitation</summary>

long.MinValue Limitation
The current implementation intentionally does not support:

<pre><code>long.MinValue</code></pre>

That is:

<pre><code>-9223372036854775808</code></pre>

The reason is that the implementation uses Math.Abs(number) for negative values.

The absolute value of long.MinValue cannot be represented as a positive long.

Therefore:

<pre><code>long.MinValue.ToBase62();</code></pre>

results in a LongMinValueIsNotSupported error.

This behavior is explicit and documented rather than relying on an overflow or unexpected intermediate result.

</details>

<details> <summary>➕ Overflow Protection</summary>

Overflow Protection
During decoding, arithmetic is performed using checked operations:

<pre><code>number = checked(number * BASE + charIndex);</code></pre>

If the Base62 value cannot fit into a signed 64-bit integer, decoding fails with:

Base62NumberIsOutOfRangeAsALongInteger

This prevents silent integer overflow.

The output value is reset to 0 when decoding fails because of an invalid digit or overflow.

</details>

<details> <summary>➕ Example: Round Trip</summary>

Encode → Decode Round Trip
A typical encode/decode round trip looks like this:

<pre><code>long original = -987654321; string encoded = original.ToBase62(); long decoded = encoded.FromBase62ToLong(); Console.WriteLine($"Original: {original}"); Console.WriteLine($"Encoded : {encoded}"); Console.WriteLine($"Decoded : {decoded}");</code></pre>

The expected relationship is:

<pre><code>decoded == original</code></pre>

for all supported long values.

</details>

<details> <summary>➕ Design Notes</summary>

Design Notes
------------
Fixed Alphabet

The alphabet is private and intentionally non-configurable.

This guarantees consistent encoding and decoding behavior.

Defensive Alphabet Validation
Although the alphabet is a compile-time constant, the implementation validates it during static initialization.

The validation checks:

Exact alphabet length.

Character uniqueness.

Allowed character ranges.

This protects the encoding and decoding behavior from accidental future changes.

Separate Validation and Decoding
The implementation separates input validation from the actual Base62 conversion.

This makes it possible to:

Validate input independently.

Return detailed error information.

Use exception-based APIs.

Use non-exception-based APIs.

Keep the decoding algorithm focused on numeric conversion.

Extension Methods
The primary encoding and decoding methods are exposed as extension methods where appropriate, allowing concise syntax such as:

<pre><code>long value = "8m0Kx".FromBase62ToLong(); string encoded = value.ToBase62();</code></pre>

</details>

<details> <summary>➕ Why Base62?</summary>

Why Base62?

Base62 is useful when a compact, human-readable representation of an integer is needed.

Common use cases include:

* Short identifiers

* URL-safe numeric IDs

* Database record identifiers

* Compact references

* Tokens derived from integer IDs

* Reducing the visible length of large numeric identifiers: For example, a large decimal integer can often be represented using significantly fewer characters in Base62.

<b>Important Security Consideration

Base62 encoding is not encryption. The encoded value can be decoded back to the original integer. Therefore, Base62 should not be used to protect confidential information.</b>

If the encoded value is exposed publicly and the underlying integer has sensitive meaning, consider an appropriate cryptographic or authorization mechanism instead.

</details>

<details> <summary>➕ License</summary>

License
---

Copyright (c) 2026 by 'Babak Mahyar'

You are welcome to use, copy, modify, and include this code in your own projects, including commercial projects.

If you find this project useful, consider giving it a ⭐ on GitHub.

Please keep the original copyright notice and give appropriate credit to the original author when redistributing or publishing substantial portions of the code.

This software is provided "as is", without warranty of any kind. The author is not responsible for any damage, loss, or other consequences resulting from the use of this software.

</details>
