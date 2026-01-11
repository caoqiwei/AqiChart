using AqiChart.Model.Dto;
using AqiChart.Model.Shared;
using AqiChartServer.DB.Business;
using AqiChartServer.DB.Enums;
using AqiChartServer.DB.Interface;
using AqiChartServer.WebApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AqiChartServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendController : ControllerBase
    {
        private readonly IFriendshipsBiz _friendshipsBiz;
        public FriendController(IFriendshipsBiz friendshipsBiz)
        {
            _friendshipsBiz = friendshipsBiz;
        }

        [HttpPost("GetFriends")]
        public List<UserDto> GetFriends()
        {
            List<UserDto> list = _friendshipsBiz.GetFriends(HttpContext.User.Identity.Name);
            return list;
        }

        [HttpPost("AddFriend")]
        public bool AddFriend(string friendId)
        {
            try
            {
                return _friendshipsBiz.AddFriend(HttpContext.User.Identity.Name, friendId);
            }
            catch (Exception ex)
            {
                throw new MyException(ex.Message);
            }
        }

        [HttpPost("ThroughFriend")]
        public bool ThroughFriend(string friendId)
        {
            try
            {
                return _friendshipsBiz.ThroughFriend(HttpContext.User.Identity.Name, friendId);
            }
            catch (Exception ex)
            {
                throw new MyException(ex.Message);
            }
        }

        [HttpPost("RejectFriend")]
        public bool RejectFriend(string friendId)
        {
            try
            {
                return _friendshipsBiz.RejectFriend(HttpContext.User.Identity.Name, friendId);
            }
            catch (Exception ex)
            {
                throw new MyException(ex.Message);
            }
        }

        [HttpPost("GetRejectFriends")]
        public List<UserDto> GetRejectFriends()
        {
            List<UserDto> list = _friendshipsBiz.GetFriendList(HttpContext.User.Identity.Name, FriendshipsStatusEnum.Rejected);
            return list;
        }

        [HttpPost("GetApplyFriends")]
        public List<UserDto> GetApplyFriends()
        {
            List<UserDto> list = _friendshipsBiz.GetFriendList(HttpContext.User.Identity.Name, FriendshipsStatusEnum.Pending);
            return list;
        }

    }
}
