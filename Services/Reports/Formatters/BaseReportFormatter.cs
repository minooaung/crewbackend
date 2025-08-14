using System.Text.Json;

namespace CrewBackend.Services.Reports.Formatters
{
    public abstract class BaseReportFormatter : IReportFormatter
    {
        protected object GetPropertyValue(object obj, string propertyName)
        {
            if (propertyName == "users")
            {
                // Special handling for users collection
                var organisationUsers = obj.GetType().GetProperty("OrganisationUsers")?.GetValue(obj) as IEnumerable<dynamic>;
                if (organisationUsers == null || !organisationUsers.Any())
                {
                    return "No assigned users";
                }

                var userList = new List<string>();
                int counter = 1;
                foreach (var orgUser in organisationUsers)
                {
                    var user = orgUser.GetType().GetProperty("User")?.GetValue(orgUser);
                    if (user != null)
                    {
                        var name = user.GetType().GetProperty("Name")?.GetValue(user)?.ToString();
                        var email = user.GetType().GetProperty("Email")?.GetValue(user)?.ToString();
                        userList.Add($"{counter} - {name} ({email})");
                        counter++;
                    }
                }
                return string.Join("\n", userList);
            }

            // Handle nested properties (e.g., "Role.RoleName")
            var propertyParts = propertyName.Split('.');
            var value = obj;
            
            foreach (var part in propertyParts)
            {
                var property = value?.GetType().GetProperty(part);
                value = property?.GetValue(value);
                if (value == null) break;
            }
            
            return value ?? string.Empty;
        }

        public abstract (byte[] Content, string ContentType) Format<T>(
            string reportType,
            Dictionary<string, string> columns,
            IEnumerable<T> data
        );

        protected IEnumerable<Dictionary<string, object>> PrepareData<T>(
            Dictionary<string, string> columns,
            IEnumerable<T> data)
        {
            return data.Select(item =>
                columns.ToDictionary(
                    col => col.Key,
                    col => GetPropertyValue(item!, col.Value)
                )
            );
        }
    }
}
