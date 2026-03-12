namespace Helpdesk.Service.Library
{
    public class AgeConverter
    {
        public int Convert(DateTime dateTime) =>
            DateTime.Now.Year - dateTime.Year;
    }
}
