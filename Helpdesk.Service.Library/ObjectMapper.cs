using Helpdesk.Service.Library;
using System.Reflection;
public class ObjectMapper
{
    public static TTarget Map<TSource, TTarget>(TSource source)
        where TTarget : new()
    {
        var target = new TTarget();

        if (source == null)
            return target;

        var sourceProperties = typeof(TSource)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        var targetProperties = typeof(TTarget)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var targetProp in targetProperties)
        {
            // determine target property with custome attribute
            var mapFromAttr = targetProp.Attributes;
            
            // detect property matches by name or by MapFrom attribute
            var sourcePropName = mapFromAttr?.SourcePropertyName ?? targetProp.Name;

            // looking for a source property with the same name or specified in MapFrom attribute
            var sourceProp = sourceProperties.FirstOrDefault(sp => sp.Name == sourcePropName);

            // set target property from source property 
            if (sourceProp != null && sourceProp.PropertyType == targetProp.PropertyType)
            {
                var value = sourceProp.GetValue(source);
                if (mapFromAttr?.TypeConverter != null)
                { 
                    var converter = Activator.CreateInstance(mapFromAttr.TypeConverter) as AgeConverter;
                    value = converter?.Convert((DateTime)value);
                }
                targetProp.SetValue(target, value);
            }
        }
        
        return target;
    }
}