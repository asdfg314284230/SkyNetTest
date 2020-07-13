--引入或者说是创建一个skynet服务
local skynet = require "skynet"

--调用skynet.start接口，并定义传入回调函数
skynet.start(
    function()
        local loginserver = skynet.newservice("logind")
        local gate = skynet.newservice("gated", loginserver)

        skynet.call(
            gate,
            "lua",
            "open",
            {
                address = "0.0.0.1",
                port = 8888,
                maxclient = 64,
                servername = "sample"
            }
        )

        
    end
)
