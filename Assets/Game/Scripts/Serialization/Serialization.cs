public class Serialization
{
    public const int VERSION = 1;
    public const string PATH = "DefenseFiles";
    public const string FILE_EXTENSION = ".btw";

    public enum ErrorCode
    {
        FileNotFound,
        VersionInvalid,
        Unknown
    }
}
