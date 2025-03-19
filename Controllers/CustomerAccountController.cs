using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GBEMiddlewareApi.Models;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; // or Newtonsoft.Json if you prefer

namespace GBEMiddlewareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [PortalOrApiKeyAuth] // Ensures either portal user or valid API key
    public class CustomerAccountController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomerAccountController> _logger;
        private readonly ApplicationDbContext _dbContext;

        private const string CreateTransactionSoapUrl = "http://10.1.200.153:7003/FCUBSRTService/FCUBSRTService";
        private const string SoapEndpointUrl = "http://10.1.200.153:7003/FCUBSAccService/FCUBSAccService";
        private readonly IAuthorizationService _authorizationService;

        public CustomerAccountController(
            IHttpClientFactory httpClientFactory,
            ILogger<CustomerAccountController> logger,
            ApplicationDbContext dbContext,
              IAuthorizationService authorizationService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _dbContext = dbContext;
            _authorizationService = authorizationService;

        }

        // ====================================================================
        // (1) SOAP Envelope builder for "CreateTransaction"
        // ====================================================================
        private string BuildCreateTransactionSoapEnvelope(CreateTransactionRequest req)
        {
            // 1) Extract first 3 digits of TxnAcc to use as <fcub:TXNBRN>
            string txnBrnFromAcc = (req.TxnAcc?.Length >= 3)
                ? req.TxnAcc.Substring(0, 3)
                : "000";

            // 2) Generate random 10-digit number
            //    Example: 0123456789, then prepend "SRCH"
            var random = new Random();
            var randomNumber = random.Next(0, 1000000000).ToString("D10"); // 10 digits, zero-padded
            string msgId = "SRCH" + randomNumber;

            // 3) Build the SOAP envelope, inserting <fcub:MSGID> inside FCUBS_HEADER
            return $@"
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:fcub=""http://fcubs.ofss.com/service/FCUBSRTService"">
               <soapenv:Header/>
               <soapenv:Body>
                  <fcub:CREATETRANSACTION_FSFS_REQ>
                     <fcub:FCUBS_HEADER>
                        <fcub:SOURCE>CHQPNT</fcub:SOURCE>
                        <fcub:UBSCOMP>FCUBS</fcub:UBSCOMP>
                        <fcub:USERID>ESBUSER</fcub:USERID>
                        <fcub:BRANCH>{req.TxnBrn}</fcub:BRANCH>
                        <fcub:SERVICE>FCUBSRTService</fcub:SERVICE>
                        <fcub:OPERATION>CreateTransaction</fcub:OPERATION>
                        <fcub:MSGID>{msgId}</fcub:MSGID>
                     </fcub:FCUBS_HEADER>
                     <fcub:FCUBS_BODY>
                        <fcub:Transaction-Details>
                           <fcub:PRD>SRCH</fcub:PRD>
                           <fcub:BRN>{req.TxnBrn}</fcub:BRN>
                           <!-- Use the first 3 digits of TxnAcc for TXNBRN -->
                           <fcub:TXNBRN>{txnBrnFromAcc}</fcub:TXNBRN>
                           <fcub:TXNACC>{req.TxnAcc}</fcub:TXNACC>
                           <fcub:TXNCCY>ETB</fcub:TXNCCY>
                           <fcub:TXNAMT>{req.TxnAmt}</fcub:TXNAMT>
                           <fcub:OFFSETBRN>{req.OffsetBrn}</fcub:OFFSETBRN>
                           <fcub:OFFSETACC>{req.OffsetAcc}</fcub:OFFSETACC>
                           <fcub:OFFSETCCY>ETB</fcub:OFFSETCCY>
                           <fcub:TXNDATE>{req.TxnDate}</fcub:TXNDATE>
                           <fcub:VALDATE>{req.ValDate}</fcub:VALDATE>
                           <fcub:NARRATIVE>{req.Narrative}</fcub:NARRATIVE>
                        </fcub:Transaction-Details>
                     </fcub:FCUBS_BODY>
                  </fcub:CREATETRANSACTION_FSFS_REQ>
               </soapenv:Body>
            </soapenv:Envelope>";
        }

        // ====================================================================
        // (2) SOAP Envelope for Query Status
        // ====================================================================
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

        // ====================================================================
        // (3) Endpoint: status-balance
        // ====================================================================
        [HttpPost("status-balance")]
        public async Task<IActionResult> GetCustomerAccountStatusBalance([FromBody] QueryCustomerAccountRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.BRN) || string.IsNullOrEmpty(request.ACC))
            {
                return BadRequest("BRN and ACC are required.");
            }

            string initiator = User?.Identity?.IsAuthenticated == true
                ? User.Identity.Name ?? "PortalUser"
                : HttpContext.Items["ApiClientName"]?.ToString() ?? "UnknownClient";

            string soapEnvelope = BuildSoapEnvelope(request.BRN, request.ACC);
            _logger.LogInformation("SOAP Request => {SoapEnvelope}", soapEnvelope);

            await SaveLogAsync(SoapEndpointUrl, soapEnvelope, null, initiator);

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

            await SaveLogAsync(SoapEndpointUrl, soapEnvelope, responseContent, initiator);

            try
            {
                XDocument xDoc = XDocument.Parse(responseContent);
                XNamespace ns = "http://fcubs.ofss.com/service/FCUBSAccService";

                var msgStat = xDoc.Descendants(ns + "MSGSTAT").FirstOrDefault()?.Value;
                if (string.Equals(msgStat, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    var custName = xDoc.Descendants(ns + "CUSTNAME").FirstOrDefault()?.Value;
                    var acStatNoDr = xDoc.Descendants(ns + "ACSTATNODR").FirstOrDefault()?.Value;
                    var acStatNoCr = xDoc.Descendants(ns + "ACSTATNOCR").FirstOrDefault()?.Value;
                    var frozen = xDoc.Descendants(ns + "FROZEN").FirstOrDefault()?.Value;
                    var availableBalance = xDoc.Descendants(ns + "ACY_AVL_BAL").FirstOrDefault()?.Value;

                    if (string.IsNullOrEmpty(custName))
                    {
                        _logger.LogWarning("No <CUSTNAME> found in SOAP response.");
                        return NotFound("Customer name not found in SOAP response.");
                    }

                    return Ok(new
                    {
                        customerName = custName,
                        acStatNoDr,
                        acStatNoCr,
                        frozen,
                        availableBalance
                    });
                }
                else if (string.Equals(msgStat, "FAILURE", StringComparison.OrdinalIgnoreCase))
                {
                    var errorDescriptions = xDoc.Descendants(ns + "FCUBS_ERROR_RESP")
                        .Descendants(ns + "ERROR")
                        .Select(e => e.Element(ns + "EDESC")?.Value)
                        .Where(ed => !string.IsNullOrEmpty(ed))
                        .ToList();

                    string errorMsg = string.Join(" | ", errorDescriptions);
                    _logger.LogWarning("SOAP Error => {ErrorMsg}", errorMsg);
                    return BadRequest(new { error = errorMsg });
                }
                else
                {
                    _logger.LogWarning("SOAP message status unknown => {MsgStat}", msgStat);
                    return StatusCode(500, "SOAP message status unknown.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SOAP response");
                return StatusCode(500, "Error parsing SOAP response: " + ex.Message);
            }
        }

        // ====================================================================
        // (4) create-transaction (SOAP pass-through)
        // --> Extracts <XREF> and <MSGID> from the SOAP response
        // ====================================================================
        [HttpPost("create-transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.TxnAcc) ||
                string.IsNullOrEmpty(request.OffsetAcc))
            {
                return BadRequest("Missing required fields (TxnAcc, OffsetAcc, etc.)");
            }

            // 1) Build the SOAP envelope
            string soapEnvelope = BuildCreateTransactionSoapEnvelope(request);
            _logger.LogInformation("CreateTransaction SOAP Request => {SoapEnvelope}", soapEnvelope);

            // 2) Log request
            await SaveLogAsync(CreateTransactionSoapUrl, soapEnvelope, null, "CreateTransaction");

            // 3) Post to the SOAP service
            var client = _httpClientFactory.CreateClient();
            var httpContent = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            httpContent.Headers.Clear();
            httpContent.Headers.Add("Content-Type", "text/xml;charset=UTF-8");

            HttpResponseMessage soapResponse;
            string responseContent;
            try
            {
                soapResponse = await client.PostAsync(CreateTransactionSoapUrl, httpContent);
                responseContent = await soapResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CreateTransaction SOAP service");
                return StatusCode(500, "Error calling SOAP service: " + ex.Message);
            }

            _logger.LogInformation("CreateTransaction SOAP Raw Response => {Resp}", responseContent);
            await SaveLogAsync(CreateTransactionSoapUrl, soapEnvelope, responseContent, "CreateTransaction");

            // Always parse SOAP body, even if status code isn't success
            try
            {
                XDocument xDoc = XDocument.Parse(responseContent);
                XNamespace ns = "http://fcubs.ofss.com/service/FCUBSRTService";

                // Extract MSGSTAT from <MSGSTAT> tag
                var msgStat = xDoc.Descendants(ns + "MSGSTAT").FirstOrDefault()?.Value;

                // Extract MSGID from <FCUBS_HEADER><MSGID>
                var msgId = xDoc
                    .Descendants(ns + "FCUBS_HEADER")
                    .Descendants(ns + "MSGID")
                    .FirstOrDefault()?.Value;

                // If the HTTP status code is not success, let's gather the error from SOAP if possible
                if (!soapResponse.IsSuccessStatusCode)
                {
                    var errorDescriptions = xDoc.Descendants(ns + "FCUBS_ERROR_RESP")
                        .Descendants(ns + "ERROR")
                        .Select(e => e.Element(ns + "EDESC")?.Value)
                        .Where(ed => !string.IsNullOrEmpty(ed))
                        .ToList();

                    if (errorDescriptions.Any())
                    {
                        string soapErrorMsg = string.Join(" | ", errorDescriptions);
                        _logger.LogError("CreateTransaction SOAP Error => {soapErrorMsg}", soapErrorMsg);
                        return StatusCode((int)soapResponse.StatusCode, $"CreateTransaction SOAP error: {soapErrorMsg}");
                    }

                    return StatusCode((int)soapResponse.StatusCode, "CreateTransaction SOAP service returned error");
                }

                // If HTTP status code was success, handle success/failure in SOAP response
                if (string.Equals(msgStat, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    var txnRef = xDoc.Descendants(ns + "XREF").FirstOrDefault()?.Value;
                    // Return both XREF & MSGID in the response
                    return Ok(new
                    {
                        message = "Transaction created successfully.",
                        transactionRef = txnRef,
                        msgId = msgId
                    });
                }
                else if (string.Equals(msgStat, "FAILURE", StringComparison.OrdinalIgnoreCase))
                {
                    var errorDescriptions = xDoc.Descendants(ns + "FCUBS_ERROR_RESP")
                        .Descendants(ns + "ERROR")
                        .Select(e => e.Element(ns + "EDESC")?.Value)
                        .Where(ed => !string.IsNullOrEmpty(ed))
                        .ToList();

                    string errorMsg = string.Join(" | ", errorDescriptions);
                    _logger.LogWarning("CreateTransaction SOAP Error => {ErrorMsg}", errorMsg);
                    return BadRequest(new { error = errorMsg });
                }
                else
                {
                    _logger.LogWarning("SOAP CreateTransaction msgStat unknown => {MsgStat}", msgStat);
                    return StatusCode(500, "CreateTransaction: SOAP message status unknown.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CreateTransaction SOAP response");
                return StatusCode(500, "Error parsing SOAP response: " + ex.Message + " | Raw: " + responseContent);
            }
        }

        // ====================================================================
        // (5) Create & List VAT Collection (Pending)
        // ====================================================================
        [HttpPost("vat-collection")]
        //[Authorize(Policy = "VatCollection_Create")]
        public async Task<IActionResult> CreateVatCollectionTransaction([FromBody] VatCollectionTransactionDto dto)
        {
            // Manually check if the current user has the "VatCollection_Create" permission.
            var authResult = await _authorizationService.AuthorizeAsync(User, null, "VatCollection_Create");
            if (!authResult.Succeeded)
            {
                // Return a custom error message with 403 Forbidden.
                return StatusCode(403, new { error = "Access denied. You do not have permission to create VAT collection transactions." });
            }
            if (dto == null || string.IsNullOrEmpty(dto.AccountNumber))
            {
                return BadRequest("Invalid or missing request data.");
            }

            // Check if BranchCode is valid (assuming 0 is not valid)
            if (dto.BranchCode == 0)
            {
                return BadRequest("Invalid Branch Code provided.");
            }

            try
            {
                var entity = new VatCollectionTransaction
                {
                    AccountNumber = dto.AccountNumber,
                    CustomerVatRegistrationNo = dto.CustomerVatRegistrationNo,
                    CustomerTinNo = dto.CustomerTinNo,
                    CustomerTelephone = dto.CustomerTelephone,
                    PrincipalAmount = dto.PrincipalAmount,
                    ServiceIncomeGl = dto.ServiceIncomeGl,
                    ServiceCharge = dto.ServiceCharge,
                    VatOnServiceCharge = dto.VatOnServiceCharge,
                    TotalAmount = dto.TotalAmount,
                    BranchCode = dto.BranchCode,
                    CreatedAt = DateTime.UtcNow,
                    Status = "PENDING",
                    CustomerName = dto.CustomerName,
                    ApprovedBy = string.Empty,
                    ServiceChargeReference = string.Empty
                };

                _dbContext.VatCollectionTransactions.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "VatCollectionTransaction created", id = entity.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VatCollectionTransaction");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("vat-collection")]
        public async Task<IActionResult> GetAllVatCollections()
        {
            try
            {
                var list = await _dbContext.VatCollectionTransactions
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving VatCollectionTransactions");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // ====================================================================
        // (6) Approve & Delete
        // ====================================================================
        [HttpPut("vat-collection/{id}/approve")]
        [Authorize(Policy = "VatCollection_Approve")]
        public async Task<IActionResult> ApproveVatCollectionTransaction(int id)
        {
            var pending = await _dbContext.VatCollectionTransactions
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == "PENDING");

            if (pending == null)
                return NotFound($"No pending transaction with Id={id}");

            // 1) Capture old data for auditing
            var oldDataObject = new
            {
                pending.Id,
                pending.AccountNumber,
                pending.Status,
                pending.ServiceChargeReference,
                pending.ServiceCharge,
                pending.VatOnServiceCharge,
                pending.TotalAmount,
                pending.PrincipalAmount,
                pending.ApprovedBy,
                pending.ApprovedDateTime
            };
            string oldDataJson = System.Text.Json.JsonSerializer.Serialize(oldDataObject);

            // 2) Update fields before calling SOAP
            pending.ApprovedDateTime = DateTime.UtcNow;
            var dateStr = pending.ApprovedDateTime.Value.ToString("yyyy-MM-dd");
            string branchStr = pending.BranchCode.ToString();

            // 3) Build SOAP request
            var svcChargeRequest = new CreateTransactionRequest
            {
                TxnBrn = branchStr,
                TxnAcc = pending.AccountNumber,
                TxnAmt = pending.ServiceCharge, // from DB
                OffsetBrn = branchStr,
                OffsetAcc = pending.ServiceIncomeGl,
                TxnDate = dateStr,
                ValDate = dateStr,
                Narrative = $"Service Charge + Vat from {pending.CustomerName}/{pending.AccountNumber}"
            };
            var externalRequestJson = System.Text.Json.JsonSerializer.Serialize(svcChargeRequest);
            _logger.LogInformation("Service Charge Request Payload => {Payload}", externalRequestJson);

            // 4) Call SOAP => Returns (bool Success, string Message, string TxnRef, string MsgId)
            var (success, message, txnRef, msgId) = await PostCreateTransactionAsync(svcChargeRequest);

            // -----------------------------------------------------------------
            //   ALWAYS CREATE A TransactionLog RECORD (even if SOAP fails)
            // -----------------------------------------------------------------
            // Combine the "message" + "msgId" into a single JSON for the ResponsePayload
            var responseObj = new { message, msgId };
            var responsePayloadJson = System.Text.Json.JsonSerializer.Serialize(responseObj);

            // Create and add the TransactionLog
            var transactionLog = new TransactionLog
            {
                VatCollectionTransactionId = pending.Id,
                CustomerAccount = pending.AccountNumber,
                TransactionAmount = pending.ServiceCharge,
                CustomerName = pending.CustomerName,

                RequestPayload = externalRequestJson,
                ResponsePayload = responsePayloadJson, // includes any error if SOAP failed
                TransactionReference = txnRef,         // might be null if SOAP fails

                // Even if SOAP fails, you can still store who attempted the approval
                ApprovedBy = User?.Identity?.Name ?? "Unknown",
                ApprovedAt = DateTimeOffset.UtcNow,

                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.TransactionLogs.Add(transactionLog);

            // -----------------------------------------------------------------
            //   IF SOAP CALL FAILED -> Save the failed log & return
            // -----------------------------------------------------------------
            if (!success)
            {
                // Save the TransactionLog (with the error info) so we don't lose it
                await _dbContext.SaveChangesAsync();

                _logger.LogError("ApproveVatCollectionTransaction => SOAP Txn failed: {Msg}", message);
                return BadRequest(new { error = message });
            }

            // -----------------------------------------------------------------
            //   IF SOAP CALL SUCCEEDED -> Mark as APPROVED + create AuditLog
            // -----------------------------------------------------------------
            pending.Status = "APPROVED";
            pending.ServiceChargeReference = txnRef; // XREF from SOAP
            pending.ApprovedBy = User?.Identity?.Name ?? "Unknown";

            _dbContext.VatCollectionTransactions.Update(pending);

            // 6) Capture new data for audit
            var newDataObject = new
            {
                pending.Id,
                pending.AccountNumber,
                pending.Status,
                pending.ServiceChargeReference,
                pending.ServiceCharge,
                pending.VatOnServiceCharge,
                pending.TotalAmount,
                pending.PrincipalAmount,
                pending.ApprovedBy,
                pending.ApprovedDateTime
            };
            string newDataJson = System.Text.Json.JsonSerializer.Serialize(newDataObject);

            // 7) Create audit record
            var audit = new AuditLog
            {
                TableName = "VatCollectionTransactions",
                RecordId = pending.Id,
                Operation = "APPROVE",
                OldData = oldDataJson,
                NewData = newDataJson,
                ChangedBy = pending.ApprovedBy,
                ChangedAt = DateTimeOffset.UtcNow,
                ExternalRequest = externalRequestJson,
                ExternalResponse = message // or raw SOAP if you prefer
            };
            _dbContext.AuditLogs.Add(audit);

            // 8) Save ALL changes and log result (updates VatCollectionTransactions, 
            //    saves the TransactionLog if not already saved, etc.)
            var auditSaveResult = await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Audit log & TransactionLog saved with result: {Result}", auditSaveResult);

            return Ok(new
            {
                message = "VAT collection approved successfully.",
                serviceChargeRef = txnRef,
                serviceChargePayload = svcChargeRequest
            });
        }

        [HttpDelete("vat-collection/{id}")]
        public async Task<IActionResult> DeleteVatCollectionTransaction(int id)
        {
            var record = await _dbContext.VatCollectionTransactions.FindAsync(id);
            if (record == null)
            {
                return NotFound($"Transaction not found for Id={id}");
            }

            _dbContext.VatCollectionTransactions.Remove(record);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "VAT collection transaction deleted successfully." });
        }

        // ====================================================================
        // (7) HELPER: PostCreateTransactionAsync
        // --> Now returns (Success, Message, TxnRef, MsgId)
        // ====================================================================
        private async Task<(bool Success, string Message, string TxnRef, string MsgId)> PostCreateTransactionAsync(CreateTransactionRequest req)
        {
            var client = _httpClientFactory.CreateClient();

            // Propagate the incoming Authorization header if present
            if (Request.Headers.ContainsKey("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", Request.Headers["Authorization"].ToString());
            }

            // Call our own create-transaction endpoint
            var response = await client.PostAsJsonAsync("https://localhost:7033/api/CustomerAccount/create-transaction", req);
            var respJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, $"HTTP {response.StatusCode}: {respJson}", null, null);
            }
            if (respJson.Contains("\"error\""))
            {
                return (false, respJson, null, null);
            }

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(respJson);
                var root = parsed.RootElement;

                // "message" from JSON
                var msg = root.TryGetProperty("message", out var msgElement)
                    ? msgElement.GetString()
                    : "No message";

                // "transactionRef" from JSON
                var txnRef = root.TryGetProperty("transactionRef", out var refElement)
                    ? refElement.GetString()
                    : null;

                // "msgId" from JSON
                var msgId = root.TryGetProperty("msgId", out var msgIdElement)
                    ? msgIdElement.GetString()
                    : null;

                // Return all 4
                return (true, msg, txnRef, msgId);
            }
            catch
            {
                // If parsing fails, return raw JSON as "Message"
                return (true, respJson, null, null);
            }
        }

        // ====================================================================
        // (8) SaveLogAsync
        // ====================================================================
        private async Task SaveLogAsync(string endpoint, string requestXml, string responseXml, string initiator)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Endpoint = endpoint,
                    RequestXml = requestXml,
                    ResponseXml = responseXml,
                    Initiator = initiator,
                    Timestamp = DateTime.UtcNow
                };

                _dbContext.LogEntries.Add(logEntry);
                int result = await _dbContext.SaveChangesAsync();
                if (result > 0)
                {
                    _logger.LogInformation("Log entry saved => ID {LogEntryId}", logEntry.LogEntryId);
                }
                else
                {
                    _logger.LogWarning("SaveChangesAsync returned 0. No log entry saved.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving log entry to DB");
            }
        }

        [HttpGet("vat-collection/status/{status}")]
        public async Task<IActionResult> GetVatCollectionsByStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest("Status parameter is required.");

            var allowedStatuses = new[] { "PENDING", "APPROVED", "REJECTED" };
            if (!allowedStatuses.Contains(status.ToUpper()))
                return BadRequest($"Invalid status provided: {status}");

            try
            {
                var list = await _dbContext.VatCollectionTransactions
                    .AsNoTracking()
                    .Where(x => x.Status.ToUpper() == status.ToUpper())
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving VatCollectionTransactions by status={status}", status);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("vat-collection/{id}/reject")]
        public async Task<IActionResult> RejectVatCollectionTransaction(int id)
        {
            var pending = await _dbContext.VatCollectionTransactions
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == "PENDING");

            if (pending == null)
                return NotFound($"No pending transaction with Id={id}");

            // Mark as REJECTED
            pending.Status = "REJECTED";
            pending.ApprovedDateTime = DateTime.UtcNow;
            pending.ApprovedBy = User?.Identity?.Name ?? "Unknown";

            _dbContext.VatCollectionTransactions.Update(pending);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"VAT collection transaction #{id} rejected." });
        }

        // ================================================
        // NEW ENDPOINT: Get all TransactionLogs
        // ================================================
        [HttpGet("transaction-logs")]
        public async Task<IActionResult> GetAllTransactionLogs()
        {
            try
            {
                var logs = await _dbContext.TransactionLogs
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction logs");
                return StatusCode(500, "Internal server error while retrieving transaction logs.");
            }
        }
    }
}
