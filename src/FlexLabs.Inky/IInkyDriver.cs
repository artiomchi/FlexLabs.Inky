using System.Threading.Tasks;

namespace FlexLabs.Inky
{
    /// <summary>
    /// Inky e-Ink Display Driver
    /// </summary>
    public interface IInkyDriver
    {
        /// <summary>
        /// Sets the border colour when rendering image (defaults to black)
        /// </summary>
        InkyPixelColour BorderColour { get; set; }

        /// <summary>
        /// Returns the display colour
        /// </summary>
        InkyDisplayColour DisplayColour { get; }

        /// <summary>
        /// Sets whether the image should be flipped horizontally during render
        /// </summary>
        bool FlipHorizontally { get; set; }

        /// <summary>
        /// Sets whether the image should be flipped vertically during render
        /// </summary>
        bool FlipVertically { get; set; }

        /// <summary>
        /// Returns the display height
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Returns the display width
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Clear the display buffer to the specified colour
        /// </summary>
        /// <param name="colour">The colour to fill the display with</param>
        void Clear(InkyPixelColour colour = InkyPixelColour.White);

        /// <summary>
        /// Init the GPIO pins and SPI interface
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Set the colour of a pixel in the display buffer
        /// </summary>
        /// <param name="x">The x coordinate of the pixel</param>
        /// <param name="y">The y coordinate of the pixel</param>
        /// <param name="colour">The colour to set the pixel to</param>
        void SetPixel(int x, int y, InkyPixelColour colour);

        /// <summary>
        /// Render the buffer on display
        /// </summary>
        /// <returns></returns>
        Task RenderBuffer();
    }
}
