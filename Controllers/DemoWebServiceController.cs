using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Data;   // <--- for MiddlewareDbContext
using GBEMiddlewareApi.Models;
using System.Text.Json; // <--- for Service, ApiCredentials

namespace GBEMiddlewareApi.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    public class DemoWebServiceController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DemoWebServiceController> _logger;
        private readonly MiddlewareDbContext _middlewareContext;
        private readonly ApplicationDbContext _applicationDbContext;


        // Adjust or rename the URL if needed:
        private const string SoapEndpointUrl = "http://10.1.200.153:7003/FCUBSAccService/FCUBSAccService";

        public DemoWebServiceController(
            IHttpClientFactory httpClientFactory,
            ILogger<DemoWebServiceController> logger,
            MiddlewareDbContext middlewareContext,
            ApplicationDbContext applicationDbContext)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _middlewareContext = middlewareContext;
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// Gets customer account information from CBS, with credential checks.
        /// </summary>
        /// <param name="ServiceCode">e.g., ESB</param>
        /// <param name="AccountNo">e.g., 1091101371448</param>
        /// <param name="ExtRefNo">external ref no</param>
        /// <param name="UserId">maps to ApiCredentials.Username</param>
        /// <param name="Password">maps to ApiCredentials.Password</param>
        /// <param name="NameBal">"Name" => returns CUSTNAME; "Bal" => returns ACY_AVL_BAL</param>
        /// <returns>JSON object with statusCode, message, AccountNo, CustomerData</returns>
        [HttpGet("AccountInfoRequest")]
        public async Task<IActionResult> AccountInfoRequest(
            [FromQuery] string ServiceCode,
            [FromQuery] string AccountNo,
            [FromQuery] string ExtRefNo,
            [FromQuery] string UserId,
            [FromQuery] string Password,
            [FromQuery] string NameBal)
        {
            try
            {
                // 1) Validate ServiceCode
                var serviceRecord = await _middlewareContext.Services
                    .FirstOrDefaultAsync(s => s.ServiceCode == ServiceCode);

                if (serviceRecord == null)
                {
                    _logger.LogWarning("Invalid Service Code => {ServiceCode}", ServiceCode);
                    return BadRequest(new { message = "Invalid Service Code: " + ServiceCode });
                }

                // 2) Retrieve matching ApiCredentials by serviceId + (username/password)
                var apiCred = await _middlewareContext.ApiCredentials
                    .FirstOrDefaultAsync(ac => ac.ServiceId == serviceRecord.ServiceId
                                            && ac.Username == UserId
                                            && ac.Password == Password
                                            && ac.Status == "ACTIVE");
                if (apiCred == null)
                {
                    _logger.LogWarning("Invalid Credentials => ServiceId: {ServiceId}, UserId: {UserId}", serviceRecord.ServiceId, UserId);
                    return BadRequest(new { message = "Invalid Credential" });
                }

                // 3) Check mandatory parameter: AccountNo
                if (string.IsNullOrEmpty(AccountNo))
                {
                    return BadRequest("AccountNo is required.");
                }

                // 4) Derive the first 3 digits from AccountNo
                if (AccountNo.Length < 3)
                {
                    return BadRequest("AccountNo must be at least 3 digits to extract BRN.");
                }
                string brn = AccountNo.Substring(0, 3);

                // 5) Build SOAP request
                string soapEnvelope = BuildSoapEnvelope(brn, AccountNo);
                _logger.LogInformation("SOAP Request => {SoapEnvelope}", soapEnvelope);

                // 6) Call CBS
                var client = _httpClientFactory.CreateClient();
                var httpContent = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                httpContent.Headers.Clear();
                httpContent.Headers.Add("Content-Type", "text/xml;charset=UTF-8");

                HttpResponseMessage soapResponse;
                try
                {
                    soapResponse = await client.PostAsync(SoapEndpointUrl, httpContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling SOAP service");
                    return StatusCode(500, "Error calling SOAP service: " + ex.Message);
                }

                if (!soapResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("SOAP service returned code {Code}", soapResponse.StatusCode);
                    return StatusCode((int)soapResponse.StatusCode, "SOAP service returned an error");
                }

                string responseContent = await soapResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("SOAP Response => {ResponseContent}", responseContent);

                // 7) Parse SOAP response
                XDocument xDoc;
                try
                {
                    xDoc = XDocument.Parse(responseContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing SOAP response");
                    return StatusCode(500, "Error parsing SOAP response: " + ex.Message);
                }

                // 8) Evaluate <MSGSTAT>
                XNamespace ns = "http://fcubs.ofss.com/service/FCUBSAccService";
                var msgStat = xDoc.Descendants(ns + "MSGSTAT").FirstOrDefault()?.Value;

                if (string.Equals(msgStat, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    // success => extract <CUSTNAME> & <ACY_AVL_BAL>
                    var custName = xDoc.Descendants(ns + "CUSTNAME").FirstOrDefault()?.Value;
                    var availableBalance = xDoc.Descendants(ns + "ACY_AVL_BAL").FirstOrDefault()?.Value;

                    if (string.Equals(NameBal, "Bal", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(availableBalance))
                        {
                            _logger.LogWarning("No <ACY_AVL_BAL> found in SOAP response => {ResponseContent}", responseContent);
                            return NotFound("Available balance not found in SOAP response.");
                        }
                        return Ok(new
                        {
                            statusCode = 200,
                            message = "Successful",
                            AccountNo = AccountNo,
                            CustomerData = availableBalance
                        });
                    }
                    else // "Name" or default
                    {
                        if (string.IsNullOrEmpty(custName))
                        {
                            _logger.LogWarning("No <CUSTNAME> found in SOAP response => {ResponseContent}", responseContent);
                            return NotFound("Customer name not found in SOAP response.");
                        }
                        return Ok(new
                        {
                            statusCode = 200,
                            message = "Successful",
                            AccountNo = AccountNo,
                            CustomerData = custName
                        });
                    }
                }
                else if (string.Equals(msgStat, "FAILURE", StringComparison.OrdinalIgnoreCase))
                {
                    // gather error descriptions
                    var errorDescriptions = xDoc.Descendants(ns + "FCUBS_ERROR_RESP")
                        .Descendants(ns + "ERROR")
                        .Select(e => e.Element(ns + "EDESC")?.Value)
                        .Where(ed => !string.IsNullOrEmpty(ed))
                        .ToList();

                    string errorMsg = string.Join(" | ", errorDescriptions);
                    _logger.LogWarning("SOAP FAILURE => {ErrorMsg}", errorMsg);
                    return BadRequest(new { statusCode = 400, message = errorMsg });
                }
                else
                {
                    _logger.LogWarning("Unknown SOAP <MSGSTAT> => {MsgStat}", msgStat);
                    return StatusCode(500, "Unknown SOAP message status.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AccountInfoRequest");
                return StatusCode(500, "Unhandled exception: " + ex.Message);
            }
        }

        // Helper method to build the SOAP envelope
        private string BuildSoapEnvelope(string brn, string acc)
        {
            return $@"
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:fcub=""http://fcubs.ofss.com/service/FCUBSAccService"">
               <soapenv:Header/>
               <soapenv:Body>
                  <fcub:QUERYCUSTACC_IOFS_REQ>
                     <fcub:FCUBS_HEADER>
                        <fcub:SOURCE>CHQPNT</fcub:SOURCE>
                        <fcub:UBSCOMP>FCUBS</fcub:UBSCOMP>
                        <fcub:USERID>ESBUSER</fcub:USERID>
                        <fcub:BRANCH>000</fcub:BRANCH>
                        <fcub:MODULEID>AC</fcub:MODULEID>
                        <fcub:SERVICE>FCUBSAccService</fcub:SERVICE>
                        <fcub:OPERATION>QueryCustAcc</fcub:OPERATION>
                        <fcub:SOURCE_OPERATION>QueryCustAcc</fcub:SOURCE_OPERATION>
                        <fcub:SOURCE_USERID>ESBUSER</fcub:SOURCE_USERID>
                     </fcub:FCUBS_HEADER>
                     <fcub:FCUBS_BODY>
                        <fcub:Cust-Account-IO>
                           <fcub:BRN>{brn}</fcub:BRN>
                           <fcub:ACC>{acc}</fcub:ACC>
                        </fcub:Cust-Account-IO>
                     </fcub:FCUBS_BODY>
                  </fcub:QUERYCUSTACC_IOFS_REQ>
               </soapenv:Body>
            </soapenv:Envelope>";
                    
        }



        [HttpPost("PostTransaction")]
        public async Task<IActionResult> PostTransaction([FromBody] PostTransactionRequest req)
        {
            // Serialize the raw user request JSON exactly as received (for debugging, if needed)
            var rawUserRequest = JsonSerializer.Serialize(req);

            // This will hold the SOAP envelope sent to CBS
            string soapRequest = null;

            try
            {
                // 1) Authenticate ServiceCode
                var serviceRecord = await _middlewareContext.Services
                    .FirstOrDefaultAsync(s => s.ServiceCode == req.ServiceCode);
                if (serviceRecord == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = 100,
                        Status = "FAILURE",
                        Message = "Invalid Credentials",
                        ErrorMsg = "FAILURE"
                    });
                }

                // 2) Retrieve matching ApiCredentials
                var apiCred = await _middlewareContext.ApiCredentials
                    .FirstOrDefaultAsync(ac =>
                        ac.ServiceId == serviceRecord.ServiceId &&
                        ac.Username == req.UserId &&
                        ac.Password == req.Password &&
                        ac.Status == "ACTIVE");
                if (apiCred == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = 100,
                        Status = "FAILURE",
                        Message = "Invalid Credentials",
                        ErrorMsg = "FAILURE"
                    });
                }

                // 3) Basic request validation
                if (string.IsNullOrEmpty(req.AccountNo))
                {
                    return BadRequest(new
                    {
                        StatusCode = 300,
                        Status = "FAILURE",
                        Message = "AccountNo is required",
                        ErrorMsg = "Missing AccountNo"
                    });
                }
                if (req.AccountNo.Length < 3)
                {
                    return BadRequest(new
                    {
                        StatusCode = 300,
                        Status = "FAILURE",
                        Message = "AccountNo must be >= 3 digits",
                        ErrorMsg = "Invalid AccountNo"
                    });
                }
                if (string.IsNullOrEmpty(req.Amount))
                {
                    return BadRequest(new
                    {
                        StatusCode = 300,
                        Status = "FAILURE",
                        Message = "Amount is required",
                        ErrorMsg = "Missing Amount"
                    });
                }

                // Ensure the user provides an ExtRefNo
                if (string.IsNullOrEmpty(req.ExtRefNo))
                {
                    return BadRequest(new
                    {
                        StatusCode = 300,
                        Status = "FAILURE",
                        Message = "ExtRefNo is required",
                        ErrorMsg = "Missing ExtRefNo"
                    });
                }

                // Check for duplicate ExtRefNo in the transaction log
                bool isDuplicate = await _applicationDbContext.TransactionLogs
                    .AnyAsync(t => t.ExternalTransactionReference == req.ExtRefNo);
                if (isDuplicate)
                {
                    return BadRequest(new
                    {
                        StatusCode = 100,
                        Status = "FAILURE",
                        Message = "Duplicate Reference",
                        ErrorMsg = "FAILURE"
                    });
                }

                // 4) Build the SOAP envelope
                string msgId = GenerateRandomMsgId();
                soapRequest = BuildTransactionSoapEnvelope(req, serviceRecord, msgId);
                _logger.LogInformation("PostTransaction SOAP Request => {SoapEnvelope}", soapRequest);

                // 5) Post to the SOAP endpoint
                var client = _httpClientFactory.CreateClient();
                var httpContent = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                httpContent.Headers.Clear();
                httpContent.Headers.Add("Content-Type", "text/xml;charset=UTF-8");

                HttpResponseMessage soapResponse;
                try
                {
                    soapResponse = await client.PostAsync("http://10.1.200.153:7003/FCUBSRTService/FCUBSRTService", httpContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling PostTransaction SOAP service");

                    var finalResponse = new
                    {
                        StatusCode = 500,
                        Status = "FAILURE",
                        Message = "Error calling SOAP service",
                        ErrorMsg = ex.Message
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    // Log using the SOAP envelope as the request payload
                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: 0,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: null, // No CBS reference
                        externalTransactionReference: req.ExtRefNo
                    );

                    return StatusCode(500, finalResponse);
                }

                string responseContent = await soapResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("PostTransaction SOAP Response => {Resp}", responseContent);

                if (!soapResponse.IsSuccessStatusCode)
                {
                    var finalResponse = new
                    {
                        StatusCode = (int)soapResponse.StatusCode,
                        Status = "FAILURE",
                        Message = "SOAP service returned an error",
                        ErrorMsg = "HTTP code " + soapResponse.StatusCode
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: 0,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: null,
                        externalTransactionReference: req.ExtRefNo
                    );

                    return StatusCode((int)soapResponse.StatusCode, finalResponse);
                }

                // 6) Parse the SOAP response
                XDocument xDoc;
                try
                {
                    xDoc = XDocument.Parse(responseContent);
                }
                catch (Exception ex)
                {
                    var finalResponse = new
                    {
                        StatusCode = 500,
                        Status = "FAILURE",
                        Message = "Error parsing SOAP response",
                        ErrorMsg = ex.Message
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: 0,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: null,
                        externalTransactionReference: req.ExtRefNo
                    );

                    return StatusCode(500, finalResponse);
                }

                XNamespace ns = "http://fcubs.ofss.com/service/FCUBSRTService";
                var msgStat = xDoc.Descendants(ns + "MSGSTAT").FirstOrDefault()?.Value;

                decimal parsedAmount = 0;
                decimal.TryParse(req.Amount, out parsedAmount);

                // 7) Evaluate the SOAP response
                if (string.Equals(msgStat, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    var xref = xDoc.Descendants(ns + "XREF").FirstOrDefault()?.Value;
                    if (string.IsNullOrEmpty(xref)) xref = "NoXREFFound";

                    var finalResponse = new
                    {
                        statusCode = 200,
                        message = "Success",
                        BankRefNo = xref
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: parsedAmount,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: xref,
                        externalTransactionReference: req.ExtRefNo
                    );

                    return Ok(finalResponse);
                }
                else if (string.Equals(msgStat, "FAILURE", StringComparison.OrdinalIgnoreCase))
                {
                    var errorDescriptions = xDoc.Descendants(ns + "FCUBS_ERROR_RESP")
                        .Descendants(ns + "ERROR")
                        .Select(e => e.Element(ns + "EDESC")?.Value)
                        .Where(ed => !string.IsNullOrEmpty(ed))
                        .ToList();

                    string errorMsg = string.Join(" | ", errorDescriptions);
                    var finalResponse = new
                    {
                        StatusCode = 300,
                        Status = "FAILURE",
                        Message = "Transaction failed in CBS",
                        ErrorMsg = errorMsg
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: parsedAmount,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: null,
                        externalTransactionReference: req.ExtRefNo
                    );

                    return BadRequest(finalResponse);
                }
                else
                {
                    var finalResponse = new
                    {
                        StatusCode = 500,
                        Status = "FAILURE",
                        Message = "Unknown SOAP message status",
                        ErrorMsg = msgStat
                    };
                    var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                    await SaveTransactionLogAsync(
                        accountNo: req.AccountNo,
                        amount: parsedAmount,
                        requestPayload: soapRequest,
                        responsePayload: finalResponseJson,
                        userId: req.UserId ?? "Unknown",
                        transactionRef: null,
                        externalTransactionReference: req.ExtRefNo
                    );

                    return StatusCode(500, finalResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in PostTransaction");
                var finalResponse = new
                {
                    StatusCode = 500,
                    Status = "FAILURE",
                    Message = "Unhandled exception",
                    ErrorMsg = ex.Message
                };
                var finalResponseJson = JsonSerializer.Serialize(finalResponse);

                await SaveTransactionLogAsync(
                    accountNo: string.IsNullOrEmpty(req.AccountNo) ? "N/A" : req.AccountNo,
                    amount: 0,
                    requestPayload: soapRequest,
                    responsePayload: finalResponseJson,
                    userId: req.UserId ?? "Unknown",
                    transactionRef: null,
                    externalTransactionReference: req.ExtRefNo
                );

                return StatusCode(500, finalResponse);
            }
        }

        private string GenerateRandomMsgId()
        {
            var random = new Random();
            var randomNumber = random.Next(0, 1000000000).ToString("D10"); // zero-padded to 10 digits
            return randomNumber;  // e.g. "6125088095"
        }

        private string BuildTransactionSoapEnvelope(PostTransactionRequest req, Service serviceRecord, string msgId)
        {
            // Determine TXNACC and OFFSETACC based on OffSetAccNo and ServiceType:
            // 1. If serviceRecord.OffsetAccNo equals "OnQuery" (ignoring case), then:
            //    TXNACC = req.AccountNo and OFFSETACC = req.ExtEntity.
            // 2. Otherwise:
            //    - For ServiceType "Credit": TXNACC = req.AccountNo, OFFSETACC = serviceRecord.OffsetAccNo.
            //    - For ServiceType "Debit":  TXNACC = serviceRecord.OffsetAccNo, OFFSETACC = req.AccountNo.
            string txnAcc;
            string offsetAcc;

            if (serviceRecord.OffsetAccNo.Equals("OnQuery", StringComparison.OrdinalIgnoreCase))
            {
                txnAcc = req.AccountNo;
                offsetAcc = req.ExtEntity;
            }
            else
            {
                if (serviceRecord.ServiceType.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                {
                    txnAcc = req.AccountNo;
                    offsetAcc = serviceRecord.OffsetAccNo;
                }
                else if (serviceRecord.ServiceType.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                {
                    txnAcc = serviceRecord.OffsetAccNo;
                    offsetAcc = req.AccountNo;
                }
                else
                {
                    // Fallback logic
                    txnAcc = req.AccountNo;
                    offsetAcc = serviceRecord.OffsetAccNo;
                }
            }

            // Use the serviceRecord.ProductCode or default to "FTRQ"
            string productCode = !string.IsNullOrEmpty(serviceRecord.ProductCode) ? serviceRecord.ProductCode : "FTRQ";

            // Adjust TXNBRN: Extract the first 3 digits from the determined TXNACC value.
            string txnbrn = "000";
            if (!string.IsNullOrEmpty(txnAcc) && txnAcc.Length >= 3)
            {
                txnbrn = txnAcc.Substring(0, 3);
            }

            // Adjust OFFSETBRN: Extract the first 3 digits from the determined OFFSETACC value.
            string offsetBranch = "000";
            if (!string.IsNullOrEmpty(offsetAcc) && offsetAcc.Length >= 3)
            {
                offsetBranch = offsetAcc.Substring(0, 3);
            }

            // Build and return the SOAP envelope
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:fcub=""http://fcubs.ofss.com/service/FCUBSRTService"">
    <soapenv:Header/>
    <soapenv:Body>
        <fcub:CREATETRANSACTION_FSFS_REQ>
            <fcub:FCUBS_HEADER>
                <fcub:SOURCE>CHQPNT</fcub:SOURCE>
                <fcub:UBSCOMP>FCUBS</fcub:UBSCOMP>
                <fcub:USERID>ESBUSER</fcub:USERID>
                <fcub:BRANCH>{req.BranchCode}</fcub:BRANCH>
                <fcub:SERVICE>FCUBSRTService</fcub:SERVICE>
                <fcub:OPERATION>CreateTransaction</fcub:OPERATION>
                <fcub:MSGID>{msgId}</fcub:MSGID>
            </fcub:FCUBS_HEADER>
            <fcub:FCUBS_BODY>
                <fcub:Transaction-Details>
                    <fcub:PRD>{productCode}</fcub:PRD>
                    <fcub:BRN>{txnbrn}</fcub:BRN>
                    <fcub:TXNBRN>{txnbrn}</fcub:TXNBRN>
                    <fcub:TXNACC>{txnAcc}</fcub:TXNACC>
                    <fcub:TXNCCY>ETB</fcub:TXNCCY>
                    <fcub:TXNAMT>{req.Amount}</fcub:TXNAMT>
                    <fcub:OFFSETBRN>{offsetBranch}</fcub:OFFSETBRN>
                    <fcub:OFFSETACC>{offsetAcc}</fcub:OFFSETACC>
                    <fcub:OFFSETCCY>ETB</fcub:OFFSETCCY>
                    <fcub:TXNDATE>{req.TxnDate}</fcub:TXNDATE>
                    <fcub:VALDATE>{req.TxnDate}</fcub:VALDATE>
                    <fcub:NARRATIVE>{req.TxnDesc}</fcub:NARRATIVE>
                </fcub:Transaction-Details>
            </fcub:FCUBS_BODY>
        </fcub:CREATETRANSACTION_FSFS_REQ>
    </soapenv:Body>
</soapenv:Envelope>";
        }

        // Saving Transaction Log
        private async Task SaveTransactionLogAsync(
           string accountNo,
           decimal amount,
           string requestPayload,
           string responsePayload,
           string userId,  // or "approvedBy"
           string? transactionRef = null,
           string? externalTransactionReference = null,
           int? vatCollectionTxId = null,
           string? customerName = null)
        {
            var logEntry = new TransactionLog
            {
                VatCollectionTransactionId = vatCollectionTxId,
                CustomerAccount = accountNo,
                TransactionAmount = amount,
                CustomerName = customerName,

                RequestPayload = requestPayload,
                ResponsePayload = responsePayload,

                TransactionReference = transactionRef,
                ExternalTransactionReference = externalTransactionReference,

                ApprovedBy = userId,
                ApprovedAt = DateTimeOffset.UtcNow,

                CreatedAt = DateTimeOffset.UtcNow
            };

            _applicationDbContext.TransactionLogs.Add(logEntry);
            await _applicationDbContext.SaveChangesAsync();
        }


    }


    public class PostTransactionRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ServiceCode { get; set; }

        public string AccountNo { get; set; }
        public string BranchCode { get; set; }
        public string Amount { get; set; }     // or decimal if you prefer
        public string TxnDate { get; set; }    // or DateTime
        public string ExtEntity { get; set; }
        public string ExtRefNo { get; set; }
        public string TxnDesc { get; set; }
    }

}
