using System.Reflection;
using SeaBattle.Infrastructure.Exceptions;

namespace SeaBattle.Domain.Helpers;

public static class DerivedInstanceCreator
{
    public static TAbstract CreateInstance<TAbstract>(string derivedTypeName) where TAbstract : class
    {
        var derivedTypes = Assembly.GetAssembly(typeof(TAbstract))!.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TAbstract)));

        var derivedType = derivedTypes.FirstOrDefault(t => t.Name.Contains(derivedTypeName));

        if (derivedType is not null)
        {
            return (TAbstract)Activator.CreateInstance(derivedType)!;
        }

        throw new DerivedInstanceException();
    }
}
