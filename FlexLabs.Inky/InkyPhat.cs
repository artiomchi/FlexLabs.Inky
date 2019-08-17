namespace FlexLabs.Inky
{
    /// <summary>
    /// Inky pHAT e-Ink Display Driver (212 x 104 pixels)
    /// </summary>
    public class InkyPhat : Inky
    {
        /// <summary>
        /// Initialise a new Inky pHAT driver
        /// </summary>
        /// <param name="colour">Display colour</param>
        public InkyPhat(InkyDisplayColour colour)
            : base(212, 104, colour)
        {
        }
    }
}
