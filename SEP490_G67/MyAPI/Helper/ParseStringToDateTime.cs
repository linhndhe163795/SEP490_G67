namespace MyAPI.Helper
{
    public class ParseStringToDateTime
    {
        public DateTime ParseToDateTime(string date, TimeSpan? time) 
         {
            var dateTimeString = $"{date} {time}";
            if (DateTime.TryParse(dateTimeString, out DateTime timeParse))
            {
                return timeParse;
            }
            else
            {
                throw new Exception();
            }


        }
    }
}
