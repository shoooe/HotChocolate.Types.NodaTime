using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace HotChocolate.Types.NodaTime
{
    public class DurationType : StringBaseType<Duration>
    {
        public DurationType()
            : base("Duration")
        {
        }

        protected override string DoFormat(Duration val)
            => DurationPattern.Roundtrip
                .WithCulture(CultureInfo.InvariantCulture)
                .Format(val);

        protected override Duration DoParse(string str)
            => DurationPattern.Roundtrip
                .WithCulture(CultureInfo.InvariantCulture)
                .Parse(str).GetValueOrThrow();
    }
}