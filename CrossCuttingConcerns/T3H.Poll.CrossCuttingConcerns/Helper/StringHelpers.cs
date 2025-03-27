using T3H.Poll.CrossCuttingConcerns.ExtensionMethods;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace T3H.Poll.CrossCuttingConcerns.Helper
{
    public static class StringHelpers
    {
        private static Random random = new Random();
        public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
        public const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string NumberChars = "0123456789";
        public const string SpecialChars = "!@#$%^*&";

        public static string IncrementAlphaNumericValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (Regex.IsMatch(value, "^[a-zA-Z0-9]+$") == false)
            {
                throw new Exception("Invalid Character: Must be a-Z or 0-9");
            }

            var characterArray = value.ToCharArray();

            for (var characterIndex = characterArray.Length - 1; characterIndex >= 0; characterIndex--)
            {
                var characterValue = Convert.ToInt32(characterArray[characterIndex]);

                if (characterValue != 57 && characterValue != 90 && characterValue != 122)
                {
                    characterArray[characterIndex]++;

                    for (int resetIndex = characterIndex + 1; resetIndex < characterArray.Length; resetIndex++)
                    {
                        characterValue = Convert.ToInt32(characterArray[resetIndex]);
                        if (characterValue >= 65 && characterValue <= 90)
                        {
                            characterArray[resetIndex] = 'A';
                        }
                        else if (characterValue >= 97 && characterValue <= 122)
                        {
                            characterArray[resetIndex] = 'a';
                        }
                        else if (characterValue >= 48 && characterValue <= 57)
                        {
                            characterArray[resetIndex] = '0';
                        }
                    }

                    return new string(characterArray);
                }
            }

            return string.Empty;
        }
        /// <summary>
        ///     Throw ArgumentNullException is value is Null or empty or whitespace 
        /// </summary>
        /// <param name="values"></param>
        public static void CheckNullOrWhiteSpace(params string[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            string[] arr1 =
            {
            "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "đ", "é", "è", "ẻ",
            "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ",
            "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý",
            "ỳ", "ỷ", "ỹ", "ỵ",
        };
            string[] arr2 =
            {
            "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "d", "e", "e", "e",
            "e", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o",
            "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y",
            "y", "y", "y", "y",
        };

            for (var i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }

            return text;
        }

        public static string SetFilterField(List<string> filerText)
        {
            if (!filerText.IsAny())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var text in filerText)
            {
                if (!string.IsNullOrEmpty(text))
                    stringBuilder.Append(RemoveUnicode(text.ToUpper())).Append("$$");
            }
            return stringBuilder.ToString();
        }

        public static string FormatMoney(decimal money, string currency)
        {
            if (money == 0) return "0";

            if (currency == "VND")
                return String.Format("{0:0,0}", money) + " " + currency;
            else
                return String.Format("{0:0,0.00}", money) + " " + currency;
        }

    
        public static string GetRandomString(int length,
            bool isIncludeChars = true,
            bool isIncludeUpperChars = true,
            bool isIncludeLowerChars = true,
            bool isIncludeNumbers = true,
            bool isIncludeSpecialChars = false)
        {
            var characters = string.Empty;
            if (isIncludeChars)
            {
                if (isIncludeUpperChars)
                {
                    characters += UpperChars;
                }

                if (isIncludeLowerChars)
                {
                    characters += LowerChars;
                }
            }

            if (isIncludeNumbers)
            {
                characters += NumberChars;
            }

            if (isIncludeSpecialChars)
            {
                characters += SpecialChars;
            }

            return GetRandomString(length, characters);
        }

        public static string GetRandomString(int length, string characters)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be less than zero.");
            }

            if (string.IsNullOrWhiteSpace(characters))
            {
                throw new ArgumentOutOfRangeException(nameof(characters), "characters invalid.");
            }

            const int byteSize = 0x100;
            if (byteSize < characters.Length)
            {
                throw new ArgumentException(
                    string.Format("{0} may contain no more than {1} characters.", nameof(characters), byteSize),
                    nameof(characters));
            }

            var outOfRangeStart = byteSize - (byteSize % characters.Length);
            using (var rng = RandomNumberGenerator.Create())
            {
                var sb = new StringBuilder();
                var buffer = new byte[128];
                while (sb.Length < length)
                {
                    rng.GetBytes(buffer);
                    for (var i = 0; i < buffer.Length && sb.Length < length; ++i)
                    {
                        // Divide the byte into charSet-sized groups. If the random value falls into
                        // the last group and the last group is too small to choose from the entire
                        // allowedCharSet, ignore the value in order to avoid biasing the result.
                        if (outOfRangeStart <= buffer[i])
                        {
                            continue;
                        }

                        sb.Append(characters[buffer[i] % characters.Length]);
                    }
                }

                return sb.ToString();
            }
        }
        public static string GenerateRememberMeToken()
        {
            return Guid.NewGuid().ToString();
        }
        public static string GetRandomColor()
        {
            List<string> colors = new List<string>
        {
            "#FF5733", "#33FF57", "#3357FF", "#F1C40F", "#8E44AD",
            "#3498DB", "#E74C3C", "#2ECC71", "#1ABC9C", "#D35400",
            "#C0392B", "#27AE60", "#2980B9", "#9B59B6", "#16A085",
            "#F39C12", "#7F8C8D", "#BDC3C7", "#2C3E50", "#E67E22"
        };
            return colors[random.Next(colors.Count)];
        }

        public static string GenerateIncrementalString(int number, int length)
        {
            string numberStr = number.ToString();
            return numberStr.PadLeft(length, '0');
        }

        public static string[] SplitAddrressString(string input, int maxLength)
        {
            List<string> parts = new List<string>();
            string remaining = input;

            while (!string.IsNullOrEmpty(remaining))
            {
                maxLength = remaining.Length > maxLength ? maxLength : remaining.Length;

                if (remaining.Length - 2 < 29)
                {
                    parts.Add(remaining);
                    break;
                }
                else
                {
                    int splitIndex = remaining.LastIndexOf(',', maxLength);
                    if (splitIndex == -1)
                    {
                        splitIndex = remaining.LastIndexOf(' ', maxLength);
                    }
                    if (splitIndex >= 0 && splitIndex < remaining.Length)
                    {
                        parts.Add(remaining.Substring(0, splitIndex + 1).Trim());
                        remaining = remaining.Substring(splitIndex + 1).Trim();
                    }
                    else
                    {
                        parts.Add(remaining);
                        break;
                    }
                }
            }
            return parts.ToArray();
        }

        public static string SplitCHDRNUM(string input)
        {
            int splitIndex = input.IndexOf('-', 0);

            return splitIndex > 0 ? input.Substring(0, splitIndex).Trim() : input;
        }

        public static string FormatNumberWithLeadingZeros(decimal number, int totalLength)
        {
            string formattedNumber = number.ToString("F0", CultureInfo.InvariantCulture);
            formattedNumber = formattedNumber.Replace(".", "").PadLeft(totalLength, '0');
            return formattedNumber;
        }

        public static string FormatDecimalNumberWithLeadingZeros(decimal number, int left, int right)
        {
            string formattedNumber = number.ToString("F0", CultureInfo.InvariantCulture);
            string formattedLeft = formattedNumber.Split(".")[0].PadLeft(left, '0');
            string formattedRight = formattedNumber.Split(".").Count() == 1 ? "".PadLeft(right, '0') : formattedNumber.Split(".")[1].PadLeft(right, '0');
            return formattedLeft + "." + formattedRight;
        }

        public static string GetLastPartOfPath(string input)
        {
            int lastBackslashIndex = input.LastIndexOf("FileUpload") + 11;
            if (lastBackslashIndex >= 0)
            {
                return input.Substring(lastBackslashIndex);
            }
            return input; // Trường hợp không tìm thấy dấu gạch chéo ngược
        }

        public static List<string> StandardizedEmailInput(string input)
        {
            List<string> result = new List<string>();

            // Tách chuỗi thành các địa chỉ email
            string[] emails = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

            // Xử lý từng địa chỉ email
            foreach (var email in emails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    // Loại bỏ khoảng trắng và thêm vào danh sách
                    result.Add(email.Trim());
                }

            }

            return result;
        }

        public static string GenerateEmailConfirmationToken()
        {
            // Sử dụng RNGCryptoServiceProvider để tạo các số ngẫu nhiên an toàn
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                // Độ dài của mã token, có thể thay đổi theo nhu cầu
                int tokenLength = 32;

                // Sử dụng mảng byte để lưu trữ token ngẫu nhiên
                byte[] tokenBytes = new byte[tokenLength];

                // Sử dụng RNGCryptoServiceProvider để tạo token ngẫu nhiên
                cryptoProvider.GetBytes(tokenBytes);

                // Chuyển đổi token ngẫu nhiên thành dạng chuỗi hex
                string token = BitConverter.ToString(tokenBytes).Replace("-", "").ToLower();

                return token;
            }
        }
        public static string EncodeToBase64(string plainText)
        {
            // Chuyển chuỗi plain text thành byte array
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            // Mã hóa byte array thành chuỗi base64
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeFromBase64(string base64EncodedData)
        {
            // Chuyển chuỗi base64 về byte array
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

            // Giải mã byte array thành chuỗi plain text
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
