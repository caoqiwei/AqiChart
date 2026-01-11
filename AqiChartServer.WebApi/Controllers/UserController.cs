using AqiChart.Model.Dto;
using AqiChartServer.DB.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AqiChartServer.WebApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserBiz _userBiz;

        public UserController(IUserBiz userBiz)
        {
            _userBiz = userBiz;
        }

        [HttpPost("GetUserList")]
        public List<UserDto> GetUserList()
        {
            List<UserDto> list = _userBiz.GetUserList();
            return list;
        }

        [HttpPost("UpdateLastUserTime")]
        public bool UpdateLastUserTime()
        {
            return _userBiz.UpdateLastUserTime(HttpContext.User.Identity.Name);
        }

        [HttpPost("SearchUserList")]
        public List<UserDto> SearchUserList(SearchDto dto) 
        {
            return _userBiz.SearchUserList(dto.Search, HttpContext.User.Identity.Name);
        }

    }
}
