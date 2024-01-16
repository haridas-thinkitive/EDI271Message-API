using EDIViewer_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDIViewer_Core.Interfaces.MessageOperation
{
    public interface IMessageOperation
    {
        Task<List<Messages>> GetAllEDIMessages();
        Task<int> SaveEDIMessageToDB();

        Task<int> SaveEligibilityP2PMessage();



    }
}
