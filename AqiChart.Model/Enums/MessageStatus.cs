using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqiChart.Model.Enums
{
    public enum MessageStatus
    {
        Sending,        // 发送中
        Sent,           // 已发送
        Delivered,      // 已送达
        Read,           // 已读
        Failed          // 发送失败
    }
}
