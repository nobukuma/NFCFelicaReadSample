using Pcsc;
using Pcsc.Common;
using StrawhatNet.NFC.FelicaReader.EDY;
using StrawhatNet.NFC.FelicaReader.Suica;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace StrawhatNet.Study.NFCFelicaRead
{
    public sealed partial class MainPage : Page
    {
        private SmartCardReader m_cardReader;
        private bool isEDY_Checked;
        private bool isSuica_Checked;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await LogMessageAsync(String.Empty, true);

            this.UpdateTargetCardFlag();

            // First try to find a reader that advertises as being NFC
            var deviceInfo = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Nfc);
            if (deviceInfo == null)
            {
                deviceInfo = await SmartCardReaderUtils.GetFirstSmartCardReaderInfo(SmartCardReaderKind.Any);
            }

            if (deviceInfo == null)
            {
                await LogMessageAsync("NFCカードリーダがサポートされていません");
                return;
            }

            if (!deviceInfo.IsEnabled)
            {
                var msgbox = new Windows.UI.Popups.MessageDialog("設定画面でNFCをONにしてください");
                msgbox.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                await msgbox.ShowAsync();

                LaunchNfcProximitySettingsPage();
                return;
            }

            if (m_cardReader == null)
            {
                m_cardReader = await SmartCardReader.FromIdAsync(deviceInfo.Id);
                m_cardReader.CardAdded += cardReader_CardAdded;
                m_cardReader.CardRemoved += cardReader_CardRemoved;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (m_cardReader != null)
            {
                m_cardReader.CardAdded -= cardReader_CardAdded;
                m_cardReader.CardRemoved -= cardReader_CardRemoved;
                m_cardReader = null;
            }
        }

        private async void cardReader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
        {
            await LogMessageAsync("カードが取り除かれました");
        }

        private async void cardReader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            SmartCard smartCard = args.SmartCard;

            try
            {
                await LogMessageAsync("カードを検知しました", true);

                using (SmartCardConnection connection = await smartCard.ConnectAsync())
                {
                    IccDetection cardIdentification = new IccDetection(smartCard, connection);
                    await cardIdentification.DetectCardTypeAync();

                    await LogMessageAsync("PC/SCデバイスクラス: " + cardIdentification.PcscDeviceClass.ToString());
                    await LogMessageAsync("カード名: " + cardIdentification.PcscCardName.ToString());

                    if (cardIdentification.PcscDeviceClass == Pcsc.Common.DeviceClass.StorageClass
                        && cardIdentification.PcscCardName == CardName.FeliCa)
                    {
                        await LogMessageAsync("FelicaCardがみつかりました");

                        if (isEDY_Checked)
                        {
                            await ReadEdyAsync(connection);
                        }
                        else
                        {
                            await ReadSuicaAsync(connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogMessageAsync("例外が発生: " + ex.ToString());
            }
        }

        private async Task ReadEdyAsync(SmartCardConnection connection)
        {
            EDYReader reader = new EDYReader(connection);

            // システムコードを指定してPolling
            await LogMessageAsync("EDYカードをポーリング: ");
            byte[] idm = await reader.Polling();

            if (idm.Length > 0)
            {
                await LogMessageAsync(BitConverter.ToString(idm));

                // ReadWithoutEncryptionでサービスコードを指定してデータ取得

                // 属性情報
                byte[] attributeData = await reader.GetAttribute(idm);
                await LogMessageAsync("属性情報: " + BitConverter.ToString(attributeData));

                // 残額情報
                byte[] balanceData = await reader.GetBalance(idm);
                await LogMessageAsync("残額情報: " + BitConverter.ToString(balanceData));

                // 利用履歴
                // 参考) http://iccard.jennychan.at-ninja.jp/format/edy.html#170F
                UsageHistory result = await reader.GetUsageHistory(idm);
                await LogMessageAsync("利用履歴: " + BitConverter.ToString(result.PacketData));

                if (result.HasData)
                {
                    await LogMessageAsync("■利用履歴の解析結果");
                    await LogMessageAsync("利用区分: 0x" + result.UsageType.ToString("X"));
                    await LogMessageAsync("取引通番: " + result.RunningNumber);
                    await LogMessageAsync("利用日時: " + result.UsedDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    await LogMessageAsync("利用金額: " + result.UsedAmount);
                    await LogMessageAsync("残額: " + result.Balance);
                }
            }

            return;
        }

        private async Task ReadSuicaAsync(SmartCardConnection connection)
        {
            byte[] result;

            SuicaReader reader = new SuicaReader(connection);

            // システムコードを指定してPolling
            await LogMessageAsync("Suicaカードをポーリング: ");
            byte[] idm = await reader.Polling();

            if (idm.Length > 0)
            {
                await LogMessageAsync(BitConverter.ToString(idm));

                // ReadWithoutEncryptionでサービスコードを指定してデータ取得
                // TODO: 取得結果のクラス化

                // 属性情報
                result = await reader.GetAttributeInfo(idm);
                await LogMessageAsync("属性情報: " + BitConverter.ToString(result));

                // 利用履歴
                result = await reader.GetUsageHistory(idm);
                await LogMessageAsync("利用履歴: " + BitConverter.ToString(result));

                // 改札入出場履歴
                result = await reader.GetTicketGateEnterLeaveHistory(idm);
                await LogMessageAsync("改札入出場履歴: " + BitConverter.ToString(result));

                // SF入場駅記録
                result = await reader.GetSFEnteredStationInfo(idm);
                await LogMessageAsync("SF入場駅記録: " + BitConverter.ToString(result));

                // 料金 発券/改札記録
                result = await reader.GetTicketIssueInspectRecord(idm);
                await LogMessageAsync("料金 発券/改札記録: " + BitConverter.ToString(result));
            }

            return;
        }

        private void CommandButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UpdateTargetCardFlag();
        }

        private void UpdateTargetCardFlag()
        {
            isEDY_Checked = IsEDYRadioButton.IsChecked.HasValue && IsEDYRadioButton.IsChecked.Value;
            isSuica_Checked = IsSuicaRadioButton.IsChecked.HasValue && IsSuicaRadioButton.IsChecked.Value;
        }

        private async Task LogMessageAsync(string message, bool clearPrevious = false)
        {
            Debug.WriteLine(message);
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (clearPrevious)
                {
                    LogText.Text = message;
                }
                else
                {
                    if (LogText.Text != "")
                    {
                        LogText.Text += "\r\n";
                    }
                    LogText.Text += message;
                }
                StatusBlockScroller.ChangeView(0, StatusBlockScroller.ExtentHeight, 1);
            });
        }

        public static async void LaunchNfcProximitySettingsPage()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-proximity:"));
        }
    }
}
