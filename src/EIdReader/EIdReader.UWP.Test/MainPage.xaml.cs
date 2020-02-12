using PieEatingNinjas.EIdReader.Shared;
using PieEatingNinjas.EIdReader.Shared.Helper;
using PieEatingNinjas.EIdReader.UWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.UI.Xaml.Controls;

namespace EIdReader.UWP.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            DoRead();
        }

        private async Task DoRead()
        {
            string selector = SmartCardReader.GetDeviceSelector();

            DeviceInformationCollection smartCardReaders =
                await DeviceInformation.FindAllAsync(selector);

            foreach (var device in smartCardReaders)
            {
                SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);

                IReadOnlyList<SmartCard> cards =
                    await reader.FindAllCardsAsync();

                var card = cards.FirstOrDefault();

                if (card == null)
                    return;

                using (SmartCardConnection connection = await card.ConnectAsync())
                {
                    var data = await connection.ReadEIdData();

                    var date = EIdDateHelper.GetDateTime(data.identity[Tags.ID_BIRTH_DATE]);
                }
            }
        }
    }
}
