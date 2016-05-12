using System;

namespace IA.NetBrew.Helpers
{
    public static class StringHelper
    {

        public static String PadLeft(string str, int chars)
        {
            while (str.Length <= chars)
                str = " " + str;
            return str;
        }


        public static String PadRight(string str, int chars)
        {
            while (str.Length <= chars)
                str += " ";
            return str;
        }
    }
}
