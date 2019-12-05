using ICD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NetworkClient : MonoBehaviour
{
    [Serializable]
    public class NetworkEvent : UnityEvent<ICD.stHeader, string> { }
    public NetworkEvent mOnRecv = null;

    public static NetworkClient mInst = null;
    public static NetworkClient Inst() { return mInst; }
    public const int PACKET_SIZE = 16 * 1024;

    TcpClient mClient = null;
    bool isWaitReceive = false;
    FifoBuffer mFifoBuffer = new FifoBuffer();

    public NetworkClient() { mInst = this; }
    public void Awake()
    {
        //ConnectAndRecv("27.117.158.3", 9435);
        //ConnectAndRecv("119.82.53.97", 9435);
        ConnectAndRecv("sjleeserver.iptime.org", 9435);
    }
    public bool IsConnected() { return mClient == null ? false : true; }
    public bool ConnectAndRecv(string ip, int port)
    {
        if (mClient != null)
            return false;

        try
        {
            mClient = new TcpClient();
            var result = mClient.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            if (!success)
                throw new Exception("Failed to connect.");

            mClient.EndConnect(result);

            NetworkStream stream = mClient.GetStream();
            isWaitReceive = true;
            StartCoroutine(RunRecieve());

            //Task task = new Task(new Action(RunRecieve));
            //task.Start();
        }
        catch (Exception ex)
        {
            mClient = null;
            Debug.Log(ex.ToString());
            return false;
        }

        return true;
    }

    IEnumerator RunRecieve()
    {
        byte[] outbuf = new byte[PACKET_SIZE];
        int nbytes = 0;
        NetworkStream stream = mClient.GetStream();
        while (isWaitReceive)
        {
            yield return new WaitForEndOfFrame();
            if (!stream.DataAvailable)
                continue;

            try
            {
                nbytes = stream.Read(outbuf, 0, outbuf.Length);
                mFifoBuffer.Push(outbuf, nbytes);

                while(true)
                {
                    byte[] buf = mFifoBuffer.readSize(mFifoBuffer.GetSize());
                    bool isError = false;
                    ICD.stHeader msg = ICD.stHeader.Parse(buf, ref isError);
                    if (isError)
                        mFifoBuffer.Clear();

                    if (msg == null)
                        break;
                    else
                        mFifoBuffer.Pop(msg.head.len);

                    IPEndPoint ep = (IPEndPoint)mClient.Client.RemoteEndPoint;
                    string ipAddress = ep.Address.ToString();
                    int port = ep.Port;
                    string info = ipAddress + ":" + port.ToString();
                    if (mOnRecv != null)
                        mOnRecv.Invoke(msg, info);
                }
            }
            catch (Exception ex)
            { Debug.Log(ex.ToString()); }
        }
        mFifoBuffer.Clear();
        stream.Close();
    }

    public bool SendToServer(byte[] data)
    {
        if (mClient == null)
            return false;

        try
        {
            NetworkStream stream = mClient.GetStream();
            int currentSize = data.Length;
            int off = 0;
            do
            {
                int sendSize = Math.Min(currentSize, PACKET_SIZE);
                stream.Write(data, off, sendSize);
                off += sendSize;
                currentSize -= sendSize;
            } while (currentSize > 0);
            
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            return false;
        }
        return true;
    }
    public bool SendMsgToServer(ICD.stHeader msg)
    {
        if (mClient == null)
            return false;

        if(msg.head.len > PACKET_SIZE * 32)
            StartCoroutine("SendToServerAsync", msg.Serialize());
        else
            SendToServer(msg.Serialize());

        return true;
    }
    IEnumerator SendToServerAsync(byte[] data)
    {
        LoadingBar loadingbar = LoadingBar.Show();
        NetworkStream stream = mClient.GetStream();
        int currentSize = data.Length;
        int off = 0;
        do
        {
            int sendSize = Math.Min(currentSize, PACKET_SIZE);
            try { stream.Write(data, off, sendSize); }
            catch (Exception ex) { Debug.Log(ex.ToString()); break; }
            off += sendSize;
            currentSize -= sendSize;
            loadingbar.SetProgress(off / (float)data.Length);
            yield return new WaitForEndOfFrame();
        } while (currentSize > 0);
        loadingbar.Hide();
    }
    public float GetProgressState()
    {
        int currentSize = mFifoBuffer.GetSize();
        if (currentSize < stHeader.HeaderSize())
            return 1f;

        HEADER head = new HEADER();
        byte[] headBuf = mFifoBuffer.readSize(stHeader.HeaderSize());
        Utils.Deserialize(ref head, headBuf);
        return currentSize / (float)head.len;
    }
    public bool IsRecvData()
    {
        return mFifoBuffer.GetSize() > 0 ? true : false;
    }
    public void Close()
    {
        if (mClient == null)
            return;

        isWaitReceive = false;
        NetworkStream st = mClient.GetStream();
        st.Close();
        mClient.Close();
        mClient = null;
    }
}
