using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EralpAPI.Models.Auth
{
    public class UserResponseDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserToken { get; set; }
    }

    public class BaseResponse<T>
        where T : class, new()
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }


        public static BaseResponse<T> Success(T data)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = null
            };
        }
        
        public static BaseResponse<T> Fail(string message)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Data = null,
                Message = message
            };
        }

    }
}
