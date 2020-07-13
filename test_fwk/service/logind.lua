--[[
	assert(a,b) ： 当用这个函数包括起来的时候,执行的是检查a是否有错误的参数Or报错问题。 b是在a发生错误时抛出的信息。是可以选参数
]]
local login = require "snax.loginserver"
local crypt = require "skynet.crypt"
local skynet = require "skynet"

-- 传进去的配置表
local server = {
	host = "0.0.0.1",
	port = 8001,
	multilogin = false, -- disallow multilogin
	name = "login_master"
}

local server_list = {}
local user_online = {}
local user_login = {}

-- 这个是验证接口的回调
function server.auth_handler(token)
	-- the token is base64(user)@base64(server):base64(password)
	local user, server, password = token:match("([^@]+)@([^:]+):(.+)")
	user = crypt.base64decode(user)
	server = crypt.base64decode(server)
	password = crypt.base64decode(password)
	assert(password == "password", "Invalid password")

	-- 服务内部验证完成后会回到这里,根据toke解析得到user,server,password
	-- 然后在这边的业务层验证账号密码等一系列操作

	-- 返回服务节点跟用户
	return server, user
end

-- 登录回调,用来控制登录到具体的服务器
-- server 服务id uid-- 用户名 secret-- 密钥
function server.login_handler(server, uid, secret)
	print(string.format("%s@%s is login, secret is %s", uid, server, crypt.hexencode(secret)))

	local gameserver = assert(server_list[server], "Unknown server")

	-- 这里是限制一个人登录一个uid的
	local last = user_online[uid]
	if last then
		-- 如果登录状态,这里是默认踢下线
		skynet.call(last.address, "lua", "kick", uid, last.subid)
	end

	-- 手动报错
	if user_online[uid] then
		error(string.format("user %s is already online", uid))
	end

	-- 通知服务上线
	local subid = tostring(skynet.call(gameserver, "lua", "login", uid, secret))
	
	-- 存储玩家登录的服
	user_online[uid] = {address = gameserver, subid = subid, server = server}
	return subid
end

local CMD = {}

-- 设置数据
function CMD.register_gate(server, address)
	server_list[server] = address
end

-- 玩家下线
function CMD.logout(uid, subid)
	local u = user_online[uid]
	if u then
		print(string.format("%s@%s is logout", uid, u.server))
		user_online[uid] = nil
	end
end

-- 通知分发接口的方法
function server.command_handler(command, ...)
	local f = assert(CMD[command])
	return f(...)
end

-- 手动调用登录
login(server)
