using System;
using NodaTime;

namespace HotChocolate.Types.NodaTime
{
    public class DateTimeZoneType : StringBaseType<DateTimeZone>
    {
        public DateTimeZoneType()
            : base("DateTimeZone")
        {
        }

        protected override string DoFormat(DateTimeZone val)
            => val.Id;

        protected override DateTimeZone DoParse(string str)
        {
            var result = DateTimeZoneProviders.Tzdb.GetZoneOrNull(str);
            if (result == null)
                throw new Exception();
            return result;
        }
    }
}