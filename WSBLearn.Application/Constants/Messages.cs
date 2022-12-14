namespace WSBLearn.Application.Constants
{
    public static class Messages
    {
        public const string InvalidId = "{0} with given id does not exist";
        public const string GenericErrorMessage = "Something went wrong";
    }

    public static class CrudMessages
    {
        public const string CreateEntitySuccess = "{0} has been successfully created. Id: {1}";
        public const string DeleteEntitySuccess = "{0} has been successfully deleted";
    }

    public static class Defaults
    {
        public const string ProfilePictureUrl = "https://wsblearnstorage.blob.core.windows.net/avatarcontainer/default_profilepic-3fc14e29-cce0-462a-8081-2a2399da74f2.png";

    }
}