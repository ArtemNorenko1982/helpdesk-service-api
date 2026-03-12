namespace Helpdesk.Service.Library
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapFromAttribute : Attribute
    {
        public string SourcePropertyName { get; }
        public Type? TypeConverter { get; }
        public MapFromAttribute(string sourceProperty, Type? typeConverter = default)
        {
            SourcePropertyName = sourceProperty;
            TypeConverter = typeConverter;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreMapAttribute : Attribute
    {
    }


    public class SourceClass
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public bool IsOutDated { get; set; }

        public DateTime BirthDate { get; set; }
    }

    public class TargetClass
    {
        [MapFrom("Id")]
        public int Number { get; set; }

        [MapFrom("FullName")]
        public string Title { get; set; } = string.Empty;

        [IgnoreMap]
        public bool IsOutDated { get; set; }

        [MapFrom("BirthDate", typeof(AgeConverter))]
        public int Age { get; set; }
    }

    public class Mapper
    {
        public void Map()
        {
            var entity = new SourceClass();
            var dto = ObjectMapper.Map<SourceClass, TargetClass>(entity);
        }
    }
}
