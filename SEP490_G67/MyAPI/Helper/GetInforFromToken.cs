using System.IdentityModel.Tokens.Jwt;

namespace MyAPI.Helper
{
    public class GetInforFromToken
    {
        public int GetIdInHeader(string token)
        {

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Lấy claim "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "ID");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return -1;
                }

                // Chuyển đổi id từ chuỗi sang int
                if (int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                else
                {
                    Console.WriteLine("Invalid token: ID claim is not a valid integer.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while reading token: {ex.Message}");
                return -1;
            }
        }
    }
}
