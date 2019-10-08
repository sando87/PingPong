using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ICD
{
    public delegate stHeader DelOnRecv(stHeader msg, string info);
    public class ICDDefines
    {
        public const int CMD_NONE = 0;
        public const int CMD_MusicList = 1;
        public const int CMD_Download = 2;
        public const int CMD_Upload = 3;
        public const int CMD_NewUser = 4;
        public const int CMD_UpdateScore = 5;
        public const int CMD_GetUserInfo = 6;

        public const int ACK_REQ = 1;
        public const int ACK_REP = 2;

        public const int FILETYPE_META = 1;
        public const int FILETYPE_MUSIC = 2;
        public const int FILETYPE_IMG = 3;

        public const int MAGIC_START = 0x1234;
    }

    public class CmdTable
    {
        static public Dictionary<int, stHeader> Pairs = new Dictionary<int, stHeader>()
            {
                { ICDDefines.CMD_NONE           , new stHeader()        },
                { ICDDefines.CMD_GetUserInfo    , new CMD_UserInfo()    },
                { ICDDefines.CMD_NewUser        , new CMD_UserInfo()    },
                { ICDDefines.CMD_UpdateScore    , new CMD_UserInfo()    },
                { ICDDefines.CMD_MusicList      , new CMD_MusicList()   },
                { ICDDefines.CMD_Upload         , new CMD_FileUpload()  },
                { ICDDefines.CMD_Download       , new CMD_FileUpload()  },
            };
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class stHeader
    {
        public static DelOnRecv OnRecv;
        public HEADER head;

        public ICD.stHeader FillHeader(int cmd)
        {
            head.startMagic = 0x1234;
            head.cmd = cmd;
            head.len = Marshal.SizeOf(this);
            head.ack = ICDDefines.ACK_REQ;
            return this;
        }
        public stHeader Copy()
        {
            byte[] copyMsg = Serialize();
            Type type = GetType();
            stHeader copiedMsg = (stHeader)Activator.CreateInstance(type);
            copiedMsg.Deserialize(copyMsg);
            return copiedMsg;
        }
        virtual public byte[] Serialize()
        {
            var buffer = new byte[Marshal.SizeOf(this)];
            var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gch.AddrOfPinnedObject();
            Marshal.StructureToPtr(this, pBuffer, false);
            gch.Free();

            return buffer;
        }
        virtual public void Deserialize(byte[] data, int size = 0)
        {
            byte[] buf = data;
            if (size > 0)
            {
                byte[] tmp = new byte[size];
                Array.Copy(data, 0, tmp, 0, size);
                buf = tmp;
            }
            var gch = GCHandle.Alloc(buf, GCHandleType.Pinned);
            Marshal.PtrToStructure(gch.AddrOfPinnedObject(), this);
            gch.Free();
        }
        public bool IsValid()
        {
            return head.startMagic == ICDDefines.MAGIC_START ? true : false;
        }
        static public int HeaderSize()
        {
            return Marshal.SizeOf(typeof(stHeader));
        }
        static public stHeader Parse(byte[] buf)
        {
            int headSize = HeaderSize();
            if (buf.Length < headSize)
                return null;

            byte[] headBuf = new byte[headSize];
            Array.Copy(buf, 0, headBuf, 0, headSize);
            stHeader header = new stHeader();
            header.Deserialize(headBuf);
            int msgSize = (int)header.head.len;
            if (!header.IsValid() || buf.Length < msgSize)
                return null;

            if (!CmdTable.Pairs.ContainsKey(header.head.cmd))
                return null;

            Type objType = CmdTable.Pairs[header.head.cmd].GetType();
            stHeader obj = (stHeader)Activator.CreateInstance(objType);
            obj.Deserialize(buf);

            return obj;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_MusicList : stHeader
    {
        MusicInfos body;
        override public void Deserialize(byte[] data, int size = 0)
        {
            int count = BitConverter.ToInt32(data, HeaderSize());
            body.musics = new MusicInfo[count];
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.PtrToStructure(gch.AddrOfPinnedObject(), this);
            gch.Free();
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_UserInfo : stHeader
    {
        public UserInfo body;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_FileUpload : stHeader
    {
        public FileInfo fileInfo;
        public MusicInfo musicInfo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 1024)]
        public byte[] stream;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct HEADER
    {
        public int startMagic;
        public int cmd;
        public int len;
        public int ack;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UserInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string username;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string password;
        public int score;
        public bool unknownUser;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MusicInfo
    {
        public int id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string artist;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string userid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string fn_meta;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string fn_img;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string fn_music;
        public int playtime;
        public int beat;
        public int bpm;
        public int startOff;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MusicInfos
    {
        public int count;
        public MusicInfo[] musics;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FileInfo
    {
        public int streamSize;
        public int type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string filename;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ext;

    }
}