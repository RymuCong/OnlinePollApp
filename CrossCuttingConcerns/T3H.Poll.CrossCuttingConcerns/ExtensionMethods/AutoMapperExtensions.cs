using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace T3H.Poll.CrossCuttingConcerns.ExtensionMethods;

public static class AutoMapperExtensions
{
    private static IMapper _mapper;
    public static IMapper RegisterMap(this IMapper mapper)
    {
        _mapper = mapper;
        return mapper;
    }

    /// <summary>
    ///     Extension method to project from a queryable using the provided mapping engine 
    /// </summary>
    /// <remarks> Projections are only calculated once and cached </remarks>
    /// <typeparam name="TDestination"> Destination type </typeparam>
    /// <param name="source">          Queryable source </param>
    /// <param name="membersToExpand"> Explicit members to expand </param>
    /// <returns>
    ///     Queryable result, use queryable extension methods to project and execute result
    /// </returns>
    public static IQueryable<TDestination> QueryTo<TDestination>(this IQueryable source, params Expression<Func<TDestination, object>>[] membersToExpand)
    {
        return source.ProjectTo(_mapper.ConfigurationProvider, null, membersToExpand);
    }

    /// <summary>
    ///     Projects the source type to the destination type given the mapping configuration 
    /// </summary>
    /// <typeparam name="TDestination"> Destination type to map to </typeparam>
    /// <param name="source">          Queryable source </param>
    /// <param name="parameters">     
    ///     Optional parameter object for parameterized mapping expressions
    /// </param>
    /// <param name="membersToExpand"> Explicit members to expand </param>
    /// <returns>
    ///     Queryable result, use queryable extension methods to project and execute result
    /// </returns>
    public static IQueryable<TDestination> QueryTo<TDestination>(this IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
    {
        return source.ProjectTo<TDestination>(_mapper.ConfigurationProvider, parameters, membersToExpand);
    }

    /// <summary>
    ///     Converts an object to another using AutoMapper library. Creates a new object of
    ///     <typeparamref name="TDestination" />. There must be a mapping between objects before
    ///     calling this method.
    /// </summary>
    /// <typeparam name="TDestination"> Type of the destination object </typeparam>
    /// <param name="source"> Source object </param>
    public static TDestination MapTo<TDestination>(this object source) where TDestination : class, new()
    {
        return _mapper.Map<TDestination>(source);
    }

    /// <summary>
    ///     Execute a mapping from the source object to the existing destination object There
    ///     must be a mapping between objects before calling this method.
    /// </summary>
    /// <typeparam name="TSource"> Source type </typeparam>
    /// <typeparam name="TDestination"> Destination type </typeparam>
    /// <param name="source">      Source object </param>
    /// <param name="destination"> Destination object </param>
    /// <returns></returns>
    public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination) where TDestination : class, new()
    {
        return _mapper.Map(source, destination);
    }

    public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        var sourceType = typeof(TSource);
        var destinationProperties = typeof(TDestination).GetProperties(flags);

        foreach (var property in destinationProperties)
        {
            var propInfoSrc = sourceType.GetProperties().FirstOrDefault(p => p.Name == property.Name);
            if (propInfoSrc == null)
                expression.ForMember(property.Name, opt => opt.Ignore());
        }
        return expression;
    }
}
