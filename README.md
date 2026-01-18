# AqiChart
用WPF Caliburn.Micro + Web.API  SignalR socket 做的一个仿微信即时聊天软件；

# 截图功能
截图功能会全局注册一个快捷键

# 数据库
项目代码中用的是mysql 数据库，由于用户名中有表情符号所以数据库版本需要时8.0以上，Sql中有注册一个事件来处理用户意外退出导致数据库用户状态一直是在线状态；
AqiChartServer.DB 中Sql文件中有数据库的表和基本的用户数据，用户密码都是加密的，加密字符串中的密码是123456;

