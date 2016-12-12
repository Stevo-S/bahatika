using AverageBid.SMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace AverageBid.Models
{
    public class OutboundMessage
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Destination { get; set; }

        public string Message { get; set; }

        [StringLength(50)]
        public string ServiceId { get; set; }

        [StringLength(100)]
        public string LinkId { get; set; }

        [StringLength(50)]
        public string Correlator { get; set; }

        public DateTime Timestamp { get; set; }

        public string Send()
        {
            string timestampString = Timestamp.ToString("yyyyMMddHHmmss");
            string password = ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + timestampString);
            using (var handler = new HttpClientHandler() { Credentials = new NetworkCredential(ConnectionDetails.GetUsername(), password) })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(ConnectionDetails.GetRemoteSmsServiceUrl());

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/SendSmsService/services/SendSms/");
                request.Content = new StringContent(buildSMSXML(timestampString: timestampString), Encoding.UTF8, "text/xml");
                string requestContentString = request.Content.ReadAsStringAsync().Result;
                var result = client.SendAsync(request).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;

                return resultContent;
            }
        }

        private string buildSMSXML(string serviceId = "6013252000121151", string sender = "20090", string timestampString = "")
        {
            string password = ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + timestampString);
            XNamespace soapenv = SOAPRequestNamespaces.Soapenv;
            XNamespace v2 = SOAPRequestNamespaces.V2;
            XNamespace loc = SOAPRequestNamespaces.LocSend;
            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "v2", v2.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header",
                        new XElement(v2 + "RequestSOAPHeader",
                            new XElement(v2 + "spId", ConnectionDetails.GetSpID()),
                            new XElement(v2 + "spPassword", password),
                            new XElement(v2 + "serviceId", serviceId),
                            new XElement(v2 + "timeStamp", timestampString),
                            new XElement(v2 + "linkid", LinkId),
                            new XElement(v2 + "OA", "tel:" + Destination),
                            new XElement(v2 + "FA", "tel:" + Destination)
                        ), // End of RequestSOAPHeader
                    new XElement(soapenv + "Body",
                        new XElement(loc + "sendSms",
                            new XElement(loc + "addresses", "tel:" + Destination),
                            new XElement(loc + "senderName", sender),
                            new XElement(loc + "message", Message),
                            new XElement(loc + "receiptRequest",
                                new XElement("endpoint", "http://" + ConnectionDetails.GetHostPPPAddress() + "/BarefootPower/api/ReceiveDeliveryNotification"),
                                new XElement("interfaceName", "SmsNotification"),
                                new XElement("correlator", Id.ToString("D5"))
                            ) // End of receiptRequest
                        ) // End of sendSms
                    ) // End of Soap Body
                ) // End of Soap Header
            ); // End of Soap Envelope

            return soapEnvelope.ToString();
        }
    }
}