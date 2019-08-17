namespace FlexLabs.Inky
{
    /// <summary>
    /// Inky wHAT e-Ink Display Driver (400 x 300 pixels)
    /// </summary>
    public class InkyWhat : Inky
    {
        /// <summary>
        /// Initialise a new Inky wHAT driver
        /// </summary>
        /// <param name="colour">Display colour</param>
        public InkyWhat(InkyDisplayColour colour)
            : base(400, 300, colour)
        {
        }
    }
}
