using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace StrawhatNet.NFC.FelicaReader.EDY
{
    public class EDYReader : FelicaReader
    {
        private const ushort SystemCode = 0xfe00;

        public EDYReader(SmartCardConnection connection)
            : base(connection)
        {
        }

        public async Task<byte[]> Polling()
        {
            return await base.Polling(SystemCode);
        }

        /// <summary>
        /// 属性情報 (service code: 0x110b, block: 2)
        /// </summary>
        /// <param name="idm"></param>
        /// <returns></returns>
        public async Task<byte[]> GetAttribute(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x110b, 0x02, new byte[] { 0x80, 0x00, 0x80, 0x01 });
            return result.BlockData;
        }

        /// <summary>
        /// 残額情報 (service code: 0x1317, block: 1)
        /// </summary>
        /// <param name="idm"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBalance(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x1317, 0x01, new byte[] { 0x80, 0x00 });
            return result.BlockData;
        }

        /// <summary>
        /// 利用履歴 (service code: 0x170f, block: max 6)
        /// 結果の解析の例
        /// </summary>
        /// <param name="idm"></param>
        /// <returns></returns>
        public async Task<UsageHistory> GetUsageHistory(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x170f, 0x01, new byte[] { 0x80, 0x00, });
            return UsageHistory.GetUsageHistory(result.BlockData);
        }
    }
}
