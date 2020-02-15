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

            var device = smartCardReaders.FirstOrDefault();

            SmartCardReader reader = await SmartCardReader.FromIdAsync(device.Id);

            IReadOnlyList<SmartCard> cards =
                await reader.FindAllCardsAsync();

            var card = cards.FirstOrDefault();

            if (card == null)
                return;

            (Dictionary<byte, string> address, Dictionary< byte, string> identity)? data = null;

            using (SmartCardConnection connection = await card.ConnectAsync())
            {
                data = await connection.ReadEIdData();
            }
            var identityData = data.Value.identity;
            var addressData = data.Value.address;

            var dateOfBirth = EIdDateHelper.GetDateTime(identityData[Tags.ID_BIRTH_DATE]);

            FullName.Text = $"{identityData[Tags.ID_FIRST_NAME]} {identityData[Tags.ID_LAST_NAME]}";
            PlaceOfBirth.Text = identityData[Tags.ID_BIRTH_LOCATION];
            DateOfBirth.Text = dateOfBirth.Value.ToString("dd/MM/yyyy");
            Gender.Text = identityData[Tags.ID_SEX];
            Nationality.Text = identityData[Tags.ID_NATIONALITY];
            NationalNumber.Text = identityData[Tags.ID_NATIONAL_NUMBER];
            Address.Text = $"{addressData[Tags.ADDRESS_STREET_NUMBER]} {addressData[Tags.ADDRESS_ZIP_CODE]} {addressData[Tags.ADDRESS_MUNICIPALITY]}";
        }
    }
}
