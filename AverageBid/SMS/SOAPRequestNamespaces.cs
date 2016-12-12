using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AverageBid.SMS
{
    public class SOAPRequestNamespaces
    {
        private static string soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
        private static string v2 = "http://www.huawei.com.cn/schema/common/v2_1";
        private static string locSend = "http://www.csapi.org/schema/parlayx/sms/send/v2_2/local";
        private static string locNotification = "http://www.csapi.org/schema/parlayx/sms/notification/v2_2/local";
        private static string locSmsNotification = "http://www.csapi.org/schema/parlayx/sms/notification_manager/v2_3/local";
        private static string locSync = "http://www.csapi.org/schema/parlayx/data/sync/v1_0/local";
        private static string ns1 = "http://www.huawei.com.cn/schema/common/v2_1";
        private static string ns2 = "http://www.csapi.org/schema/parlayx/sms/notification/v2_2/local";

        public static string Soapenv { get { return soapenv; } }
        public static string V2 { get { return v2; } }
        public static string LocSend { get { return locSend; } }
        public static string LocNotification { get { return locNotification; } }
        public static string LocSmsNotification { get { return locSmsNotification; } }
        public static string LocSync { get { return locSync; } }
        public static string Ns1 { get { return ns1; } }
        public static string Ns2 { get { return ns2; } }
    }
}