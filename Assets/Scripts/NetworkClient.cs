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

    TcpClient mClient = null;
    bool isWaitReceive = false;

    public NetworkClient() { mInst = this; }
    public void Awake()
    {
        ConnectAndRecv("127.0.0.1", 9435);
    }

    public bool ConnectAndRecv(string ip, int port)
    {
        if (mClient != null)
            return false;

        try
        {
            mClient = new TcpClient(ip, port);
            NetworkStream stream = mClient.GetStream();
            isWaitReceive = true;
            StartCoroutine(RunRecieve());

            //Task task = new Task(new Action(RunRecieve));
            //task.Start();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            return false;
        }

        return true;
    }

    IEnumerator RunRecieve()
    {
        byte[] outbuf = new byte[16 * 1024];
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
                byte[] recvBuf = new byte[nbytes];
                Array.Copy(outbuf, 0, recvBuf, 0, nbytes);

                ICD.stHeader msg = ICD.stHeader.Parse(recvBuf);
                if (msg == null)
                    continue;

                IPEndPoint ep = (IPEndPoint)mClient.Client.RemoteEndPoint;
                string ipAddress = ep.Address.ToString();
                int port = ep.Port;
                string info = ipAddress + ":" + port.ToString();
                if (mOnRecv != null)
                    mOnRecv.Invoke(msg, info);
                //ICD.stHeader.OnRecv(msg, info);
            }
            catch (Exception ex)
            { Debug.Log(ex.ToString()); }
        }
        stream.Close();
    }

    public bool SendToServer(byte[] data)
    {
        if (mClient == null)
            return false;

        try
        {
            NetworkStream stream = mClient.GetStream();
            stream.Write(data, 0, data.Length);
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
        return SendToServer(msg.Serialize());
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
