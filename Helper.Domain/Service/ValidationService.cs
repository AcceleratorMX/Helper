namespace Helper.Domain.Service;

public class ValidationService
{
    private const string AdminId = "fa3701af-a9ad-4f79-9a9f-3331a46f694f";

    public static bool IsAdmin(string adminId) => string.Equals(adminId, AdminId, StringComparison.OrdinalIgnoreCase);
}