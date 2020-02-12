using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UWPSample
{
    public sealed partial class MainPage : Page
    {
        protected readonly byte[] SELECT_ADDRESS_APDU_COMMAND = new byte[] {
                            0x00, //CLA   
                            0xA4, //INS  
                            0x08, //P1  => absolute path
                            0x0C, //P2 
                            0x06, //Length of the path
                            //ADDRESS FILE PATH
                            0x3F,//directory MF "3f00"  
                            0x00,
                            0xDF,//subdirectory identity DF(ID) "DF01"  
                            0x01,
                            0x40,//the address file EF(ID#Address) "4033"  
                            0x33
        };
        
        protected readonly byte[] SELECT_ID_APDU_COMMAND = new byte[] {
                            0x00, //CLA   
                            0xA4, //INS  
                            0x08, //P1  => absolute path
                            0x0C, //P2 
                            0x06, //Length of the path
                            //ID FILE PATH
                            0x3F,//directory MF "3f00"  
                            0x00,
                            0xDF,//subdirectory identity DF(ID) "DF01"  
                            0x01,
                            0x40,//identity file EF(ID#RN) "4031"  
                            0x31
        };


        protected readonly byte[] READ_FILE_COMMAND = new byte[] {
                            0x00, //CLA   
                            0xB0, //Read binary command  
                            0x00, //OFF_H higher byte of the offset (bit 8 = 0)  
                            0x00,
                            0 //le
        };

        private byte[] GetReadCommand(int length = 0)
        {
            return new byte[] {
                            0x00, //CLA   
                            0xB0, //Read binary command  
                            0x00, //OFF_H higher byte of the offset (bit 8 = 0)  
                            0x00,
                            (byte)length //le
            };
        }

        public MainPage()
        {
            this.InitializeComponent();
            GetSmartCardReaders();
        }

        private async void GetSmartCardReaders()
        {
            string selector = SmartCardReader.GetDeviceSelector();

            DeviceInformationCollection smartCardReaders =
                await DeviceInformation.FindAllAsync(selector);

            var conn = await GetConnection(smartCardReaders.FirstOrDefault());

            var result = await conn.TransmitAsync(SELECT_ADDRESS_APDU_COMMAND.AsBuffer());
            CryptographicBuffer.CopyToByteArray(result, out byte[] response);

            if (response?.Length == 2 &&
                 (ushort)((response[0] << 8) + response[1]) == 0x9000)
            {
                //We're good!
            }
            else
            {
                //We didn't get a OK status when selecting the file
                //We are unable to read the card
                throw new ReadCardException();
            }

            byte[] fileContents = null;

            result = await conn.TransmitAsync(GetReadCommand().AsBuffer());
            CryptographicBuffer.CopyToByteArray(result, out byte[] readResponse);

            if (readResponse?.Length == 2 &&
                 readResponse[0] == 0x6C)
            {
                //Getting back 0x6C, Length is in second byte
                result = await conn.TransmitAsync(GetReadCommand(readResponse[1]).AsBuffer());
                CryptographicBuffer.CopyToByteArray(result, out fileContents);
            }
            else
            {
                //Don't think this should occur...
                CryptographicBuffer.CopyToByteArray(result, out fileContents);
            }
            //Raw contents of file is in fileContents array


            var data = ReadData(fileContents, new List<byte>()
            {
                ADDRESS_STREET_NUMBER_TAG,
                ADDRESS_ZIP_CODE_TAG,
                ADDRESS_MUNICIPALITY_TAG
            });

            var street = data[ADDRESS_STREET_NUMBER_TAG];
            var zip = data[ADDRESS_ZIP_CODE_TAG];
            var muni = data[ADDRESS_MUNICIPALITY_TAG];

        }

        const byte ADDRESS_STREET_NUMBER_TAG = 0x01;
        const byte ADDRESS_ZIP_CODE_TAG = 0x02;
        const byte ADDRESS_MUNICIPALITY_TAG = 0x03;



        private Dictionary<byte, string> ReadData(byte[] rawData, List<byte> tags)
        {
            var data = tags.ToDictionary(k => k, v => "");

            int idx = 0; //we start at 0 :-)
            while (idx < rawData.Length)
            {
                byte tag = rawData[idx]; //at this location we have a Tag
                idx++;
                var length = rawData[idx]; //the next position holds the lenght of the data
                idx++;  //start of the data

                if (tags.Contains(tag)) //this is a tag we are interested in
                {
                    var res = new byte[length]; //create array to put data of this tag in. We know the length
                    Array.Copy(rawData, idx, res, 0, length); //fill

                    var value = Encoding.UTF8.GetString(res); //convert to string

                    data[tag] = value; //put the string value we read in the data dictionary
                }
                idx += length; //moving on, skipping the length of data we just read
            }
            return data;
        }

        private async Task<SmartCardConnection> GetConnection(DeviceInformation device)
        {
            SmartCardReader reader =
                   await SmartCardReader.FromIdAsync(device.Id);

            IReadOnlyList<SmartCard> cards =
                await reader.FindAllCardsAsync();

            var card = cards.FirstOrDefault();

            if (card == null)
                throw new NoCardFoundException();

            return await card.ConnectAsync();
        }
    }

    public class NoCardFoundException : Exception
    {
        public NoCardFoundException() : base("No eID card found!")
        { }
    }

    public class ReadCardException : Exception
    {
        public ReadCardException() : base("Unable to read the inserted card!")
        { }
    }
}
