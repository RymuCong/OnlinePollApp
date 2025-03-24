namespace FDS.CRM.CrossCuttingConcerns.ExtensionMethods;

public static class EnumerableExtension
{
    /// <summary>
    /// Check null for a IEnumerable
    /// </summary>
    /// <typeparam name="T">Generic data</typeparam>
    /// <param name="data">True if not null, o</param>
    /// <returns>True if data not null, otherwise, false.</returns>
    public static bool IsAny<T>(this IEnumerable<T> data)
    {
        return data != null && data.Any();
    }

    public static IEnumerable<T> Page<T>(this IEnumerable<T> en, int pageSize, int page)
    {
        return en.Skip((page - 1) * pageSize).Take(pageSize);
    }

    //public static FilterModel? GetFilterModel(this List<FilterModel> data, string fieldName)
    //{
    //    return data.FirstOrDefault(x => x.FieldName == fieldName) ?? null;
    //}
}
