SERVERDIR="SERVER"
AUTHPORT=2220
GAMEPORT=2221
DATABASE="dev2"
Cur_Dir="/root/SkyNetTest/config"
sky_dir="/root/skynet"

export SERVERDIR
export AUTHPORT
export GAMEPORT
export DATABASE

sky_dir="$Cur_Dir/../../skynet"

echo $sky_dir
cd $sky_dir

$sky_dir/skynet $Cur_Dir/config
