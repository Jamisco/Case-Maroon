using static CaseMaroon.Backend.BackendResponses;

namespace CaseMaroon.Backend
{
    public static class AuthManager
    {
        public static string Token { get; private set; }
        public static string Username { get; private set; }
        public static void SetToken(LoginResponse lr)
        {
            Token = lr.token;
            Username = lr.username;
        }

        public static bool HasToken()
        {
            return !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Username);
        }

        public static void ClearAuth()
        {
            Token = "";
            Username = "";
        }
    }

}
