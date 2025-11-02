namespace SmartStorageBackend.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public object User { get; set; } = null!;
    }
}
