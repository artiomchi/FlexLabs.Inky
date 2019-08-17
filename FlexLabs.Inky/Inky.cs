using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace FlexLabs.Inky
{
    /// <summary>
    /// Inky e-Ink Display Driver
    /// </summary>
    public class Inky
    {
        private const int Pin_Reset = 27;
        private const int Pin_Busy = 17;
        private const int Pin_DC = 22;

        private static readonly Dictionary<(short, short), (short, short, short)> Resolutions =
            new Dictionary<(short, short), (short, short, short)>
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

        private readonly (short width, short height) _resolution;
        private readonly short _columns, _rows, _rotation;
        private readonly InkyDisplayColour _displayColour, _lut;
        private readonly InkyPixelColour[,] _buffer;

        private bool _setup = false;

        /// <summary>
        /// Initialise a new Inky driver
        /// </summary>
        /// <param name="width">Display width in pixels</param>
        /// <param name="height">Display height in pixels</param>
        /// <param name="colour">Display colour</param>
        public Inky(short width, short height, InkyDisplayColour colour)
        {
            _resolution = (width, height);
            (_columns, _rows, _rotation) = Resolutions[_resolution];
            _displayColour = _lut = colour;

            //self.eeprom = eeprom.read_eeprom()
            //if self.eeprom is not None:
            //    if self.eeprom.width != self.width or self.eeprom.height != self.height:
            //        raise ValueError('Supplied width/height do not match Inky: {}x{}'.format(self.eeprom.width, self.eeprom.height))
            //    if self.eeprom.display_variant in (1, 6) and self.eeprom.get_color() == 'red':
            //        self.lut = 'red_ht'

            _buffer = new InkyPixelColour[_resolution.height, _resolution.width];
        }

        /// <summary>
        /// Sets whether the image should be flipped vertically during render
        /// </summary>
        public bool FlipVertically { get; set; }

        /// <summary>
        /// Sets whether the image should be flipped horizontally during render
        /// </summary>
        public bool FlipHorizontally { get; set; }

        /// <summary>
        /// Sets the border colour when rendering image (defaults to black)
        /// </summary>
        public InkyPixelColour BorderColour { get; set; } = InkyPixelColour.Black;

        /// <summary>
        /// Returns the display width
        /// </summary>
        public short Width => _resolution.width;

        /// <summary>
        /// Returns the display height
        /// </summary>
        public short Height => _resolution.height;

        /// <summary>
        /// Returns the display colour
        /// </summary>
        public InkyDisplayColour DisplayColour => _displayColour;

        #region Internal display rendering functions

        private void SendCommand(byte command, params byte[] data)
        {
            Pi.Gpio[Pin_DC].Value = false;
            Pi.Spi.Channel0.Write(new[] { command });

            if (data?.Length > 0)
            {
                SendData(data);
            }
        }

        private void SendData(params byte[] data)
        {
            Pi.Gpio[Pin_DC].Value = true;
            Pi.Spi.Channel0.Write(data);
        }

        private async Task Update(byte[] blackPixels, byte[] colourPixels, bool busyWait = true)
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
            if (BorderColour == InkyPixelColour.Black)
                SendCommand(0x3c, 0b00000000);  // GS Transition Define A + VSS + LUT0
            else if (BorderColour == InkyPixelColour.Red && _displayColour == InkyDisplayColour.Red)
                SendCommand(0x3c, 0b01110011);  // Fix Level Define A + VSH2 + LUT3
            else if (BorderColour == InkyPixelColour.Yellow && _displayColour == InkyDisplayColour.Yellow)
                SendCommand(0x3c, 0b00110011);  // GS Transition Define A + VSH2 + LUT3
            else if (BorderColour == InkyPixelColour.White)
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

        #endregion

        #region Buffer modification methods

        private T[,] Clone<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[i, j] = input[i, j];

            return result;
        }

        private T[,] FlipLR<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[i, width - j - 1] = input[i, j];

            return result;
        }

        private T[,] FlipUD<T>(T[,] input)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var result = new T[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[height - i - 1, j] = input[i, j];

            return result;
        }

        private T[,] Rotate<T>(T[,] input, short angle)
        {
            var rotations = angle / 90;
            while (rotations > 3)
                rotations -= 4;
            while (rotations < 0)
                rotations += 4;
            if (rotations == 0)
                return input;

            var height = input.GetLength(0);
            var width = input.GetLength(1);

            var result = rotations % 2 == 0
                ? new T[height, width]
                : new T[width, height];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    switch (rotations)
                    {
                        case 1:
                            result[j, height - i - 1] = input[i, j];
                            break;
                        case 2:
                            result[height - i - 1, width - j - 1] = input[i, j];
                            break;
                        case 3:
                            result[width - j - 1, i] = input[i, j];
                            break;
                    }
                }

            return result;
        }

        private byte[] PackBits(InkyPixelColour[,] input, Func<InkyPixelColour, bool> match)
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

        #endregion

        /// <summary>
        /// Setup the GPIO pins and SPI interface
        /// </summary>
        /// <returns></returns>
        public async Task Setup()
        {
            if (!_setup)
            {
                Pi.Init<BootstrapWiringPi>();

                Pi.Gpio[Pin_DC].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[Pin_DC].Value = false;

                Pi.Gpio[Pin_Reset].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[Pin_Reset].Value = true;

                Pi.Gpio[Pin_Busy].PinMode = GpioPinDriveMode.Input;
                Pi.Gpio[Pin_Busy].InputPullMode = GpioPinResistorPullMode.Off;

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

        /// <summary>
        /// Wait until the screen finished rendering
        /// </summary>
        /// <returns></returns>
        public async Task BusyWait()
        {
            while (Pi.Gpio[Pin_Busy].Value)
                await Task.Delay(TimeSpan.FromSeconds(.01));
        }

        /// <summary>
        /// Clear the display buffer to the specified colour
        /// </summary>
        /// <param name="colour">The colour to fill the display with</param>
        public void Clear(InkyPixelColour colour = InkyPixelColour.White)
        {
            for (int i = 0; i < _buffer.GetLength(0); i++)
                for (int j = 0; j < _buffer.GetLength(1); j++)
                    _buffer[i, j] = colour;
        }

        /// <summary>
        /// Set the colour of a pixel in the display buffer
        /// </summary>
        /// <param name="x">The x coordinate of the pixel</param>
        /// <param name="y">The y coordinate of the pixel</param>
        /// <param name="colour">The colour to set the pixel to</param>
        public void SetPixel(int x, int y, InkyPixelColour colour)
        {
            _buffer[y, x] = colour;
        }

        /// <summary>
        /// Show the buffer on display
        /// </summary>
        /// <param name="busyWait">If True, wait for display update to finish before returning.</param>
        /// <returns></returns>
        public Task Show(bool busyWait = true)
        {
            var region = Clone(_buffer);

            if (FlipVertically)
                region = FlipLR(region);
            if (FlipHorizontally)
                region = FlipUD(region);
            if (_rotation != 0)
                region = Rotate(region, _rotation);

            var bufferA = PackBits(region, b => b != InkyPixelColour.Black);
            var bufferB = PackBits(region, b => b == InkyPixelColour.Red);

            return Update(bufferA, bufferB, busyWait);
        }
    }
}
