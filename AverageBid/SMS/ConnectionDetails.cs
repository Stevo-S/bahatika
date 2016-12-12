using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AverageBid.SMS
{
    public class ConnectionDetails
    {
        private static string username = ConfigurationManager.AppSettings["SPUserName"];
        private static string password = ConfigurationManager.AppSettings["SPPassword"];
        private static string HostPPPAddress = ConfigurationManager.AppSettings["ServerPPPIPAddress"];
        private static string RemoteSmsServiceUrl = ConfigurationManager.AppSettings["RemoteSMSServiceURI"];
        private static string spID = ConfigurationManager.AppSettings["SPID"];

        public static string GetUsername()
        {
            return username;
        }

        public static string GetPassword()
        {
            return password;
        }

        public static string GetSpID()
        {
            return spID;
        }

        public static string GetHostPPPAddress()
        {
            return HostPPPAddress;
        }


        public static string HashPassword(string input)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(s => s.ToString("x2")));
        }

        public static string GetRemoteSmsServiceUrl()
        {
            return RemoteSmsServiceUrl;
        }
    }
}