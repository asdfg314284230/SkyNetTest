using Skynet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Cline : MonoBehaviour
{
    private Socket clientSocket;

    byte[] s_secret;
    string s_subid;
    int player_id;
    string server;
    string user;
    string token;
    int session = 0;
    string pass;
    int index = 1;

    [SerializeField]
    Button button;

    [SerializeField]
    Button send_button;

    // Start is called before the first frame update
    void Start()
    {
        send_button.onClick.AddListener(() => {
            send_request("213123", 201);
        });

        button.onClick.AddListener(() => {
            server = "sample";
            user = "123";
            pass = "password";
            token = Crypt.Base64Encode(Encoding.UTF8.GetBytes(user))
                + "@" + Crypt.Base64Encode(Encoding.UTF8.GetBytes(server))
                + ":" + Crypt.Base64Encode(Encoding.UTF8.GetBytes(pass));


            string host = "120.55.192.125";//服务器端ip地址
            int port = 8001;


            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);


            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(ipe);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.SocketErrorCode);
                return;
            }


            var s = readline();
            var challenge = Crypt.Base64Decode(s);
            var clientkey = Crypt.RandomKey();
            writeline(Crypt.DHExchange(clientkey));

            s = readline();
            var serverkey = Crypt.Base64Decode(s);
            var secret = Crypt.DHSecret(serverkey, clientkey);
            var hmac = Crypt.HMAC64(challenge, secret);
            writeline(hmac);

            var etoken = Crypt.DesEncode(secret, Encoding.UTF8.GetBytes(token));
            writeline(etoken);


            var result = readline();
            var code = result.Substring(0, 3);
            var subid = Crypt.Base64Decode(result.Substring(4, result.Length - 4));



            s_secret = secret;
            s_subid = Encoding.UTF8.GetString(subid);



            clientSocket.Close();

            reset();
        });
      

    }



    void reset()
    {
        // 这里应该连接网关了
        string host = "120.55.192.125";//服务器端ip地址
        int port = 8888;

        IPAddress ip = IPAddress.Parse(host);
        IPEndPoint ipe = new IPEndPoint(ip, port);

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            clientSocket.Connect(ipe);
        }
        catch (SocketException se)
        {
            Console.WriteLine(se.SocketErrorCode);
            return;
        }

        handshake();


        // 在考虑是否需要开启一个循环监听

    }

    string readline()
    {
        byte[] buff = new byte[1024];
        clientSocket.Receive(buff);
        string s = Encoding.UTF8.GetString(buff);
        int i = s.IndexOf("\n");
        s = s.Substring(0, i);
        return s;
    }


    void handshake()
    {
        string handshake = Crypt.Base64Encode(Encoding.UTF8.GetBytes(user))
            + "@" + Crypt.Base64Encode(Encoding.UTF8.GetBytes(server))
            + "#" + Crypt.Base64Encode(Encoding.UTF8.GetBytes(s_subid))
            + ":" + 1;

        var hmac = Crypt.HMAC64(Crypt.HashKey(Encoding.UTF8.GetBytes(handshake)), s_secret);

        var d = handshake + ":" + Crypt.Base64Encode(hmac);
        
        int len = d.Length;
        
        var buff = new byte[2 + len];

        byte[] bData = BitConverter.GetBytes((Int16)len);
        
        if (BitConverter.IsLittleEndian) // 若为 小端模式
        {
            Array.Reverse(bData); // 转换为 大端模式               
        }

        BinaryWriter br = new BinaryWriter(new MemoryStream(buff));
        
        br.Write(bData);
        
        br.Write(Encoding.UTF8.GetBytes(d));
        
        clientSocket.Send(buff);


        byte[] b = new byte[1024];
        
        clientSocket.Receive(b);
        
        string s = Encoding.UTF8.GetString(b);

    }


    public void send_request(string v, int msg_id)
    {
        session++;
        var size = v.Length + 4;
        var buff = new byte[2 + size];

        byte[] bsize = BitConverter.GetBytes((Int16)size);
        //byte[] op_id = BitConverter.GetBytes((Int16)2);
        //byte[] bmsg_id = BitConverter.GetBytes((Int16)msg_id);
        byte[] bsession = BitConverter.GetBytes((UInt32)session);

        if (BitConverter.IsLittleEndian) // 若为 小端模式
        {
            Array.Reverse(bsize); // 转换为 大端模式       
            //Array.Reverse(op_id); // 转换为 大端模式   
            //Array.Reverse(bmsg_id); // 转换为 大端模式 
            Array.Reverse(bsession); // 转换为 大端模式 
        }


        BinaryWriter br = new BinaryWriter(new MemoryStream(buff));

        br.Write(bsize);
        //br.Write(op_id);
        //br.Write(bmsg_id);
        br.Write(Encoding.UTF8.GetBytes(v));
        br.Write(bsession);

        try
        {
            clientSocket.Send(buff);
        }
        catch (SocketException se)
        {
            Console.WriteLine(se.SocketErrorCode);
            return;
        }

    }


    void writeline(Byte[] text)
    {
        var s = Crypt.Base64Encode(text);
        s = s + "\n";
        byte[] byteArray = Encoding.UTF8.GetBytes(s);
        clientSocket.Send(byteArray);
    }

}
