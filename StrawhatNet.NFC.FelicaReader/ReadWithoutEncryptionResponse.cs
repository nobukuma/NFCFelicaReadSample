using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrawhatNet.NFC.FelicaReader
{
    public class ReadWithoutEncryptionResponse
    {
        public byte[] PacketData { get; set; }

        public byte[] IDm { get; set; }

        public byte StatusFlag1 { get; set; }

        public byte StatusFlag2 { get; set; }

        public byte BlockNumber { get; set; }

        public byte[] BlockData { get; set; }

        public static ReadWithoutEncryptionResponse ParsePackage(
            byte[] packatData)
        {
            // カード情報の解析
            // len 0x07 IDm(8 byte) status(2 byte) block数 <<data>>
            byte responseLen = packatData[0];

            byte[] idm = new byte[8];
            Array.Copy(packatData, 2, idm, 0, idm.Length);

            byte status1 = packatData[10];
            byte status2 = packatData[11];
            byte outBlockNumber = packatData[12];

            byte[] data = new byte[outBlockNumber * 16];
            Array.Copy(packatData, 13, data, 0, data.Length);

            return new ReadWithoutEncryptionResponse()
            {
                PacketData = packatData,
                IDm = idm,
                StatusFlag1 = status1,
                StatusFlag2 = status2,
                BlockNumber = outBlockNumber,
                BlockData = data,
            };
        }
    }
}
