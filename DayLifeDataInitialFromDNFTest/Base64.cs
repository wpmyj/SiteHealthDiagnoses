using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;

namespace Helpers
{
    public class Base64 {  
    private static   char[] legalChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToArray();  
    /** 
     * data[]进行编码 
     * @param data 
     * @return 
     */  
        public static String encode(byte[] data) {  
            int start = 0;  
            int len = data.Length;  
            StringBuilder buf = new StringBuilder();  
  
            int end = len - 3;  
            int i = start;  
            int n = 0;  
  
            while (i <= end) {  
                int d = ((((int) data[i]) & 0x0ff) << 16)  
                        | ((((int) data[i + 1]) & 0x0ff) << 8)  
                        | (((int) data[i + 2]) & 0x0ff);  
  
                buf.Append(legalChars[(d >> 18) & 63]);  
                buf.Append(legalChars[(d >> 12) & 63]);  
                buf.Append(legalChars[(d >> 6) & 63]);  
                buf.Append(legalChars[d & 63]);  
  
                i += 3;  
  
                if (n++ >= 14) {  
                    n = 0;  
                    buf.Append(" ");  
                }  
            }  
  
            if (i == start + len - 2) {  
                int d = ((((int) data[i]) & 0x0ff) << 16)  
                        | ((((int) data[i + 1]) & 255) << 8);  
  
                buf.Append(legalChars[(d >> 18) & 63]);  
                buf.Append(legalChars[(d >> 12) & 63]);  
                buf.Append(legalChars[(d >> 6) & 63]);  
                buf.Append("=");  
            } else if (i == start + len - 1) {  
                int d = (((int) data[i]) & 0x0ff) << 16;  
  
                buf.Append(legalChars[(d >> 18) & 63]);  
                buf.Append(legalChars[(d >> 12) & 63]);  
                buf.Append("==");  
            }  
  
            return buf.ToString();  
        }  
  
        private static int decode(char c) {  
            if (c >= 'A' && c <= 'Z')  
                return ((int) c) - 65;  
            else if (c >= 'a' && c <= 'z')  
                return ((int) c) - 97 + 26;  
            else if (c >= '0' && c <= '9')  
                return ((int) c) - 48 + 26 + 26;  
            else  
                switch (c) {  
                case '+':  
                    return 62;  
                case '/':  
                    return 63;  
                case '=':  
                    return 0;  
                default:  
                    throw new Exception ("unexpected code: " + c);  
                }  
        }  
  
        /** 
         * Decodes the given Base64 encoded String to a new byte array. The byte 
         * array holding the decoded data is returned. 
         */  
  
      

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

         
}  

}
