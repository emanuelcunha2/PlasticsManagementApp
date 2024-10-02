using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    class MTS
    {
        private string address;
        private int infoPort;
        private string infoProcess;
        private int transferPort;
        private string transferStation;
        private string transferProcess;
        private int maxMsgTotalBytes = 1048576;

        public MTS()
        {
        }

        public static string GetPymsParam(string param)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Application Name=PlasSymbol;Workstation ID=PlasSymbol;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=10"))
                {
                    using (SqlCommand cmd = new SqlCommand(string.Format("SELECT Value FROM tbParam WHERE Parameter = '{0}'", param), cn))
                    {
                        cn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader["Value"] as string;
                            }
                        }
                    }
                }
                return null;

            }
            catch (Exception e)
            {

                throw new Exception("Erro na comunicação com o servidor:" + e.Message + "  => " + e.InnerException.ToString());
            }
        }

        public MTSInfo UnitInfo(string id)
        {
            if (string.IsNullOrEmpty(address))
                address = GetPymsParam("PL_MTS_Server");

            if (infoPort == 0 || string.IsNullOrEmpty(infoProcess))
            {
                var infoParams = GetPymsParam("PL_MTS_Info").Split(new char[] { ';' });
                infoPort = int.Parse(infoParams[0]);
                infoProcess = infoParams[1];
            }

            IPAddress ipaddress = IPAddress.Parse(address);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, infoPort);

            var bcmp = string.Format("BCMP|id={0}|process={1}\n", id, infoProcess);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(endpoint);

                socket.Send(Encoding.ASCII.GetBytes(bcmp));

                StringBuilder builder = new StringBuilder();

                byte[] buffer = new byte[8192];
                var totalbytes = 0;
                var bytecount = 0;
                do
                {
                    bytecount = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    totalbytes += bytecount;
                    builder.Append(Encoding.ASCII.GetString(buffer, 0, bytecount));

                } while ((socket.Available > 0 || (bytecount > 0 && buffer[bytecount - 1] != 0x0A)) && totalbytes < maxMsgTotalBytes);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                var info = ProcessInfoReply(builder.ToString());

                if (info.Status == "FAIL")
                    throw new Exception(string.Format("MTS Error: {0}", info.Msg));

                return info;
            }
        }

        private MTSInfo ProcessInfoReply(string msg)
        {
            if (!msg.StartsWith("BACK"))
                throw new Exception("Invalid response from MTS server.");

            msg = msg.Remove(msg.Length - 1, 1);

            var reply = new MTSInfo();

            var param = msg.Split('|');

            for (int i = 1, length = param.Length; i < length; i++)
            {
                var item = param[i];

                if (item.StartsWith("station") || item.StartsWith("process") || item.StartsWith("version"))
                    continue;

                if (item.StartsWith("id"))
                    reply.ID = ExtractValue("id", item);
                else if (item.StartsWith("status"))
                    reply.Status = ExtractValue("status", item);
                else if (item.StartsWith("msg"))
                    reply.Msg = ExtractValue("msg", item);
                else if (item.StartsWith("uk2"))
                    reply.PN = ExtractValue("uk2", item);
                else if (item.StartsWith("uk1"))
                    reply.Location = ExtractValue("uk1", item);
                else if (item.StartsWith("quantity"))
                    reply.Qty = int.Parse(ExtractValue("quantity", item));
            }

            return reply;

        }

        public void Transfer(string id, string destination)
        {
            if (string.IsNullOrEmpty(address))
                address = GetPymsParam("PL_MTS_Server");

            if (transferPort == 0 || string.IsNullOrEmpty(transferProcess) || string.IsNullOrEmpty(transferStation))
            {
                var transferParams = GetPymsParam("PL_MTS_Transfer").Split(new char[] { ';' });
                transferPort = int.Parse(transferParams[0]);
                transferProcess = transferParams[1];
                transferStation = transferParams[2];
            }

            IPAddress ipaddress = IPAddress.Parse(address);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, transferPort);

            var bcmp = string.Format("BCMP|id={0}|process={1}|station={2}|dest={3}|pos=OFFLINE\n",
                id, transferProcess, transferStation, destination);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(endpoint);

                socket.Send(Encoding.ASCII.GetBytes(bcmp));

                StringBuilder builder = new StringBuilder();

                byte[] buffer = new byte[8192];
                var totalbytes = 0;
                var bytecount = 0;
                do
                {
                    bytecount = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    totalbytes += bytecount;
                    builder.Append(Encoding.ASCII.GetString(buffer, 0, bytecount));

                } while ((socket.Available > 0 || (bytecount > 0 && buffer[bytecount - 1] != 0x0A)) && totalbytes < maxMsgTotalBytes);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                var info = ProcessTransferReply(builder.ToString());

                if (info.Status == "FAIL")
                    throw new Exception(string.Format("MTS Error: {0}", info.Msg));
            }

        }

        private MTSInfo ProcessTransferReply(string msg)
        {
            if (!msg.StartsWith("BACK"))
                throw new Exception("Invalid response from MTS server.");

            msg = msg.Remove(msg.Length - 1, 1);

            var reply = new MTSInfo();

            var param = msg.Split('|');

            for (int i = 1, length = param.Length; i < length; i++)
            {
                var item = param[i];

                if (item.StartsWith("station") || item.StartsWith("process") || item.StartsWith("version"))
                    continue;

                if (item.StartsWith("id"))
                    reply.ID = ExtractValue("id", item);
                else if (item.StartsWith("status"))
                    reply.Status = ExtractValue("status", item);
                else if (item.StartsWith("msg"))
                    reply.Msg = ExtractValue("msg", item);
            }

            return reply;

        }

        private string ExtractValue(string paramName, string paramMsg)
        {
            var fields = paramMsg.Split(new char[] { '=' });
            if (fields.Length != 2)
                throw new Exception(string.Format("Invalid {0} parameter format in MTS response", paramName));
            return fields[1];
        }
    }

    public class MTSInfo
    {
        public string ID { get; set; }
        public string PN { get; set; }
        public string Status { get; set; }
        public string Msg { get; set; }
        public int Qty { get; set; }
        public string Location { get; set; }
    }
}
