using System;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace StrawhatNet.NFC.FelicaReader
{
    public class FelicaReader
    {
        protected Felica.AccessHandler felicaAccess;
        protected SmartCardConnection connection;

        public FelicaReader(SmartCardConnection connection)
        {
            this.connection = connection;
            this.felicaAccess = new Felica.AccessHandler(connection);
        }

        public async Task<byte[]> GetUid()
        {
            byte[] uid = await felicaAccess.GetUidAsync();
            return uid;
        }

        public async Task<byte[]> Polling(UInt16 systemCode)
        {
            byte systemCodeHigher = (byte)(systemCode >> 8);
            byte systemCodeLower = (byte)(systemCode & 0x00ff);

            byte[] commandData = new byte[] {
               0x00, 0x00, systemCodeHigher, systemCodeLower, 0x01, 0x0f,
            };
            commandData[0] = (byte)commandData.Length;

            byte[] result = await felicaAccess.TransparentExchangeAsync(commandData);

            byte[] idm = new byte[8];
            Array.Copy(result, 2, idm, 0, idm.Length);

            return idm;
        }

        public async Task<ReadWithoutEncryptionResponse> ReadWithoutEncryption(
            byte[] idm,
            UInt16 serviceCode,
            byte blockNumber,
            byte[] blockList)
        {
            byte serviceCodeHigher = (byte)(serviceCode >> 8);
            byte serviceCodeLower = (byte)(serviceCode & 0x00ff);

            byte[] commandDataPrefix = new byte[] {
                0x00,
                0x06,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01,
                serviceCodeLower, serviceCodeHigher,
                blockNumber,
                // block list
            };

            for (int i = 0; i < idm.Length; i++)
            {
                commandDataPrefix[i + 2] = idm[i];
            }

            byte[] commandData = new byte[commandDataPrefix.Length + blockList.Length];
            Array.Copy(commandDataPrefix, commandData, commandDataPrefix.Length);
            Array.Copy(blockList, 0, commandData, commandDataPrefix.Length, blockList.Length);
            commandData[0] = (byte)commandData.Length;

            byte[] result = await felicaAccess.TransparentExchangeAsync(commandData);
            return ReadWithoutEncryptionResponse.ParsePackage(result);
        }
    }
}
