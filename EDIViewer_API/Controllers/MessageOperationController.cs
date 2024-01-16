using Azure;
using EDIViewer_Core.Interfaces.MessageOperation;
using EDIViewer_DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EDIViewer_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageOperationController : ControllerBase
    {
        private readonly IMessageOperation _MessageOperation;
        public MessageOperationController(IMessageOperation messageOperation)
        {
                _MessageOperation = messageOperation;
        }



        /// <summary>
        /// Post The Messages from File To DB
        /// </summary>
        /// <returns></returns>
        [HttpPost("SaveEDIMessageIntoDB")]
        public async Task<ActionResult<int>> SaveEDIMessageToDB()
        {
            var result = await _MessageOperation.SaveEDIMessageToDB();

            if (result > 0)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponceStatus
                {
                    StatusCode = "200",
                    ApiResponce = "Message Added Successfully" });
                }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new APIResponceStatus
                {
                    StatusCode = "400",
                    ApiResponce = "Something is wrong Please check it.."
                });
            }
        }



        /// <summary>
        /// Get All Messages From Database
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAllLessages")]
        public async Task<ActionResult<List<Messages>>> GetAllEDIMessages()
        {
            var result = await _MessageOperation.GetAllEDIMessages();
            return Ok(result);
        }



        /// <summary>
        /// Save Copayement Of P2P Messages
        /// </summary>
        /// <returns></returns>
        [HttpGet("SaveEligibilityP2PMessage")]
        public async Task<IActionResult> SaveEligibilityP2PMessage()
        {
            var Result = await _MessageOperation.SaveEligibilityP2PMessage();
            if(Result > 0)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponceStatus
                {
                    StatusCode = "200",
                    ApiResponce = "Co-Payment Added Successfully"
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new APIResponceStatus
                {
                    StatusCode = "400",
                    ApiResponce = "Something is wrong Please check it.."
                });
            }
        }


    }
}
