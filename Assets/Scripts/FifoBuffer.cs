﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FifoBuffer
{
    private const int MAX_FIFO_SIZE = 16 * 1024 * 1024;
    private object lockObject = new object();
    private byte[] mArray = null;
    private int mHead = 0;
    private int mTail = 0;
    private int mReserve = 0;
    private int mSize = 0;

    public FifoBuffer(int reserve = MAX_FIFO_SIZE)
    {
        mReserve = reserve;
        mArray = new byte[reserve];
    }

    public void Clear()
    {
        //lock (lockObject)
        {
            Array.Clear(mArray, 0, mReserve);
            mHead = 0;
            mTail = 0;
            mSize = 0;
        }
    }

    public int GetSize()
    {
        //lock (lockObject)
        {
            return mSize;
        }
    }

    public void Push(byte[] buf, int size)
    {
        //lock (lockObject)
        {
            if (size < 0 || size > mReserve - mSize)
            {
                Clear();
                return;
            }

            int remainSize = mReserve - mTail;
            if (remainSize >= size)
            {
                Array.Copy(buf, 0, mArray, mTail, size);
                mTail += size;
                mSize += size;
            }
            else
            {
                Array.Copy(buf, 0, mArray, mTail, remainSize);
                Array.Copy(buf, remainSize, mArray, 0, size - remainSize);
                mTail = size - remainSize;
                mSize += size;
            }
        }
    }

    public byte[] Pop(int size)
    {
        //lock (lockObject)
        {
            if (size < 0 || mSize < size)
            {
                Clear();
                return null;
            }

            byte[] dest = new byte[size];
            int remainSize = mReserve - mHead;
            if (remainSize >= size)
            {
                Array.Copy(mArray, mHead, dest, 0, size);
                mHead += size;
                mSize -= size;
            }
            else
            {
                Array.Copy(mArray, mHead, dest, 0, remainSize);
                Array.Copy(mArray, 0, dest, remainSize, size - remainSize);
                mHead = size - remainSize;
                mSize -= size;
            }

            return dest;
        }
    }

    public byte[] readSize(int size, int offset = 0)
    {
        //lock (lockObject)
        {
            int realSize = offset + size;
            if (size < 0 || offset < 0 || mSize < realSize)
            {
                Clear();
                return null;
            }

            byte[] dest = new byte[size];
            int offHead = (mHead + offset) % mReserve;
            int remainSize = mReserve - offHead;
            if (remainSize >= size)
            {
                Array.Copy(mArray, offHead, dest, 0, size);
            }
            else
            {
                Array.Copy(mArray, offHead, dest, 0, remainSize);
                Array.Copy(mArray, 0, dest, remainSize, size - remainSize);
            }

            return dest;
        }
    }

    public byte this[int index]
    {
        get
        {
            byte[] buf = readSize(1, index);
            if (buf == null)
                return 0;
            return buf[0];
        }
    }

    public short readShort(int index)
    {
        byte[] buf = readSize(2, index * 2);
        if (buf == null)
            return 0;
        return (short)(buf[0] | (buf[1] << 8));
    }

    public int readInt(int index)
    {
        byte[] buf = readSize(4, index * 4);
        if (buf == null)
            return 0;
        return buf[0] | (buf[1] << 8) | (buf[2] << 16) | (buf[3] << 24);
    }
}
