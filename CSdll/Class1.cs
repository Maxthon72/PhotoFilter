using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSdll
{
    public class Class1
    {
        unsafe public static long[] GrayscaleCSharp(byte* pCur, byte* output, int width,int hight,int Instride,int Outstride)
        {
            Stopwatch stopwatch = new Stopwatch();
            long[] time = {0,0};
            byte* pCur2 = output;
            byte* src = pCur;
            for (int i = 0; i < hight; i++)
            {
                stopwatch.Start();
                src = pCur + (i* Instride);
                pCur2 = output + (i* Outstride);
                for (int x = 0; x < width; x++)
                {
                    pCur2[x] = (byte)(src[0] * 0.2126f + src[1] * 0.7152f + src[2] * 0.0722f);
                    src += 3;
                }
                src -=3;
                stopwatch.Stop();
                if (time[0] == 0 || time[0] > stopwatch.ElapsedTicks)
                    time[0] = stopwatch.ElapsedTicks;
                if (time[1] == 0 || time[1] < stopwatch.ElapsedTicks)
                    time[1] = stopwatch.ElapsedTicks;
                stopwatch.Reset();

            }
            return time;
            
        }
        unsafe public static long NegativeCSharp(byte* pCur, byte* output, int hight, int Instride)
        {
            byte* pCur2 = output;
            byte* src = pCur;
            long time = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < hight; i++)
            {
                src = pCur + (i * Instride);
                pCur2 = output + (i * Instride);
                for (int x = 0; x < Instride; x++)
                {
                    pCur2[x] = (byte)(255-src[x]);
                }

            }
            stopwatch.Stop();
            time = stopwatch.ElapsedTicks;
            return time;

        }
    }
}
