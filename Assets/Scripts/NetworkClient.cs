using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkClient
{
    public static NetworkClient mInst = new NetworkClient();
    TcpClient mClient = null;
    bool isRunThread = false;
    public static NetworkClient Inst() { return mInst; }

    public bool ConnectAndRecv(string ip, int port)
    {
        if (mClient != null)
            return false;

        try
        {
            mClient = new TcpClient(ip, port);
            isRunThread = true;
            Task task = new Task(new Action(RunRecieve));
            task.Start();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            return false;
        }

        return true;
    }

    void RunRecieve()
    {
        byte[] outbuf = new byte[16 * 1024];
        int nbytes;
        try
        {
            NetworkStream stream = mClient.GetStream();
            while ((nbytes = stream.Read(outbuf, 0, outbuf.Length)) > 0 && isRunThread)
            {
                byte[] recvBuf = new byte[nbytes];
                Array.Copy(outbuf, 0, recvBuf, 0, nbytes);

                ICD.stHeader msg = ICD.stHeader.Parse(recvBuf);
                if (msg == null)
                    continue;

                IPEndPoint ep = (IPEndPoint)mClient.Client.RemoteEndPoint;
                string ipAddress = ep.Address.ToString();
                int port = ep.Port;
                try
                {
                    ICD.stHeader.OnRecv(msg, ipAddress + ":" + port.ToString());
                }
                catch (Exception ex)
                { Debug.Log(ex.ToString()); }
            }
            stream.Close();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
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
    public void Close()
    {
        if (mClient == null)
            return;

        isRunThread = false;
        NetworkStream st = mClient.GetStream();
        st.Close();
        mClient.Close();
        mClient = null;
    }
}
