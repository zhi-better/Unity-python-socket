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

    // �������紫��Ĳ���
    private Socket socket = null;
    private IPEndPoint endPoint = null;
    private string host = "127.0.0.1";
    private int port = 4444;

    // ���ӵ�������
    public void ConnectServer()
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();//�������Ӳ�������
            this.endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            args.RemoteEndPoint = this.endPoint;
            args.Completed += OnConnectedCompleted;//������Ӵ����ɹ�����
            socket.ConnectAsync(args); //�첽��������
        }
        catch (Exception e)
        {
            Debug.Log("�����������쳣:" + e);
        }
    }

    private void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {   ///���Ӵ����ɹ���������
            if (args.SocketError != SocketError.Success)
            {
                //֪ͨ�ϲ�����ʧ��
                Debug.Log("connection failed. ");
            }
            else
            {
                //Debug.Log("�������ӳɹ��̣߳�" + Thread.CurrentThread.ManagedThreadId.ToString());
                //֪ͨ�ϲ����Ӵ����ɹ�
                Debug.Log("connection success. ");
                StartReceiveMessage(); //����������Ϣ

            }
        }
        catch (Exception e)
        {
            Debug.Log("�������������쳣" + e);
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
            //�������Ͳ���
            SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.RemoteEndPoint = endPoint;
            //����Ҫ���͵�����
            sendEventArgs.SetBuffer(data, 0, data.Length);
            //�첽��������
            socket.SendAsync(sendEventArgs);
        }
        catch (Exception e)
        {
            Debug.Log("���������쳣��" + e);
        }
    }

    private void StartReceiveMessage()
    {
        //����������Ϣ
        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        //���ý�����Ϣ�Ļ����С����ʽ��Ŀ�п��Է������� �ļ���
        byte[] buffer = new byte[1024 * 512];
        //���ý��ջ���
        receiveArgs.SetBuffer(buffer, 0, buffer.Length);
        receiveArgs.RemoteEndPoint = this.endPoint;
        receiveArgs.Completed += OnReceiveCompleted; //���ճɹ�
        socket.ReceiveAsync(receiveArgs);//��ʼ�첽���ռ���
    }

    public void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {
            //Debug.Log("������ճɹ��̣߳�" + Thread.CurrentThread.ManagedThreadId.ToString());

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                //������ȡ���ݵĻ���
                byte[] bytes = new byte[args.BytesTransferred];
                //�����ݸ��Ƶ�������
                Buffer.BlockCopy(args.Buffer, 0, bytes, 0, bytes.Length);
                Debug.Log(Encoding.GetEncoding("gb2312").GetString(bytes));
//                 if (ReceiveSocketDataHandler == null)
//                 {
//                     Debug.Log("û�д��������Ϣ���¼� ");
//                 }
//                 //�������ݳɹ������ϲ㴦��������ݵ��¼�
//                 ReceiveSocketDataHandler?.Invoke(bytes);
                //�ٴ������������ݼ����������´ε����ݡ�
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
            Debug.Log("���������쳣��" + e);
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
