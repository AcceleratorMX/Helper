using Helper.Domain.Entities;

namespace Helper.Domain.Service;

public class DirectoryService
{
    private static readonly string RootDirectoryName = "wwwroot/uploads".Replace('/', Path.DirectorySeparatorChar);
    private static readonly string UsersDirectoryName = "users";
    private static readonly string JobsDirectoryName = "jobs";
    private const string DefaultProfilePictureName = "default.jpg";

    public static void CreateUserDirectory(User user)
    {
        var userDirectory = Path.Combine(RootDirectoryName, UsersDirectoryName, user.Username.ToLower());

        if (!Directory.Exists(userDirectory))
        {
            Directory.CreateDirectory(userDirectory);
        }

        user.ProfilePicturePath = Path.Combine(userDirectory, DefaultProfilePictureName);
    }

    public static void CreateJobDirectory(Job job)
    {
        var jobDirectory = Path.Combine(RootDirectoryName, JobsDirectoryName, job.Id.ToString());

        if (!Directory.Exists(jobDirectory))
        {
            Directory.CreateDirectory(jobDirectory);
        }

        job.JobPicturePath = Path.Combine(jobDirectory, DefaultProfilePictureName);
    }
}

