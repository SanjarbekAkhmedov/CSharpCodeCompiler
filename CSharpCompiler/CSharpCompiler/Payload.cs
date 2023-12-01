using System.Text.RegularExpressions;

namespace CSharpCompiler
{
    public static class Payload
    {
        public static string GetPayload(this Student student, string payload)
        {
            var properties = Regex.Matches(payload, @"@(\w+)");

            foreach (Match property in properties)
            {
                var propertyName = property.Groups[1].Value;
                var propertyValue = GetPropertyValue(student, propertyName);
                payload = payload.Replace($"@{propertyName}", $"\"{propertyValue}\"");
            }

            return payload;
        }

        private static string GetPropertyValue(Student student, string propertyName)
        {
            switch (propertyName)
            {
                case "Id":
                    return student.Id.ToString();
                case "Name":
                    return student.Name;
                default:
                    throw new ArgumentException($"Property {propertyName} not found in the Student class.");
            }
        }
    }
}
