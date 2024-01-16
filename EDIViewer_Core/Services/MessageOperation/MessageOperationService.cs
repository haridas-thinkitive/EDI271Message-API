using EdiFabric.Core.Model.Edi;
using EdiFabric;
using EdiFabric.Framework.Readers;
using EDIViewer_Core.Interfaces.MessageOperation;
using EDIViewer_DataAccess.Data;
using EDIViewer_DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdiFabric.Templates.Hipaa5010;

namespace EDIViewer_Core.Services.MessageOperation
{
    public class MessageOperationService : IMessageOperation
    {
        private readonly ApplicationDbContext _Context;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<MessageOperationService> _Logger;

        private readonly string _service_Code_Type;
        private readonly string _segmentDelimiter;
        private readonly string _DataElementDelimiter;
        private readonly string _InterchangeControlStandardsIdentifier_11;
        private readonly string _SPMsgCheck;
        private readonly string _PCMsgCheck;
        private readonly string _ServiceCode_T;
        private readonly string _ServiceCode_ST;
        private readonly string _ServiceCode_CH;
        private readonly string _timePeriodQualifier_06;
        private readonly string _inPlanNetworkIndicator_12;
        public MessageOperationService(ApplicationDbContext applicationDbContext,IConfiguration configuration,ILogger<MessageOperationService> logger)
        {
            _Context = applicationDbContext;
            _Configuration = configuration;
            _Logger = logger;

            _service_Code_Type = _Configuration["MessageSettings:Service_Code_Type"];
            _DataElementDelimiter = _Configuration["MessageSettings:DataElementDelimiter"];
            _SPMsgCheck = _Configuration["MessageSettings:SPMsgCheck"];
            _PCMsgCheck = _Configuration["MessageSettings:PCMsgCheck"];
            _InterchangeControlStandardsIdentifier_11 = _Configuration["MessageSettings:InterchangeControlStandardsIdentifier_11"];
            _segmentDelimiter = _Configuration["MessageSettings:SegmentDelimiter"];

            _ServiceCode_T = _Configuration["MessageSettings:ServiceCode_T"];
            _ServiceCode_ST = _Configuration["MessageSettings:ServiceCode_ST"];
            _ServiceCode_CH = _Configuration["MessageSettings:ServiceCode_CH"];
            _timePeriodQualifier_06 = _Configuration["MessageSettings:TimePeriodQualifier_06"];
            _inPlanNetworkIndicator_12 = _Configuration["MessageSettings:InPlanNetworkIndicator_12"];

        }

        public async Task<int> SaveEDIMessageToDB()
        {
            try
            {
                int Result = 0;
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), _Configuration["FileSettings:FileName"]);

                if (!System.IO.File.Exists(filePath))
                {
                    return Result;
                }

                string hl7Message = System.IO.File.ReadAllText(filePath);

                if (!string.IsNullOrEmpty(hl7Message))
                {
                    var message = new Messages { Response = hl7Message };
                    await _Context.Messages.AddAsync(message);
                    await _Context.SaveChangesAsync();
                    _Logger.LogInformation($"Successfully Added Message Into DB: {message.EncounterId}");
                    return Result = 1;
                }
                return Result;

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Messages>> GetAllEDIMessages()
        {
            try
            {
                List<Messages> result = await _Context.Messages.ToListAsync();

                if (result != null)
                {
                    _Logger.LogInformation("Information Retrieved Successfully");
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "An error occurred while retrieving EDI messages.");
                throw;
            }
        }

