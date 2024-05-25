using System.Text.RegularExpressions;

namespace Helper.Domain.Service;

public class ValidationService
{
    private const string AdminId = "fa3701af-a9ad-4f79-9a9f-3331a46f694f";

    public bool IsAdmin(string adminId) => string.Equals(adminId, AdminId, StringComparison.OrdinalIgnoreCase);
    
    public bool IsHasMoreSpaces(string input, int limit)
    {
        var regex = new Regex($"[ ]{{{limit},}}");
        return regex.IsMatch(input);
    }
}