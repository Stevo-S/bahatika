using AverageBid.Models;
using AverageBid.SMS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace AverageBid.Controllers
{
    public class SMSNotificationsController : Controller
    {
        // GET: SMSNotifications
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Start()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Start(string shortCode, string serviceId, string criteria, string correlator)
        {
            var timestampString = DateTime.Now.ToString("yyyyMMddHHmmss");
            XNamespace soapenv = SOAPRequestNamespaces.Soapenv;
            XNamespace v2 = SOAPRequestNamespaces.V2;
            XNamespace loc = SOAPRequestNamespaces.LocSmsNotification;
            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "v2", v2.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header",
                            new XElement(v2 + "RequestSOAPHeader",
                                new XAttribute("xmlns", v2.NamespaceName),
                                new XElement(v2 + "spId", ConnectionDetails.GetSpID()),
                                new XElement(v2 + "spPassword", ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + timestampString)),
                                new XElement(v2 + "serviceId", serviceId),
                                new XElement(v2 + "timeStamp", timestampString)
                            )
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(loc + "startSmsNotification",
                            new XElement(loc + "reference",
                                new XElement("endpoint", "http://" + ConnectionDetails.GetHostPPPAddress() + "/AverageBid/SmsNotifications/Receive"),
                                new XElement("interfaceName", "notifySmsReception"),
                                new XElement("correlator", correlator)
                            ),
                            new XElement(loc + "smsServiceActivationNumber", shortCode)
                        )
                    )
                );

            var notificationRequest = soapEnvelope.ToString();
            TempData["SmsNotificationResponse"] = Send(notificationRequest, timestampString);
            TempData["SmsNotificationRequest"] = notificationRequest;
            return View("Stop");
        }

        public ActionResult Stop()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Stop(string shortCode, string serviceId, string criteria, string correlator)
        {
            var timestampString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string password = ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + timestampString);
            XNamespace soapenv = SOAPRequestNamespaces.Soapenv;
            XNamespace v2 = SOAPRequestNamespaces.V2;
            XNamespace loc = SOAPRequestNamespaces.LocSmsNotification;
            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "v2", v2.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header",
                            new XElement(v2 + "RequestSOAPHeader",
                                new XElement("spId", ConnectionDetails.GetSpID()),
                                new XElement("spPassword", password),
                                new XElement("serviceId", serviceId),
                                new XElement("timeStamp", timestampString)
                            )
                    ),
                    new XElement(soapenv + "Body",
                        new XElement(loc + "stopSmsNotification",
                            new XElement(loc + "correlator", correlator)
                        )
                    )
                );

            var notificationRequest = soapEnvelope.ToString();
            TempData["SmsNotificationResponse"] = Send(notificationRequest, timestampString);
            TempData["SmsNotificationRequest"] = notificationRequest;
            return View("Start");
        }

        [HttpPost]
        [Route("SmsNotifications/Receive")]
        public  string ReceiveSmsNotification()
        {
            string notificationSoapString = (new StreamReader(Request.InputStream, true)).ReadToEnd();


            XNamespace ns1 = SOAPRequestNamespaces.Ns1;
            XNamespace loc = SOAPRequestNamespaces.LocNotification;
            XNamespace v2 = SOAPRequestNamespaces.V2;

            //string notificationSoapString = Request.Content.ReadAsStringAsync().Result;
            //Elmah.ErrorSignal.FromCurrentContext().Raise(new InvalidOperationException("Unable to process received SMS:" + notificationSoapString));
            XElement soapEnvelope = XElement.Parse(notificationSoapString);

            string sender = (string)
                                    (from el in soapEnvelope.Descendants("senderAddress")
                                     select el).First();
            sender = sender.Substring(4);


            string message = (string)
                                        (from el in soapEnvelope.Descendants("message")
                                         select el).First();

            string serviceId = (string)
                                    (from el in soapEnvelope.Descendants(ns1 + "serviceId")
                                     select el).First();

            string shortCode = (string)
                                    (from el in soapEnvelope.Descendants("smsServiceActivationNumber")
                                     select el).First();
            shortCode = shortCode.Substring(4);

            string correlator = (string)
                                (from el in soapEnvelope.Descendants(loc + "correlator")
                                 select el).First();

            string linkId = (string)
                                (from el in soapEnvelope.Descendants(ns1 + "linkid")
                                 select el).First();

            string traceUniqueId = (string)
                                        (from el in soapEnvelope.Descendants(ns1 + "traceUniqueID")
                                         select el).First();

            using (var db = new ApplicationDbContext())
            {

                var incoming = new InboundMessage()
                {
                    Sender = sender,
                    Correlator = correlator,
                    LinkId = linkId,
                    Message = message,
                    ServiceId = serviceId,
                    ShortCode = shortCode,
                    Timestamp = DateTime.Now,
                    TraceUniqueId = traceUniqueId
                };


                db.InboundMessages.Add(incoming);
                db.SaveChanges();

                var outgoing = new OutboundMessage()
                {
                    Destination = incoming.Sender,
                    LinkId = incoming.LinkId,
                    Message = incoming.GetResponse(db),
                    ServiceId = serviceId,
                    Correlator = incoming.Correlator,
                    Timestamp = DateTime.Now
                };

                db.OutboundMessages.Add(outgoing);
                db.SaveChanges();


                try
                {
                    outgoing.Send();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return "OK";
        }

        private string Send(string message, string timestampString)
        {
            using (var handler = new HttpClientHandler() { Credentials = new NetworkCredential(ConnectionDetails.GetUsername(), ConnectionDetails.HashPassword(ConnectionDetails.GetSpID() + ConnectionDetails.GetPassword() + timestampString)) })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(ConnectionDetails.GetRemoteSmsServiceUrl());

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/SmsNotificationManagerService/services/SmsNotificationManager/");
                request.Content = new StringContent(message, Encoding.UTF8, "text/xml");
                string requestContentString = request.Content.ReadAsStringAsync().Result;
                var result = client.SendAsync(request).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;

                return resultContent;
            }
        }
    }
}