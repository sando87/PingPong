﻿using System;
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
                { ICDDefines.CMD_Upload         , new CMD_SongFile()  },
                { ICDDefines.CMD_Download       , new CMD_SongFile()  },
            };
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class stHeader
    {
        public static DelOnRecv OnRecv;
        public HEADER head = new HEADER();

        public ICD.stHeader FillHeader(int cmd)
        {
            head.startMagic = 0x1234;
            head.cmd = cmd;
            head.len = TotalSize();
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
        virtual public int TotalSize()
        {
            return Marshal.SizeOf(this);
        }
        public bool IsValid()
        {
            return head.startMagic == ICDDefines.MAGIC_START ? true : false;
        }
        static public int HeaderSize()
        {
            return Marshal.SizeOf(typeof(stHeader));
        }
        static public stHeader Parse(byte[] buf, ref bool isError)
        {
            isError = false;
            int headSize = HeaderSize();
            if (buf.Length < headSize)
                return null;

            byte[] headBuf = new byte[headSize];
            Array.Copy(buf, 0, headBuf, 0, headSize);
            stHeader header = new stHeader();
            header.Deserialize(headBuf);
            int msgSize = (int)header.head.len;
            if (!header.IsValid())
            {
                isError = true;
                return null;
            }

            if (buf.Length < msgSize)
                return null;

            if (!CmdTable.Pairs.ContainsKey(header.head.cmd))
            {
                isError = true;
                return null;
            }

            Type objType = CmdTable.Pairs[header.head.cmd].GetType();
            stHeader obj = (stHeader)Activator.CreateInstance(objType);
            obj.Deserialize(buf);

            return obj;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_MusicList : stHeader
    {
        public List<Song> musics = new List<Song>();
        public override int TotalSize()
        {
            return HeaderSize() + Marshal.SizeOf(typeof(Song)) * musics.Count;
        }
        override public byte[] Serialize()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(Utils.Serialize(head));
            for (int i = 0; i < musics.Count; ++i)
                buffer.AddRange(Utils.Serialize(musics[i]));

            return buffer.ToArray();
        }
        override public void Deserialize(byte[] data, int size = 0)
        {
            Utils.Deserialize(ref head, data, HeaderSize());
            int songSize = Marshal.SizeOf(typeof(Song));
            int count = (data.Length - HeaderSize()) / songSize;
            musics = new List<Song>();
            for(int i = 0; i < count; ++i)
            {
                Song song = new Song();
                byte[] tmpBuf = new byte[songSize];
                Array.Copy(data, HeaderSize() + i * songSize, tmpBuf, 0, songSize);
                Utils.Deserialize(ref song, tmpBuf);
                musics.Add(song);
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_UserInfo : stHeader
    {
        public UserInfo body = new UserInfo();
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    class CMD_SongFile : stHeader
    {
        public Song song = new Song();
        public List<byte> stream = new List<byte>();
        public override int TotalSize()
        {
            return HeaderSize() + Marshal.SizeOf(typeof(Song)) + stream.Count;
        }
        override public byte[] Serialize()
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(Utils.Serialize(head));
            buffer.AddRange(Utils.Serialize(song));
            buffer.AddRange(stream.ToArray());
            return buffer.ToArray();
        }
        override public void Deserialize(byte[] data, int size = 0)
        {
            Utils.Deserialize(ref head, data, HeaderSize(), 0);
            Utils.Deserialize(ref song, data, Marshal.SizeOf(song), HeaderSize());
            int count = data.Length - HeaderSize() - Marshal.SizeOf(song);
            byte[] tmp = new byte[count];
            Array.Copy(data, HeaderSize() + Marshal.SizeOf(song), tmp, 0, count);
            stream = new List<byte>();
            stream.AddRange(tmp);
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class HEADER
    {
        public int startMagic;
        public int cmd;
        public int len;
        public int ack;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class UserInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string username;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string password;
        public int score;
        public bool unknownUser;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class Song
    {
        public int DBID;
        public int BPM;
        public float JumpDelay;
        public PP.BeatType Beat;
        public PP.FileType Type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string UserID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string FilePath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string FileNameNoExt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Artist;
        public int Grade;
        public int BarCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public Bar[] Bars;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Bar
    {
        public bool Main;
        public bool Half;
        public bool PreHalf;
        public bool PostHalf;
    }

}