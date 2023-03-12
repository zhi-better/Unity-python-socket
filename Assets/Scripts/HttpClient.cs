using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class HttpClient : MonoBehaviour
{
    public Camera cam;
    private RenderTexture cameraView;

    // 关于网络传输的参数
    private Socket socket = null;
    private IPEndPoint endPoint = null;
    private string host = "127.0.0.1";
    private int port = 4444;

    // 连接到服务器
    public void ConnectServer()
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();//创建连接参数对象
            this.endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            args.RemoteEndPoint = this.endPoint;
            args.Completed += OnConnectedCompleted;//添加连接创建成功监听
            socket.ConnectAsync(args); //异步创建连接
        }
        catch (Exception e)
        {
            Debug.Log("服务器连接异常:" + e);
        }
    }

    private void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {   ///连接创建成功监听处理
            if (args.SocketError != SocketError.Success)
            {
                //通知上层连接失败
                Debug.Log("connection failed. ");
            }
            else
            {
                //Debug.Log("网络连接成功线程：" + Thread.CurrentThread.ManagedThreadId.ToString());
                //通知上层连接创建成功
                Debug.Log("connection success. ");
                StartReceiveMessage(); //启动接收消息

            }
        }
        catch (Exception e)
        {
            Debug.Log("开启接收数据异常" + e);
        }

    }

    public void SendToServer(byte[] data)
    {
        try
        {
            if (socket == null || !socket.Connected)
            {
                Debug.Log("socket has not been connected, send message failed. ");
                return;
            }
            //创建发送参数
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.RemoteEndPoint = endPoint;
            //设置要发送的数据
            sendEventArgs.SetBuffer(data, 0, data.Length);
            //异步发送数据
            socket.SendAsync(sendEventArgs);
        }
        catch (Exception e)
        {
            Debug.Log("发送数据异常：" + e);
        }
    }

    private void StartReceiveMessage()
    {
        //启动接收消息
        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        //设置接收消息的缓存大小，正式项目中可以放在配置 文件中
        byte[] buffer = new byte[1024 * 512];
        //设置接收缓存
        receiveArgs.SetBuffer(buffer, 0, buffer.Length);
        receiveArgs.RemoteEndPoint = this.endPoint;
        receiveArgs.Completed += OnReceiveCompleted; //接收成功
        socket.ReceiveAsync(receiveArgs);//开始异步接收监听
    }

    public void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {
            //Debug.Log("网络接收成功线程：" + Thread.CurrentThread.ManagedThreadId.ToString());

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                //创建读取数据的缓存
                byte[] bytes = new byte[args.BytesTransferred];
                //将数据复制到缓存中
                Buffer.BlockCopy(args.Buffer, 0, bytes, 0, bytes.Length);
                Debug.Log(Encoding.GetEncoding("gb2312").GetString(bytes));
//                 if (ReceiveSocketDataHandler == null)
//                 {
//                     Debug.Log("没有处理接收消息的事件 ");
//                 }
//                 //接收数据成功，调上层处理接收数据的事件
//                 ReceiveSocketDataHandler?.Invoke(bytes);
                //再次启动接收数据监听，接收下次的数据。
                StartReceiveMessage();
            }
            else
            {
                //CompleteConnectServerHandler(ConnectedStatus.Disconnect);
                ;
            }
        }
        catch (Exception e)
        {
            Debug.Log("接收数据异常：" + e);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
//         cameraView = new RenderTexture(Screen.width, Screen.height, 24);
//         cameraView.enableRandomWrite = true;
// 
//         cam.targetTexture = cameraView;
        ConnectServer();
    }

    private static byte[] strToToHexByte(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        if ((hexString.Length % 2) != 0)
            hexString += " ";
        byte[] returnBytes = new byte[hexString.Length / 2];
        for (int i = 0; i < returnBytes.Length; i++)
            returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        return returnBytes;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string dataSend = "ping";
            Debug.Log(dataSend);
            SendToServer(Encoding.GetEncoding("gb2312").GetBytes(dataSend));

        }
    }

    void OnDestory()
    {
        socket.Close();
    }
}
