using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FlexLabs.Inky.Text;
using FlexLabs.Inky.Text.Fonts;

namespace FlexLabs.Inky.Preview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IInkyDriver _inky;

        public MainWindow()
        {
            InitializeComponent();
            var bitmap = new WriteableBitmap(212, 104, 96, 96, PixelFormats.Bgra32, null);
            _inky = new DrawingInkyDriver(212, 104, InkyDisplayColour.Yellow, bitmap);

            imgInky.Source = bitmap;

            _ = DrawHelloWorld();
        }

        public async Task DrawHelloWorld()
        {
            _inky.Clear();

            var p = _inky.DrawString(0, 0, InkyPixelColour.Black, Arial.Size8, "Hello");
            _inky.DrawString(0, p.y, InkyPixelColour.Black, Arial.Size8, "World");
            await _inky.RenderBuffer();
        }
    }
}