        public async Task<int> SaveEligibilityP2PMessage()
        {
            try
            {
                var messagesData = await _Context.Messages.ToListAsync();
                SerialKey.Set("c417cb9dd9d54297a55c032a74c87996");

                foreach (var messageText in messagesData)
                {
                    var ids = messageText.EncounterId;
                    if (ids == 11924055)
                    {
                        var dd = "1";
                    }
                    try
                    {
                        var ediString = messageText?.Response?.ToString();

                        if (!string.IsNullOrEmpty(ediString))
                        {
                            using (var ediStream = new MemoryStream(Encoding.ASCII.GetBytes(ediString)))
                            {
                                List<IEdiItem> ediItems;
                                using (var ediReader = new X12Reader(ediStream, "EdiFabric.Templates.Hipaa"))
                                {
                                    ediItems = ediReader.ReadToEnd().ToList();
                                    var transactions = ediItems.OfType<TS271>();

                                    var rescheck = transactions
                                    .SelectMany(ts271 => ts271.Loop2000A.Take(1))
                                    .SelectMany(loop2000A => loop2000A?.Loop2000B.Take(1))
                                    .SelectMany(loop2000B => loop2000B?.Loop2000C.Take(1))
                                    .Select(loop2000C => loop2000C?.Loop2100C)
                                    .Where(loop2100C => loop2100C.Loop2110C == null)
                                    .ToList();

                                    if (rescheck.Count > 0)
                                    {
                                            var ActRes = transactions
                                                .SelectMany(ts271 => ts271.Loop2000A.Take(1))
                                                .SelectMany(loop2000A => loop2000A?.Loop2000B.Take(1))
                                                .SelectMany(loop2000B => loop2000B?.Loop2000C.Take(1))
                                                .Select(loop2000C => loop2000C?.Loop2000D)
                                                .Where(loop2000D => loop2000D != null)
                                                .SelectMany(loop2000D => loop2000D.Take(1))
                                                .Select(loop2100D => loop2100D.Loop2100D)
                                                .SelectMany(loop2110D => loop2110D.Loop2110D)
                                                .Where(loop2110D =>
                                                    loop2110D?.EB_DependentEligibilityorBenefitInformation != null &&
                                                    loop2110D.EB_DependentEligibilityorBenefitInformation.ServiceTypeCode_03?.Any(cd => cd == _service_Code_Type) == true &&
                                                    //loop2110D.EB_DependentEligibilityorBenefitInformation.ServiceTypeCode_03?.Any(code => code == "BN") == true && 
                                                    loop2110D.EB_DependentEligibilityorBenefitInformation.EligibilityorBenefitInformation_01 == "B")


                                            .Select(loop2110D => new TestMessage
                                            {
                                                SubMessages = loop2110D.MSG_MessageText
                                                ?.Where(text => text?.FreeFormMessageText_01?.Contains(_SPMsgCheck) == true || text?.FreeFormMessageText_01?.Contains(_PCMsgCheck) == true)
                                                .Select(text => new SubMessageData
                                                {
                                                    messageListDatas = new List<MessageListData>
                                                    {
                                                        new MessageListData
                                                        {
                                                            FreeMessageText_01 = string.Join(", ", loop2110D.MSG_MessageText.Select(msg => msg.FreeFormMessageText_01)),
                                                            Segment = ConactMessageLoop2110D(loop2110D),
                                                            MessageType = GetMessageTypes(text.FreeFormMessageText_01),
                                                            CoPayment = loop2110D.EB_DependentEligibilityorBenefitInformation.BenefitAmount_07,
                                                            EncounterID = messageText.EncounterId  ,
                                                            Logmessage = "Successfully Co-Pay processed with ID:"+ messageText.EncounterId,
                                                            InsuranceDate = GetDTP_SubscriberDate(transactions)
                                                        }
                                                    }
                                                }).ToList()
                                            })
                                            .Where(testMessage => testMessage.SubMessages?.Any(subMessage =>
                                                subMessage.messageListDatas?.Any(data =>
                                                    data.FreeMessageText_01?.Contains(_SPMsgCheck) == true ||
                                                    data.FreeMessageText_01?.Contains(_PCMsgCheck) == true) == true) == true)
                                            .ToList();

                                            //await  _context.testMessages.AddRangeAsync(ActRes);
                                            var recordsToSave = ActRes
                                            .SelectMany(resultData => resultData.SubMessages?.SelectMany(sub => sub.messageListDatas))
                                            .Where(messageListData =>
                                                messageListData != null &&
                                                messageListData.Segment != null &&
                                                messageListData.Segment.Contains(_timePeriodQualifier_06) &&
                                                messageListData.Segment.Contains(_inPlanNetworkIndicator_12))
                                            .ToList();

                                            var testMessagesToSave = new List<TestMessage>
                                            {
                                                new TestMessage
                                                {
                                                    SubMessages = new List<SubMessageData>
                                                    {
                                                        new SubMessageData
                                                        {
                                                            messageListDatas = recordsToSave
                                                        }
                                                    }
                                                }
                                            };

                                            await _Context.testMessages.AddRangeAsync(testMessagesToSave);
                                            await _Context.SaveChangesAsync();
                                            _Logger.LogInformation($"Successfully processed message with ID: {messageText.EncounterId}");
                                        }
                                    else if (rescheck.Count <= 0)
                                    {
                                        var messageTextList = transactions
                                        .SelectMany(ts271 => ts271.Loop2000A.Take(1))
                                        .SelectMany(loop2000A => loop2000A?.Loop2000B.Take(1))
                                        .SelectMany(loop2000B => loop2000B?.Loop2000C.Take(1))
                                        .Select(loop2000C => loop2000C?.Loop2100C)
                                        .SelectMany(loop2100C => loop2100C?.Loop2110C)
                                        .Where(loop2110C => loop2110C?.EB_SubscriberEligibilityorBenefitInformation != null &&
                                                           loop2110C.EB_SubscriberEligibilityorBenefitInformation.ServiceTypeCode_03?.Any(cd => cd == _service_Code_Type) == true &&
                                                           //loop2110C.EB_SubscriberEligibilityorBenefitInformation.ServiceTypeCode_03?.Any(code => code == "BN") == true && 
                                                           loop2110C.EB_SubscriberEligibilityorBenefitInformation.EligibilityorBenefitInformation_01 == "B")
                                        .Select(loop2110C => new TestMessage
                                        {
                                            SubMessages = loop2110C.MSG_MessageText
                                                ?.Where(text => text?.FreeFormMessageText_01?.Contains(_SPMsgCheck) == true || text?.FreeFormMessageText_01?.Contains(_PCMsgCheck) == true)
                                                .Select(text => new SubMessageData
                                                {
                                                    messageListDatas = new List<MessageListData>
                                                    {
                                                        new MessageListData
                                                        {
                                                            FreeMessageText_01 = string.Join(", ", loop2110C.MSG_MessageText.Select(msg => msg.FreeFormMessageText_01)),
                                                            Segment = ConcatMessageLoop2110C(loop2110C),
                                                            MessageType = GetMessageTypes(text.FreeFormMessageText_01),

                                                            CoPayment = loop2110C.EB_SubscriberEligibilityorBenefitInformation.BenefitAmount_07,
                                                            EncounterID = messageText.EncounterId,
                                                            Logmessage = "Successfully Co-Pay processed with ID:"+ messageText.EncounterId,
                                                            InsuranceDate = Get_SubscriberDate(transactions)

                                                        }
                                                    }
                                                }).ToList()
                                        })
                                        .Where(testMessage => testMessage.SubMessages?.Any(subMessage => subMessage.messageListDatas?.Any(data => data.FreeMessageText_01?.Contains(_SPMsgCheck) == true || data.FreeMessageText_01?.Contains(_PCMsgCheck) == true) == true) == true)
                                        .ToList();

                                        //await _context.testMessages.AddRangeAsync(messageTextList);

                                        var recordsToSave = messageTextList
                                        .SelectMany(resultData => resultData.SubMessages?.SelectMany(sub => sub.messageListDatas))
                                        .Where(messageListData =>
                                            messageListData != null &&
                                            messageListData.Segment != null &&
                                            messageListData.Segment.Contains(_timePeriodQualifier_06) &&
                                            messageListData.Segment.Contains(_inPlanNetworkIndicator_12))
                                        .ToList();

                                        var testMessagesToSave = new List<TestMessage>
                                        {
                                            new TestMessage
                                            {
                                                SubMessages = new List<SubMessageData>
                                                {
                                                    new SubMessageData
                                                    {
                                                        messageListDatas = recordsToSave
                                                    }
                                                }
                                            }
                                        };

                                        await _Context.testMessages.AddRangeAsync(testMessagesToSave);
                                        await _Context.SaveChangesAsync();
                                        _Logger.LogInformation($"Successfully processed message with ID: {messageText.EncounterId}");

                                    }
                                }
                            }
                        }
                        else
                        {
                            _Logger.LogError($"Empty or null EDI string for message with ID: {messageText.EncounterId}");
                            return 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, $"Error processing message with ID: {messageText.EncounterId}");
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "An error occurred while processing eligibility P2P messages.");
                throw;
            }
        }

        private string ConactMessageLoop2110D(Loop_2110D_271 loop2110D)
        {
            string ServiceCodeMessage = _segmentDelimiter + _DataElementDelimiter +
                                        loop2110D.EB_DependentEligibilityorBenefitInformation.EligibilityorBenefitInformation_01 + _DataElementDelimiter +
                                        new string(loop2110D.EB_DependentEligibilityorBenefitInformation.BenefitCoverageLevelCode_02) +
                                        _DataElementDelimiter;
            string[] serviceTypeCodeArray = loop2110D.EB_DependentEligibilityorBenefitInformation.ServiceTypeCode_03.ToArray();
            foreach (var serviceTypeCode in serviceTypeCodeArray)
            {
                bool firstConditionMet = false;

                if (serviceTypeCode == _ServiceCode_T)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _ServiceCode_ST)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _ServiceCode_CH)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _service_Code_Type)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += new string(_DataElementDelimiter[0], 3);
                    }
                }
            }
            ServiceCodeMessage += loop2110D.EB_DependentEligibilityorBenefitInformation.TimePeriodQualifier_06 +
                                    _DataElementDelimiter +
                                    loop2110D.EB_DependentEligibilityorBenefitInformation.BenefitAmount_07 +
                                    new string(_DataElementDelimiter[0], 4) +
                                    loop2110D.EB_DependentEligibilityorBenefitInformation.AuthorizationorCertificationIndicator_11 +
                                    _DataElementDelimiter +
                                    loop2110D.EB_DependentEligibilityorBenefitInformation.InPlanNetworkIndicator_12;

            return ServiceCodeMessage;
        }

        private string GetMessageTypes(string freeFormMessageText)
        {
            var messageTypes = new List<string>();

            if (freeFormMessageText.Contains(_PCMsgCheck))
            {
                messageTypes.Add(_PCMsgCheck);
            }

            if (freeFormMessageText.Contains(_SPMsgCheck))
            {
                messageTypes.Add(_SPMsgCheck);
            }

            return string.Join(", ", messageTypes);
        }

        private string ConcatMessageLoop2110C(Loop_2110C_271 loop2110C)
        {
            string ServiceCodeMessage = _segmentDelimiter + _DataElementDelimiter +
                                        loop2110C.EB_SubscriberEligibilityorBenefitInformation.EligibilityorBenefitInformation_01 + _DataElementDelimiter +
                                        new string(loop2110C.EB_SubscriberEligibilityorBenefitInformation.BenefitCoverageLevelCode_02) +
                                        _DataElementDelimiter;
            string[] serviceTypeCodeArray = loop2110C.EB_SubscriberEligibilityorBenefitInformation.ServiceTypeCode_03.ToArray();
            foreach (var serviceTypeCode in serviceTypeCodeArray)
            {
                bool firstConditionMet = false;

                if (serviceTypeCode == _ServiceCode_T)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _ServiceCode_ST)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _ServiceCode_CH)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += _InterchangeControlStandardsIdentifier_11;
                    }
                }
                else if (serviceTypeCode == _service_Code_Type)
                {
                    ServiceCodeMessage += serviceTypeCode;
                    firstConditionMet = true;
                    if (firstConditionMet)
                    {
                        ServiceCodeMessage += new string(_DataElementDelimiter[0], 3);
                    }
                }
            }
            ServiceCodeMessage += loop2110C.EB_SubscriberEligibilityorBenefitInformation.TimePeriodQualifier_06 +
                                    _DataElementDelimiter +
                                    loop2110C.EB_SubscriberEligibilityorBenefitInformation.BenefitAmount_07 +
                                    new string(_DataElementDelimiter[0], 4) +
                                    loop2110C.EB_SubscriberEligibilityorBenefitInformation.AuthorizationorCertificationIndicator_11 +
                                    _DataElementDelimiter +
                                    loop2110C.EB_SubscriberEligibilityorBenefitInformation.InPlanNetworkIndicator_12;

            return ServiceCodeMessage;

        }

        private string GetDTP_SubscriberDate(IEnumerable<TS271> transactions)
        {
            if (transactions != null)
            {
                foreach (var transaction in transactions)
                {
                    var dates = transaction
                    .Loop2000A.Take(1)
                    .SelectMany(loop2000A => loop2000A?.Loop2000B.Take(1))
                    .SelectMany(loop2000B => loop2000B?.Loop2000C.Take(1))
                    .SelectMany(loop2000C => loop2000C?.Loop2000D)
                    .Select(loop2000D => loop2000D.Loop2100D)
                    .SelectMany(loop2100D => loop2100D.DTP_DependentDate)
                    .ToList();

                    var firstDateTimePeriod = dates
                    .Select(date => date?.DateTimePeriod_03)
                    .FirstOrDefault(dateTimePeriod => !string.IsNullOrEmpty(dateTimePeriod));

                    if (!string.IsNullOrEmpty(firstDateTimePeriod))
                    {
                        return firstDateTimePeriod;
                    }
                }

            }
            return null;
        }

        private string Get_SubscriberDate(IEnumerable<TS271> transactions)
        {
            try
            {
                if (transactions != null)
                {
                    foreach (var transaction in transactions)
                    {

                        var dates = transaction
                            .Loop2000A.Take(1)
                            .SelectMany(loop2000A => loop2000A?.Loop2000B.Take(1))
                            .SelectMany(loop2000B => loop2000B?.Loop2000C.Take(1))
                            .Select(loop2000C => loop2000C?.Loop2100C)
                            .Where(loop2100C => loop2100C != null && loop2100C.DTP_SubscriberDate != null)
                            .SelectMany(loop2100C => loop2100C.DTP_SubscriberDate)
                            .FirstOrDefault();
                        if (dates != null)
                            return dates.DateTimePeriod_03;
                        else
                            return "DTP_SubscriberDate is not available";
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
