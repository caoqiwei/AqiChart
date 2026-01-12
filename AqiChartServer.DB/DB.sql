CREATE TABLE chat_users (
    user_id VARCHAR(50) PRIMARY KEY, 
    
    -- 用户身份信息
    username VARCHAR(50) NOT NULL UNIQUE COMMENT '唯一用户名',
    password_hash VARCHAR(255) NOT NULL COMMENT 'BCrypt加密后的密码',
    email VARCHAR(100) NOT NULL COMMENT '验证过的邮箱',
    phone VARCHAR(15) COMMENT '国际区号+号码（可选）',

    -- 用户资料
    avatar_url VARCHAR(255) DEFAULT '/Resources/Images/avatar.png' COMMENT '头像CDN地址',
		nickname VARCHAR(50) DEFAULT '' COMMENT '用户显示名称（可重复）',
    status ENUM('online', 'offline', 'away') DEFAULT 'offline' COMMENT '实时状态',

    -- 时间追踪
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '注册时间',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '最后更新时间',
    last_online TIMESTAMP NULL COMMENT '最后在线时间',

    -- 索引优化
    INDEX idx_email (email),
    INDEX idx_phone (phone),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user1','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user1@xx.com', '🇨🇳小明同学✨');


INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user2','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user2@xx.com', '小白');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user3','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user3@xx.com', '↑⑤的阳光');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user4','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user4@xx.com', '↑⑤的阳光');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user5','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user5@xx.com', '网络迷航者');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user6','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user6@xx.com', '到底是个');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user7','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user7@xx.com', '水电费');

INSERT INTO chat_users (user_id, username,password_hash,email, nickname) 
VALUES (UUID(),'user8','100000:J/Lxo8pqFAaP8XtW2vWmmUdpECx9wlLNLfFQ/0xClOs=:Ey1mxijGynGR5sf0vhZ8r3HPT0SkptnxQyaiDcPWXJc=','user8@xx.com', '房管局');



-- 好友关系表
CREATE TABLE friendships (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id1 VARCHAR(50) NOT NULL,
    user_id2 VARCHAR(50) NOT NULL,
    status ENUM('Apply','Pending', 'Accepted', 'Rejected') DEFAULT 'Pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '添加时间'
);

select * from friendships;


CREATE TABLE private_chat (
    -- 消息唯一标识
    message_id VARCHAR(50) PRIMARY KEY,
    
    -- 通信双方
    sender_id VARCHAR(50) NOT NULL COMMENT '发送方UUID',
    receiver_id VARCHAR(50) NOT NULL COMMENT '接收方UUID',
    
    -- 消息内容
    content TEXT NOT NULL COMMENT '加密消息体（JSON格式）',
    content_type ENUM('text', 'image', 'file') DEFAULT 'text',
    file_metadata JSON COMMENT '文件元数据 {"name":"file.pdf","size":5242880,"hash":"sha256:..."}',
    
    -- 状态控制
    is_read TINYINT(1) DEFAULT 0 COMMENT '0=未读，1=已读',
    is_recalled TINYINT(1) DEFAULT 0 COMMENT '0=正常，1=撤回',
    
    -- 时间管理
    created_at TIMESTAMP(3) DEFAULT CURRENT_TIMESTAMP(3) NOT NULL,
    updated_at TIMESTAMP(3) DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    
    -- 关系约束
    FOREIGN KEY (sender_id) REFERENCES chat_users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (receiver_id) REFERENCES chat_users(user_id) ON DELETE CASCADE,
    
    -- 复合索引优化
    INDEX idx_conversation (sender_id, receiver_id, created_at),
    INDEX idx_reverse_conversation (receiver_id, sender_id, created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- 启用事件调度器
SET GLOBAL event_scheduler = ON;
-- 创建定时事件，每分钟检查一次
CREATE EVENT auto_update_user_status
ON SCHEDULE EVERY 1 MINUTE
STARTS CURRENT_TIMESTAMP
DO
BEGIN
    UPDATE chat_users 
    SET status = 'offline'
    WHERE status = 'online' 
    AND last_online < NOW() - INTERVAL 10 MINUTE;
END;
