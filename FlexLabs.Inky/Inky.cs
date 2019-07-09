using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace FlexLabs.Inky
{
    public class Inky
    {
        private const int Pin_Reset = 27;
        private const int Pin_Busy = 17;
        private const int Pin_DC = 22;
        private const int Pin_Mosi = 10;
        private const int Pin_Sclk = 11;
        private const int Pin_cs0 = 0;

        private const int SPI_Chunk_Size = 4096;
        private const string SPI_Command = "GPIO.LOW";
        private const string SPI_Data = "GPIO.HIGH";

        private static readonly Dictionary<(int, int), (int, int, int)> Resolutions =
            new Dictionary<(int, int), (int, int, int)>
            {
                [(400, 300)] = (400, 300, 0),
                [(212, 104)] = (104, 212, -90),
            };
        private static readonly Dictionary<InkyDisplayColour, byte[]> Luts =
            new Dictionary<InkyDisplayColour, byte[]>
            {
                [InkyDisplayColour.Black] = new byte[]
                {
                    0b01001000, 0b10100000, 0b00010000, 0b00010000, 0b00010011, 0b00000000, 0b00000000,
                    0b01001000, 0b10100000, 0b10000000, 0b00000000, 0b00000011, 0b00000000, 0b00000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0b01001000, 0b10100101, 0b00000000, 0b10111011, 0b00000000, 0b00000000, 0b00000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0x10, 0x04, 0x04, 0x04, 0x04,
                    0x10, 0x04, 0x04, 0x04, 0x04,
                    0x04, 0x08, 0x08, 0x10, 0x10,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                },
                [InkyDisplayColour.Red] = new byte[]
                {
                    0b01001000, 0b10100000, 0b00010000, 0b00010000, 0b00010011, 0b00000000, 0b00000000,
                    0b01001000, 0b10100000, 0b10000000, 0b00000000, 0b00000011, 0b00000000, 0b00000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0b01001000, 0b10100101, 0b00000000, 0b10111011, 0b00000000, 0b00000000, 0b00000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0x40, 0x0C, 0x20, 0x0C, 0x06,
                    0x10, 0x08, 0x04, 0x04, 0x06,
                    0x04, 0x08, 0x08, 0x10, 0x10,
                    0x02, 0x02, 0x02, 0x40, 0x20,
                    0x02, 0x02, 0x02, 0x02, 0x02,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                },
                [InkyDisplayColour.Red_Ht] = new byte[]
                {
                    0b01001000, 0b10100000, 0b00010000, 0b00010000, 0b00010011, 0b00010000, 0b00010000,
                    0b01001000, 0b10100000, 0b10000000, 0b00000000, 0b00000011, 0b10000000, 0b10000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0b01001000, 0b10100101, 0b00000000, 0b10111011, 0b00000000, 0b01001000, 0b00000000,
                    0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0x43, 0x0A, 0x1F, 0x0A, 0x04,
                    0x10, 0x08, 0x04, 0x04, 0x06,
                    0x04, 0x08, 0x08, 0x10, 0x0B,
                    0x01, 0x02, 0x01, 0x10, 0x30,
                    0x06, 0x06, 0x06, 0x02, 0x02,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                },
                [InkyDisplayColour.Yellow] = new byte[]
                {
                    0b11111010, 0b10010100, 0b10001100, 0b11000000, 0b11010000, 0b00000000, 0b00000000,
                    0b11111010, 0b10010100, 0b00101100, 0b10000000, 0b11100000, 0b00000000, 0b00000000,
                    0b11111010, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000,
                    0b11111010, 0b10010100, 0b11111000, 0b10000000, 0b01010000, 0b00000000, 0b11001100,
                    0b10111111, 0b01011000, 0b11111100, 0b10000000, 0b11010000, 0b00000000, 0b00010001,
                    0x40, 0x10, 0x40, 0x10, 0x08,
                    0x08, 0x10, 0x04, 0x04, 0x10,
                    0x08, 0x08, 0x03, 0x08, 0x20,
                    0x08, 0x04, 0x00, 0x00, 0x10,
                    0x10, 0x08, 0x08, 0x00, 0x20,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00,
                },
            };

        private bool _setup = false;
        private (int width, int height) _resolution;
        private int _columns, _rows, _rotation;
        private InkyDisplayColour _displayColour, _lut;
        private InkyPixelColour[,] _buffer;
        private InkyPixelColour _borderColour = InkyPixelColour.Black;


        public Inky((int width, int height) resolution, InkyDisplayColour colour)
        {
            _resolution = resolution;
            (_columns, _rows, _rotation) = Resolutions[resolution];
            _displayColour = _lut = colour;

            //self.eeprom = eeprom.read_eeprom()
            //if self.eeprom is not None:
            //    if self.eeprom.width != self.width or self.eeprom.height != self.height:
            //        raise ValueError('Supplied width/height do not match Inky: {}x{}'.format(self.eeprom.width, self.eeprom.height))
            //    if self.eeprom.display_variant in (1, 6) and self.eeprom.get_color() == 'red':
            //        self.lut = 'red_ht'

            _buffer = new InkyPixelColour[_resolution.height, _resolution.width];
        }

        public async Task Setup()
        {
            if (!_setup)
            {
                Pi.Gpio[Pin_DC].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[Pin_DC].Value = false;
                Pi.Gpio[Pin_DC].InputPullMode = GpioPinResistorPullMode.Off;

                Pi.Gpio[Pin_Reset].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[Pin_Reset].Value = true;
                Pi.Gpio[Pin_Reset].InputPullMode = GpioPinResistorPullMode.Off;

                Pi.Gpio[Pin_Reset].PinMode = GpioPinDriveMode.Input;
                Pi.Gpio[Pin_Reset].InputPullMode = GpioPinResistorPullMode.Off;

                Pi.Spi.Channel0Frequency = 488000;

                _setup = true;
            }

            Pi.Gpio[Pin_Reset].Value = false;
            await Task.Delay(TimeSpan.FromSeconds(.1));
            Pi.Gpio[Pin_Reset].Value = true;
            await Task.Delay(TimeSpan.FromSeconds(.1));

            SendCommand(0x12); // Soft Reset
            await BusyWait();
        }

        public async Task BusyWait()
        {
            while (Pi.Gpio[Pin_Busy].Value)
                await Task.Delay(TimeSpan.FromSeconds(.01));
        }

        public void SendCommand(byte command, params byte[] data)
        {
            Pi.Gpio[Pin_DC].Value = false;
            Pi.Spi.Channel0.Write(new[] { command });

            if (data?.Length > 0)
            {
                SendData(data);
            }
        }

        public void SendData(params byte[] data)
        {
            Pi.Gpio[Pin_DC].Value = true;
            Pi.Spi.Channel0.Write(data);
        }

        // Update display
        public async Task Update(byte[] blackPixels, byte[] colourPixels, bool busyWait = true)
        {
            await Setup();

            var packedHeight = new byte[] { (byte)(_rows % 256), (byte)(_rows / 256) };

            SendCommand(0x74, 0x54);  // Set Analog Block Control
            SendCommand(0x7e, 0x3b);  // Set Digital Block Control

            SendCommand(0x01, new byte[] { packedHeight[0], packedHeight[1], 0 }); // Gate setting              // packed_height + [0x00]

            SendCommand(0x03, 0x17);  // Gate Driving Voltage
            SendCommand(0x04, new byte[] { 0x41, 0xAC, 0x32 });  // Source Driving Voltage

            SendCommand(0x3a, 0x07);  // Dummy line period
            SendCommand(0x3b, 0x04);  // Gate line width
            SendCommand(0x11, 0x03);  // Data entry mode setting 0x03 = X/Y increment

            SendCommand(0x2c, 0x3c);  // VCOM Register, 0x3c = -1.5v?

            SendCommand(0x3c, 0b00000000);
            if (_borderColour == InkyPixelColour.Black)
                SendCommand(0x3c, 0b00000000);  // GS Transition Define A + VSS + LUT0
            else if (_borderColour == InkyPixelColour.Red && _displayColour == InkyDisplayColour.Red)
                SendCommand(0x3c, 0b01110011);  // Fix Level Define A + VSH2 + LUT3
            else if (_borderColour == InkyPixelColour.Yellow && _displayColour == InkyDisplayColour.Yellow)
                SendCommand(0x3c, 0b00110011);  // GS Transition Define A + VSH2 + LUT3
            else if (_borderColour == InkyPixelColour.White)
                SendCommand(0x3c, 0b00110001);  // GS Transition Define A + VSH2 + LUT1

            if (_displayColour == InkyDisplayColour.Yellow)
                SendCommand(0x04, new byte[] { 0x07, 0xAC, 0x32 });  // Set voltage of VSH and VSL
            if (_displayColour == InkyDisplayColour.Red && _resolution == (400, 300))
                SendCommand(0x04, new byte[] { 0x30, 0xAC, 0x22 });

            SendCommand(0x32, Luts[_lut]);  // Set LUTs

            SendCommand(0x44, new byte[] { 0x00, (byte)(_columns / 8 - 1) });  // Set RAM X Start/End           // (self.cols // 8) - 1
            SendCommand(0x45, new byte[] { 0, 0, packedHeight[0], packedHeight[1] });  // Set RAM Y Start/End   // [0x00, 0x00] + packed_height

            void writeBuffer(byte location, byte[] data)
            {
                SendCommand(0x4e, 0x00);  // Set RAM X Pointer Start
                SendCommand(0x4f, new byte[] { 0x00, 0x00 });  // Set RAM Y Pointer Start
                SendCommand(location, data);
            }
            // 0x24 == RAM B/W, 0x26 == RAM Red/Yellow/etc
            writeBuffer(0x24, blackPixels);
            writeBuffer(0x26, colourPixels);

            SendCommand(0x22, 0xC7);  // Display Update Sequence
            SendCommand(0x20);  // Trigger Display Update
            await Task.Delay(TimeSpan.FromSeconds(.05));

            if (busyWait)
            {
                await BusyWait();
                SendCommand(0x10, 0x01);  // Enter Deep Sleep
            }
        }

        public void SetPixel(int x, int y, InkyPixelColour colour)
        {
            _buffer[x, y] = colour;
        }

        T[,] Clone<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[i, j] = input[i, j];

            return result;
        }

        T[,] FlipLR<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[i, width - j - 1] = input[i, j];

            return result;
        }

        T[,] FlipUD<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[height - i - 1, j] = input[i, j];

            return result;
        }

        T[,] Flip90<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[width, height];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[j, i] = input[i, j];

            return result;
        }

        byte[] PackBits(InkyPixelColour[,] input, Func<InkyPixelColour, bool> match)
        {
            byte bit = 0, current = 0;
            var result = new List<byte>();
            for (int i = 0; i < input.GetLength(0); i++)
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    if (match(input[i, j]))
                        current += (byte)Math.Pow(2, 7 - bit);
                    if (++bit > 7)
                    {
                        result.Add(current);
                        bit = current = 0;
                    }
                }
            if (bit > 0)
                result.Add(current);
            return result.ToArray();
        }

        public Task Show(bool busyWait = true)
        {
            var region = Clone(_buffer);

            //if (vFlip)
            //    region = FlipLR(region);
            //if (hFlip)
            //    region = FlipUD(region);

            if (_rotation % 180 != 0)
                region = Flip90(region);

            var bufferA = PackBits(region, b => b != InkyPixelColour.Black);
            var bufferB = PackBits(region, b => b == InkyPixelColour.Red);

            return Update(bufferA, bufferB, busyWait);
        }
    }
}
