namespace T3H.Poll.CrossCuttingConcerns.Exceptions;

public class ValidationException : Exception
{
    public static void Requires(bool expected, string errorMessage)
    {
        if (!expected)
        {
            throw new ValidationException(errorMessage);
        }
    }
    public static void NotNullOrEmpty(string value, string errorMessage)
    {
        Requires(!string.IsNullOrEmpty(value), errorMessage);
    }

    /// <summary>
    /// Kiểm tra chuỗi không được null hoặc chỉ chứa khoảng trắng
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void NotNullOrWhiteSpace(string value, string errorMessage)
    {
        Requires(!string.IsNullOrWhiteSpace(value), errorMessage);
    }

    /// <summary>
    /// Kiểm tra số nguyên phải lớn hơn 0
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void GreaterThanZero(int value, string errorMessage)
    {
        Requires(value > 0, errorMessage);
    }

    /// <summary>
    /// Kiểm tra số nguyên không được âm
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void NonNegative(int value, string errorMessage)
    {
        Requires(value >= 0, errorMessage);
    }

    /// <summary>
    /// Kiểm tra số thực phải lớn hơn 0
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void GreaterThanZero(double value, string errorMessage)
    {
        Requires(value > 0, errorMessage);
    }

    /// <summary>
    /// Kiểm tra số thực không được âm
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void NonNegative(double value, string errorMessage)
    {
        Requires(value >= 0, errorMessage);
    }

    /// <summary>
    /// Kiểm tra độ dài chuỗi nằm trong khoảng cho phép
    /// </summary>
    /// <param name="value"></param>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <param name="errorMessage"></param>
    public static void LengthInRange(string value, int minLength, int maxLength, string errorMessage)
    {
        Requires(value.Length >= minLength && value.Length <= maxLength, errorMessage);
    }

    /// <summary>
    /// Kiểm tra định dạng email cơ bản
    /// </summary>
    /// <param name="email"></param>
    /// <param name="errorMessage"></param>
    public static void ValidEmail(string email, string errorMessage)
    {
        Requires(email.Contains("@") && email.Contains("."), errorMessage);
    }

    /// <summary>
    /// Kiểm tra một chuỗi chỉ chứa chữ cái
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void OnlyLetters(string value, string errorMessage)
    {
        Requires(value.All(char.IsLetter), errorMessage);
    }

    /// <summary>
    /// Kiểm tra một chuỗi chỉ chứa chữ cái hoặc số
    /// </summary>
    /// <param name="value"></param>
    /// <param name="errorMessage"></param>
    public static void OnlyLettersOrDigits(string value, string errorMessage)
    {
        Requires(value.All(char.IsLetterOrDigit), errorMessage);
    }

    /// <summary>
    /// Kiểm tra danh sách không được rỗng
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="errorMessage"></param>
    public static void NotEmpty<T>(IEnumerable<T> collection, string errorMessage)
    {
        Requires(collection != null && collection.Any(), errorMessage);
    }

    /// <summary>
    /// Kiểm tra ngày không được là quá khứ
    /// </summary>
    /// <param name="date"></param>
    /// <param name="errorMessage"></param>
    public static void NotPastDate(DateTime date, string errorMessage)
    {
        Requires(date >= DateTime.Today, errorMessage);
    }

    /// <summary>
    /// Kiểm tra ngày phải nằm trong khoảng
    /// </summary>
    /// <param name="date"></param>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <param name="errorMessage"></param>
    public static void DateInRange(DateTime date, DateTime minDate, DateTime maxDate, string errorMessage)
    {
        Requires(date >= minDate && date <= maxDate, errorMessage);
    }

    public static bool ValidPhone(object value)
    {
        bool result = true;
        var phone = value.ToString().Replace(" ", "");
        if (string.IsNullOrEmpty(phone))
        {
            return result;
        }

        if (IsNumberString(phone))
        {
            if (phone.StartsWith("+84"))
            {
                phone = phone.Replace("+84", "0");
                result = CheckPhone(phone);
            }
            else if (phone.StartsWith("0") || phone.StartsWith("19"))
            {
                result = CheckPhone(phone);
            }
            else
            {
                result = false;
            }
        }
        else
        {
            result = false;
        }

        return result;
    }

    private static bool CheckPhone(string phone)
    {
        var listHeadPhone = new List<string>()
        {
          "086","096","097","098","032","033","034","035","036","037","038","039",// đầu số Viettel
          "089","090","093","070","079","077","076","078", // đầu số Mobi
          "088","091","094","083","084","085","081","082", // đầu số Vina
          "092","056","058", // đầu số VietnamMobile
          "099","056", "190", // đầu số Gmobile
          "087"                // Đầu số itel
        };
        var theFirstNumber = phone.Substring(0, 3);
        if (listHeadPhone.Contains(theFirstNumber) || phone.Substring(0, 2) == "02") // Check đầu số máy bàn
        {
            return true;
        }
        return false;
    }
    private static bool IsNumberString(string phone)
    {
        phone = phone.Replace("+", "");
        if (phone.Length < 10 || phone.Length > 12)
        {
            return false;
        }
        var result = phone.All(char.IsDigit);
        return result;
    }


    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
