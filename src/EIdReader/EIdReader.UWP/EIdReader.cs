using PieEatingNinjas.EIdReader.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Security.Cryptography;

namespace PieEatingNinjas.EIdReader.UWP
{
    internal class EIdReader : IDisposable
    {
        private bool disposed = false;
        readonly SmartCardConnection connection;

        internal EIdReader(SmartCardConnection connection)
        {
            this.connection = connection;
        }

        internal Task<Dictionary<byte, string>> ReadAddress()
            => ReadFile(Command.ADDRESS_FILE_LOCATION, Tags.ADDRESS_TAGS);

        internal Task<Dictionary<byte, string>> ReadIdentity()
            => ReadFile(Command.IDENTITY_FILE_LOCATION, Tags.IDENTITY_TAGS);

        private async Task<Dictionary<byte, string>> ReadFile(byte[] fileLocation, IEnumerable<byte> tags)
        {
            await SelectFile(fileLocation);
            var data = await ReadFile(0);
            return ReadData(data, tags);
        }

        private Dictionary<byte, string> ReadData(byte[] rawData, IEnumerable<byte> tags)
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

        private async Task<byte[]> ReadFile(byte lenght)
        {
            var result = await connection.TransmitAsync(Command.GetReadFileCommand(lenght).AsBuffer());
            CryptographicBuffer.CopyToByteArray(result, out byte[] readResponse);

            if (readResponse?.Length == 2 &&
                 readResponse[0] == StatusCode.READ_FILE_LE_INCORRECT)
            {
                //Getting back 0x6C, Length is in second byte
                return await ReadFile(readResponse[1]);
            }
            else
            {
                return readResponse;
            }
        }


        private async Task SelectFile(byte[] location)
        {
            var command = Command.GetSelectFileCommand(location);
            var result = await connection.TransmitAsync(command.AsBuffer());
            CryptographicBuffer.CopyToByteArray(result, out byte[] response);

            if (response?.Length == 2 && (ushort)((response[0] << 8) + response[1]) == StatusCode.SELECT_FILE_OK)
            {
                //We're good!
            }
            else
            {
                //We didn't get a OK status when selecting the file
                //We are unable to read the card
                throw new ReadCardException();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
                connection.Dispose();

            disposed = true;
        }
    }
}
