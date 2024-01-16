using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDIViewer_DataAccess.Models
{
    public class TestMessage
    {
        public int Id { get; set; }
        public List<SubMessageData> SubMessages { get; set; }

    }

    public class SubMessageData
    {
        public int Id { get; set; }
        public List<MessageListData> messageListDatas { get; set; }
    }

    public class MessageListData
    {
        public int Id { get; set; }
        public string FreeMessageText_01 { get; set; }

        public string Segment { get; set; }

        public string MessageType { get; set; }

        public string CoPayment { get; set; }

        public int EncounterID { get; set; }

        public string Logmessage { get; set; }

        public string InsuranceDate { get; set; }


    }
}
