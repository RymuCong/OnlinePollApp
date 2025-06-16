using System.Security.Claims;
using T3H.Poll.Application.Common.DTOs;
using T3H.Poll.CrossCuttingConcerns.Common.Interfaces.Registers;

namespace T3H.Poll.Application.Common.Token;

public interface ITokenFactory : ISingleton
{
    // thời gian hết hạn của AccessToken
    public DateTime AccesstokenExpiredTime { get; }   
    // RefreshToken cũng là 1 chuỗi token đi kèm với lần đăng nhập đầu tiên và sử dụng cho việc cấp phát lại 
    // AccessToken sau khi AccessToken hết hạn
    public DateTime RefreshtokenExpiredTime { get; }  // thời gian hết hạn của  RefreshToken
    
    /// <summary>
    /// Giải mã, phân tích cái token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    DecodeTokenResponse DecodeToken(string token);   

    /// <summary>
    /// Tạo ra JWT token
    /// </summary>
    /// <param name="claims">Danh sách các quyền, các thông tin của user đăng nhập</param>
    /// <param name="expirationTime">Thời gian hệ hạn</param>
    /// <returns></returns>
    string CreateToken(IEnumerable<Claim> claims, DateTime expirationTime);
}
