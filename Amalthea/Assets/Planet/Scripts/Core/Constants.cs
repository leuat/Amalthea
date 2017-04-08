using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class Constants 
    {
        public static double Luminosity0 = 3.828E26;
        public static double boltzmannSigma = 5.670373E-8;
        public static double AU = 149597870700;
        public static double sunR = 695500;
        public static double massEarth = 5.972E24;
        public static double massSun = 1.989E30;
        public static double secondsPerYear = 31556926;
        public static double G = 6.67E-11;//×10−11 m3 kg−1 s−2
        public static float LumFromT(double R, float T)
        {
//            return (float)(boltzmannSigma / Mathf.PI * Mathf.Pow(T, 4));
            return (float)(4*Mathf.PI*boltzmannSigma*R*R * Mathf.Pow(T, 4));
        }

        public static string getFormattedMass(double val)
        {
            if (val >= 10E17 && val < 10E27)
                return "" + (val / massEarth).ToString("0.0000") +" Earths";
            if (val >= 10E27 && val < 10E35)
                return "" + (val / massSun).ToString("0.0000") + " Sols";

            return "" + val + " kg";

        }

        public static string getFormattedTimeFromSeconds(double s) {

            int m = (int)(s / 60);
            int h = (int)(m / 60);
            int d = (int)h / 24;
            int mn = (int)d / 30;
            int y = (int)mn / 12;
            s = s % 60;
            m = m % 60;
            h = h % 24;
            d = d % 30;
            mn = mn % 12;


            string str = "";
            if (y != 0)
                str += y + " years ";
            if (mn != 0)
                str += mn + " months ";
            if (d != 0)
                str += d + " days ";
            if (y == 0)
            {
                if (h != 0)
                    str += h + "h ";
                if (m != 0)
                    str += m + "m ";
            }
//            str += QString::number(s) + "." + QString::number(ds) + "s ";
            return str;
        }


        public static float getBlackBodyTemperature(float L, float D, float A)
        {
//            float D = (float)(D * Constants.AU);
            return Mathf.Pow((float)(L * (1 - A) / (16 * Mathf.PI * D * D * Constants.boltzmannSigma)), 0.25f);
        }

        public static Color getRGBFromTemperature(float T)
        {
            double tmpCalc;
            T = Mathf.Clamp(T, 1000, 40000);
            T = T / 100;
            Color c = Color.black;
            if (T <= 66)
                c.r = 1;
            else
            {
                tmpCalc = T - 60;
                tmpCalc = 329.698727446 * (Mathf.Pow((float)tmpCalc, -0.1332047592f));
                c.r = Mathf.Clamp((float)tmpCalc/255f,0,1);
            }
            if (T<66)
            {
                tmpCalc = T;
                tmpCalc = 99.4708025861 * Mathf.Log((float)tmpCalc) - 161.1195681661f;
                c.g = Mathf.Clamp((float)tmpCalc/255f, 0, 1);

            }
            else
            {
                tmpCalc = T - 60;
                tmpCalc = 288.1221695283 * Mathf.Pow((float)tmpCalc, -0.0755148492f);
                c.g = Mathf.Clamp((float)tmpCalc / 255f, 0, 1);
            }
            if (T >= 66)
            {
                c.b = 1;
            }
            else
            if (T <= 19)
                c.b = 0;
            else
            {
                tmpCalc = T - 10;
                tmpCalc = 138.5177312231 * Mathf.Log((float)tmpCalc) - 305.0447927307f;
                c.b = Mathf.Clamp((float)tmpCalc / 255f, 0, 1);
            }
            return c;
        }

    }

}