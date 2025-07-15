namespace T3H.Poll.Application.Common;

public static class Utils
{
    public static bool IsHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(ICommandHandler<>) || 
               genericType == typeof(ICommandHandler<,>) ||
               genericType == typeof(IQueryHandler<,>);
    }
}
