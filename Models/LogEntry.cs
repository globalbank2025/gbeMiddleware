using System;

namespace GBEMiddlewareApi.Models
{
    public class LogEntry
    {
        public int LogEntryId { get; set; }

        // e.g. "http://10.1.200.153:7003/FCUBSAccService/FCUBSAccService"
        public string Endpoint { get; set; }

        // Full SOAP or JSON request
        public string RequestXml { get; set; }

        // Full SOAP or JSON response
        public string ResponseXml { get; set; }

        // Optionally track the user or client who initiated the request
        public string Initiator { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
