using AverageBid.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace AverageBid.SMS
{
    public class SMSMessage
    {
        public string Destination { get; set; }
        public string Text { get; set; }
        public string Correlator { get; set; }
        public string TimeStamp { get; set; }
        public string Sender { get; set; }
        public string ServiceId { get; set; }

        public string ToXML()
        {
            return buildSMSXML();
        }

        private string buildSMSXML()
        {
            //string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            XNamespace soapenv = SOAPRequestNamespaces.LocSend;
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
                            new XElement(v2 + "spPassword", ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + TimeStamp))),
                            new XElement(v2 + "serviceId", ServiceId),
                            new XElement(v2 + "timeStamp", TimeStamp)//,
                                                                     //new XElement(v2 + "linkid"),
                                                                     //new XElement(v2 + "OA", "tel:" + message.GetDestination()),
                                                                     //new XElement(v2 + "FA", "tel:" + message.GetDestination())
                        ), // End of RequestSOAPHeader
                    new XElement(soapenv + "Body",
                        new XElement(loc + "sendSms",
                            new XElement(loc + "addresses", "tel:" + Destination),
                            new XElement(loc + "senderName", Sender),
                            new XElement(loc + "message", Text),
                            new XElement(loc + "receiptRequest",
                                new XElement("endpoint", "http://" + ConnectionDetails.GetHostPPPAddress() + "/AverageBid/Deliveries/"),
                                new XElement("interfaceName", "SmsNotification"),
                                new XElement("correlator", Correlator)
                            ) // End of receiptRequest
                        ) // End of sendSms
                    ) // End of Soap Body
                ); // End of Soap Envelope

            return soapEnvelope.ToString();
        }
    }
}