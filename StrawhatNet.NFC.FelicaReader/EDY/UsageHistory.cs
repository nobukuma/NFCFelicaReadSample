using System;

namespace StrawhatNet.NFC.FelicaReader.EDY
{
    public class UsageHistory
    {
        public bool HasData
        {
            get
            {
                return PacketData != null && PacketData.Length > 0;
            }
        }
        public byte[] PacketData { get; set; }
        public byte UsageType { get; set; }
        public ushort RunningNumber { get; set; }
        public DateTime UsedDateTime { get; set; }

        public decimal UsedAmount { get; set; }

        public decimal Balance { get; set; }

        public static UsageHistory GetUsageHistory(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return new UsageHistory()
                {
                    PacketData = new byte[0],
                };
            }

            // カード情報の解析
            // data: 利用区分 0x00 取引通番(2 byte) 利用日時(4 byte) 利用金額(4 byte) 残額(4 byte)

            byte usageType = data[0];
            int runningNumber = (data[2] << 8) | data[3];
            int usedDateTimeValue = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7];
            int usedAmountValue = (data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11];
            int balanceValue = (data[12] << 24) | (data[13] << 16) | (data[14] << 8) | data[15];

            // 利用日時(4 byte) = 利用日付(15bit) 通算秒数(17bit)
            // 2000/01/01からの通算日数
            // 当日の0:0:0からの通算秒数
            int usedDate = (int)((uint)usedDateTimeValue >> 17);
            int runningSeconds = (int)((uint)usedDateTimeValue & 0x1ffff);
            DateTime usedDateTime = new DateTime(2000, 1, 1, 0, 0, 0).AddDays(usedDate).AddSeconds(runningSeconds);

            return new UsageHistory()
            {
                PacketData = data,
                UsageType = usageType,
                RunningNumber = (ushort)runningNumber,
                UsedDateTime = usedDateTime,
                UsedAmount = (decimal)usedAmountValue,
                Balance = (decimal)balanceValue,
            };
        }
    }
}
