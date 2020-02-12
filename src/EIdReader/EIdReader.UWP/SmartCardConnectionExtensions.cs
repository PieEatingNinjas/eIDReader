using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;

namespace PieEatingNinjas.EIdReader.UWP
{
    public static class SmartCardConnectionExtensions
    {
        public static Task<Dictionary<byte, string>> ReadAddress(this SmartCardConnection connection)
        {
            using (var reader = new EIdReader(connection))
            {
                return ReadEIdAddress(reader);
            }
        }

        public static Task<Dictionary<byte, string>> ReadIdentity(this SmartCardConnection connection)
        {
            using (var reader = new EIdReader(connection))
            {
                return ReadEIdIdentity(reader);
            }
        }

        public static async Task<(Dictionary<byte, string> address, Dictionary<byte, string>identity)> ReadEIdData(this SmartCardConnection connection)
        {
            using (var reader = new EIdReader(connection))
            {
                var address = await ReadEIdAddress(reader);
                var identity = await ReadEIdIdentity(reader);
                return (address, identity);
            }
        }

        private static Task<Dictionary<byte, string>> ReadEIdAddress(EIdReader reader)
            => reader.ReadAddress();

        private static Task<Dictionary<byte, string>> ReadEIdIdentity(EIdReader reader)
            => reader.ReadIdentity();
    }
}
