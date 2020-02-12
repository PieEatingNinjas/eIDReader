using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PieEatingNinjas.EIdReader.UWP")]
namespace PieEatingNinjas.EIdReader.Shared
{
    internal static class Command
    {
        //see Reference Manual BelPic (V1.7) page 21
        //6.3 SELECT FILE Command (ISO 7816-4)
        internal static readonly byte[] SELECT_FILE_APDU_COMMAND = new byte[] {
                            0x00, //CLA   
                            0xA4, //INS  
                            0x08, //P1  
                            0x0C, //P2 
        };

        //see belgian_electronic_identity_card_content_v2.8.a page 10 + 13
        //4.1 File structure
        internal static readonly byte[] IDENTITY_FILE_LOCATION = new byte[] {
                            0x3F,// MASTER FILE, Head directory MF "3f00"  
                            0x00,
                            0xDF,// Dedicated File, subdirectory identity DF(ID) "DF01"  
                            0x01,
                            0x40,// Elementary File, the identity file itself EF(ID#RN) "4031"  
                            0x31
        };

        //see belgian_electronic_identity_card_content_v2.8.a page 10 + 13
        //4.1 File structure
        internal static readonly byte[] ADDRESS_FILE_LOCATION = new byte[] {
                            0x3F,// MASTER FILE, Head directory MF "3f00"  
                            0x00,
                            0xDF,// Dedicated File, subdirectory identity DF(ID) "DF01"  
                            0x01,
                            0x40,// Elementary File, the address file EF(ID#Address) "4033"  
                            0x33
        };

        //see Reference Manual BelPic (V1.7) page 23
        //6.4 READ BINARY Command (ISO 7816-4)
        internal static readonly byte[] READ_FILE_COMMAND = new byte[] {
                            0x00, //CLA   
                            0xB0, //Read binary command  
                            0x00, //OFF_H higher byte of the offset (bit 8 = 0)  
                            0x00,
                            0 //le
        };

        //Construct the APDU command to select a file
        //see Reference Manual BelPic (V1.7) page 21
        //6.3 SELECT FILE Command (ISO 7816-4)
        //format is the fixed select command followed by the 'length' of the fileLocation
        //followed by the fileLocation itself
        internal static byte[] GetSelectFileCommand(byte[] fileLocation)
        {
            var selectCommandLength = SELECT_FILE_APDU_COMMAND.Length;
            var fileLocationLength = fileLocation.Length;

            //total size should be the length of the fixed select structure + the length of the 
            //fileLocation + 1 additional byte to indicate the length of the file location
            var command = new byte[selectCommandLength + 1 + fileLocationLength];

            //fixed select structure
            Array.Copy(SELECT_FILE_APDU_COMMAND, command, selectCommandLength);
            //byte indicating the length of the fileLocation
            command[selectCommandLength] = (byte)fileLocationLength;
            //the file location
            Array.Copy(fileLocation, 0, command, selectCommandLength + 1, fileLocationLength);

            return command;
        }

        //Construct the APDU command to read a file
        //see Reference Manual BelPic (V1.7) page 23
        //6.4 READ BINARY Command (ISO 7816-4)
        //format is the fixed read command followed by the 'length' of the data to read
        //ofcourse, we can't (always) predict the length of the data to read, we can construct 
        //this read command will with a lenght of 0
        //see HandleReadFileResponse how the story continues...
        internal static byte[] GetReadFileCommand(byte length)
        {
            var readFileCommandLength = READ_FILE_COMMAND.Length;

            var command = new byte[readFileCommandLength];

            Array.Copy(READ_FILE_COMMAND, command, readFileCommandLength);
            command[readFileCommandLength - 1] = length;

            return command;
        }
    }
}
