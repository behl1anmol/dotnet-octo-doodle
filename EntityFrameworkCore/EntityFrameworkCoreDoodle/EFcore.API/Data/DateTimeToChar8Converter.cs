using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFcore.API.Data;

public class DateTimeToChar8Converter : ValueConverter<DateTime, string>
{
    public DateTimeToChar8Converter() 
        : base(dateTime => dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture), 
            stringValue => DateTime.ParseExact(stringValue, "yyyyMMdd", CultureInfo.InvariantCulture))
    {
    }

}