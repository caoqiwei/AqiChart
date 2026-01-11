using AqiChart.Model.Dto;
using AqiChartServer.DB.Interface;
using AqiChartServer.WebApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AqiChartServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrivateChatController : ControllerBase
    {
        private readonly IPrivateChatBiz _privateChatBiz;
        public PrivateChatController(IPrivateChatBiz privateChatBiz)
        {
            _privateChatBiz = privateChatBiz;
        }

        /// <summary>
        /// 获取所有好友未读取信息
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        [HttpPost("GetAllUnreadChart")]
        public List<PrivateChatDto> GetAllUnreadChart()
        {
            return _privateChatBiz.GetAllUnreadByUserId(HttpContext.User.Identity.Name);
        }

        /// <summary>
        /// 获取单个好友未读取信息
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        [HttpPost("GetUnreadChart")]
        public List<PrivateChatDto> GetUnreadChart(string friendId)
        {
            return _privateChatBiz.GetUnreadUserChart(HttpContext.User.Identity.Name, friendId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="MyException"></exception>
        [HttpPost("SetReadById")]
        public bool SetReadById(string id)
        {
            try
            {
                return _privateChatBiz.SetReadById(id);
            }
            catch (Exception ex)
            {
                throw new MyException(ex.Message);
            }
        }

        [HttpPost("SetReadByFriendId")]
        public bool SetReadByFriendId(string friendId)
        {
            try
            {
                return _privateChatBiz.SetReadByFriendId(HttpContext.User.Identity.Name, friendId);
            }
            catch (Exception ex)
            {
                throw new MyException(ex.Message);
            }
        }

    }
}
