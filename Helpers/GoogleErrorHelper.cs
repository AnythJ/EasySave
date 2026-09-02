using System.Net;

namespace EasySave.Helpers
{
    public static class GoogleErrorHelper
    {
        public static string Describe(Google.GoogleApiException ex)
        {
            return ex.HttpStatusCode switch
            {
                HttpStatusCode.Unauthorized => "not authorized, re-login required",
                HttpStatusCode.Forbidden => "access denied or storage quota exceeded",
                HttpStatusCode.NotFound => "file not found on Drive (may have been deleted)",
                _ => ex.Message
            };
        }
    }
}
