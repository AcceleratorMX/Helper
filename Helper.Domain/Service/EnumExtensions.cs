using System.ComponentModel.DataAnnotations;
using Helper.Domain.Entities.Abstract;

namespace Helper.Domain.Service;

public static class EnumExtensions
{
    public static string GetDisplayName(this string value)
    {
        if (!Enum.TryParse(value, out JobStatuses status)) return value;
        var fieldInfo = status.GetType().GetField(status.ToString());
        var descriptionAttributes = fieldInfo!.GetCustomAttributes(
            typeof(DisplayAttribute), false) as DisplayAttribute[];

        if (descriptionAttributes != null && descriptionAttributes.Length != 0)
        {
            return descriptionAttributes.First().Name!;
        }

        return value;
    }
}
