namespace Helpdesk.Service.Library
{
    public interface ITypeConverter<TSource, TTarget>
        where TSource : notnull
    {
        TTarget Convert(TSource source);
    }

    public interface ITypeConverter
    {
        int Convert(DateTime source);
    }
}
