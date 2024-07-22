using System.ComponentModel;

namespace AbsenceNotifier.Core.Helpers
{
    public static class EnumsHelper
    {
        public static T? GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        return (T)field.GetValue(null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                else
                {
                    if (field.Name == description)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        return (T)field.GetValue(null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
            }

            throw new ArgumentException("Enum Not found.", nameof(description));
        }
    }
}
