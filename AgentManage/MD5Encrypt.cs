using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AgentManage
{
    public class MD5Encrypt
    {
        public static string GetMD5Password(string str)
        {
            string strMD5Password = String.Empty;
            using MD5 md5 = MD5.Create();
            byte[] byteArray = md5.ComputeHash(Encoding.Unicode.GetBytes(str));
            for (int i = 0; i < byteArray.Length; i++)
            {
                strMD5Password += byteArray[i].ToString("x");
            }
            return strMD5Password;
        }
    }
}
