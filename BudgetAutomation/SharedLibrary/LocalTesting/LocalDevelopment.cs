namespace SharedLibrary.LocalTesting;

public static class LocalDevelopment
{
    public static string Prefix(bool localDevelopment) => localDevelopment ? "dev-" : "";
}