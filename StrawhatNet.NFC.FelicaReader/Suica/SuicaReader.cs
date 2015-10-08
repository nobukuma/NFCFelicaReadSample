using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace StrawhatNet.NFC.FelicaReader.Suica
{
    /// <summary>
    /// Suicaなどのサイバネ規格ICカード乗車券の読み取りクラス
    /// 結果のフォーマットは以下を参照
    /// http://iccard.jennychan.at-ninja.jp/format/suica.xml
    /// </summary>
    public class SuicaReader : FelicaReader
    {
        private const ushort SystemCode = 0x0003;

        public SuicaReader(SmartCardConnection connection)
            : base(connection)
        {
        }

        public async Task<byte[]> Polling()
        {
            return await base.Polling(SystemCode);
        }

        /// <summary>
        /// 属性情報 (service code: 0x008b, block: 1)
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetAttributeInfo(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x008b, 0x01, new byte[] { 0x80, 0x00, });
            return result.BlockData;
        }

        /// <summary>
        /// 利用履歴 (service code: 0x090f, block: max 20)
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetUsageHistory(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x090f, 0x01, new byte[] { 0x80, 0x00, });
            return result.BlockData;
        }

        /// <summary>
        /// 改札入出場履歴 (service code: 0x108f, block: max 3)
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetTicketGateEnterLeaveHistory(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x108f, 0x01, new byte[] { 0x80, 0x00, });
            return result.BlockData;
        }

        /// <summary>
        /// SF入場駅記録 (service code: 0x10cb, block: 2)
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetSFEnteredStationInfo(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x10cb, 0x01, new byte[] { 0x80, 0x00, });
            return result.BlockData;
        }

        /// <summary>
        /// 料金 発券/改札記録 (service code: 0x184b, block: 1)
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetTicketIssueInspectRecord(byte[] idm)
        {
            ReadWithoutEncryptionResponse result = await base.ReadWithoutEncryption(idm, 0x184b, 0x01, new byte[] { 0x80, 0x00, });
            return result.BlockData;
        }
    }
}
