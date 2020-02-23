namespace FlexLabs.Inky
{
    /// <summary>
    /// Inky wHAT e-Ink Display Driver (400 x 300 pixels)
    /// </summary>
    public class InkyWHat : InkyDriver
    {
        /// <summary>
        /// Initialise a new Inky wHAT driver
        /// </summary>
        /// <param name="colour">Display colour</param>
        public InkyWHat(InkyDisplayColour colour)
            : base(400, 300, colour)
        {
        }
    }
}
