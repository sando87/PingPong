using PP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Utils
{
    public static byte[] Serialize(object obj)
    {
        using (var memoryStream = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, obj);
            memoryStream.Flush();
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }
    }
    public static T Deserialize<T>(byte[] buf)
    {
        using (var stream = new MemoryStream(buf))
        {
            var formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }

    static public double[] FFT(stIQ[] _iq)
    {
        if (!IsSquare(_iq.Length))
        {
            Debug.Log("FFT 변환 에러");
            return null;
        }

        //입력값에 Nosie값 추가
        System.Random rand = new System.Random();
        int cnt = _iq.Length;
        Complex[] Buf = new Complex[cnt];
        for (int i = 0; i < cnt; ++i)
        {
            double noise = (double)rand.Next(-10, 10) * 1e-5;
            Buf[i] = new Complex(_iq[i].I + noise, _iq[i].Q + noise);
        }

        //실제 FFT 수행
        Complex[] BufOut = new Complex[cnt];
        Array.Copy(Buf, BufOut, cnt);

        _FFT(Buf, 0, BufOut, 0, cnt, 1);

        //FFT 결과값을 사용자가 보기 편한 방식으로 재조정
        double maxAmp = 20.0 * Math.Log10(32768);
        double[] rets = new double[cnt];
        for (int idx = 0; idx < cnt; idx++)
        {
            double coeff = Buf[idx].Magnitude;
            //double coeff = sqrt(xn.com[idx].real * xn.com[idx].real + xn.com[idx].imag * xn.com[idx].imag);
            coeff = coeff / cnt;
            //coeff = 20.0 * Math.Log10(coeff) - maxAmp;

            rets[idx] = coeff;
        }

        //FFT 결과값을 Shift
        //shiftFFT(rets);

        return rets;
    }
    static private void _FFT(Complex[] buf, int bufOff, Complex[] bufOut, int bufOutOff, int n, int step)
    {
        if (step < n)
        {
            _FFT(bufOut, bufOutOff, buf, bufOff, n, step * 2);
            _FFT(bufOut, bufOutOff + step, buf, bufOff + step, n, step * 2);

            for (int i = 0; i < n; i += 2 * step)
            {
                double th = Math.PI * i / n;
                Complex t = (new Complex(Math.Cos(th), -Math.Sin(th))) * bufOut[bufOutOff + i + step];
                buf[bufOff + (i / 2)] = bufOut[bufOutOff + i] + t;
                buf[bufOff + ((i + n) / 2)] = bufOut[bufOutOff + i] - t;
            }
        }
    }
    static private void shiftFFT(double[] _vals)
    {
        double[] _data = _vals;
        int _cnt = _vals.Length;

        int cnt_half = _cnt / 2;
        double[] tmp_buf = new double[cnt_half];
        Array.Copy(_data, tmp_buf, cnt_half);
        Array.Copy(_data, cnt_half, _data, 0, cnt_half);
        Array.Copy(tmp_buf, 0, _data, cnt_half, cnt_half);
    }
    static private bool IsSquare(int x) //2의 거듭제곱인지 판별
    {
        return (x & (x - 1)) == 0 ? true : false;
    }
}
