--引入或者说是创建一个skynet服务
local skynet = require "skynet"

print("main函数执行")

--调用skynet.start接口，并定义传入回调函数
skynet.start(
    function()
        skynet.error("Server First Test")
    end
)
