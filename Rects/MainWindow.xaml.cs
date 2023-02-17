using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rects
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private Rectangle _currentRectangle;
        private List<Rectangle> _rectangles = new List<Rectangle>();

        public MainWindow()
        {
            InitializeComponent();

            // TODO the mouseddown could also be changed to window mousedown
            canvas.MouseDown += Canvas_MouseDown;
            this.MouseUp += Window_MouseUp;
            this.MouseMove += Window_MouseMove;
        }


        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentRectangle != null)
            {
                // Get the position of the mouse relative to the canvas
                Point endPoint = e.GetPosition(canvas);

                // Restrict the rectangle to the bounds of the image
                if (endPoint.X > canvas.ActualWidth)
                {
                    endPoint.X = canvas.ActualWidth;
                }
                else if (endPoint.X < 0)
                {
                    endPoint.X = 0;
                }

                if (endPoint.Y > canvas.ActualHeight)
                {
                    endPoint.Y = canvas.ActualHeight;
                }
                else if (endPoint.Y < 0)
                {
                    endPoint.Y = 0;
                }

                // Calculate the size and position of the rectangle
                double width = Math.Abs(endPoint.X - _startPoint.X);
                double height = Math.Abs(endPoint.Y - _startPoint.Y);
                double x = Math.Min(_startPoint.X, endPoint.X);
                double y = Math.Min(_startPoint.Y, endPoint.Y);

                // Set the position and size of the rectangle
                _currentRectangle.SetValue(Canvas.LeftProperty, x);
                _currentRectangle.SetValue(Canvas.TopProperty, y);
                _currentRectangle.Width = width;
                _currentRectangle.Height = height;

                // Add the rectangle to the canvas
                _rectangles.Add(_currentRectangle);

                // Reset the rectangle and starting point
                _currentRectangle = null;
                _startPoint = new Point();
            }
        }


        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(canvas);
            _currentRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            canvas.Children.Add(_currentRectangle);
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentRectangle == null)
            {
                return;
            }

            Point endPoint = e.GetPosition(canvas);

            double x = Math.Min(_startPoint.X, endPoint.X);
            double y = Math.Min(_startPoint.Y, endPoint.Y);
            double width = Math.Abs(_startPoint.X - endPoint.X);
            double height = Math.Abs(_startPoint.Y - endPoint.Y);

            // Limit the rectangle to the bounds of the canvas
            if (x < 0)
            {
                width += x;
                x = 0;
            }
            if (y < 0)
            {
                height += y;
                y = 0;
            }
            if (x + width > canvas.ActualWidth)
            {
                width = canvas.ActualWidth - x;
            }
            if (y + height > canvas.ActualHeight)
            {
                height = canvas.ActualHeight - y;
            }

            Canvas.SetLeft(_currentRectangle, x);
            Canvas.SetTop(_currentRectangle, y);
            _currentRectangle.Width = width;
            _currentRectangle.Height = height;
        }



        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            // Create a new OpenFileDialog instance
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif) | *.jpg; *.jpeg; *.png; *.gif";

            // Show the dialog and get the result
            bool? result = dialog.ShowDialog();

            // If the user selected a file, set it as the canvas background and resize the canvas
            if (result == true)
            {
                string filename = dialog.FileName;

                // Load the image and set it as the canvas background
                BitmapImage image = new BitmapImage(new Uri(filename, UriKind.Absolute));
                ImageBrush brush = new ImageBrush(image);
                canvas.Background = brush;

                // Calculate the aspect ratio of the image
                double aspectRatio = (double)image.PixelWidth / (double)image.PixelHeight;

                // Set the size of the canvas to match the aspect ratio of the image
                if (aspectRatio > 1)
                {
                    canvas.Width = canvas.ActualHeight * aspectRatio;
                    canvas.Height = canvas.ActualHeight;
                }
                else
                {
                    canvas.Width = canvas.ActualWidth;
                    canvas.Height = canvas.ActualWidth / aspectRatio;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Create a new SaveFileDialog instance
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png|GIF (*.gif)|*.gif";

            // Show the dialog and get the result
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Force the canvas to render itself before creating the RenderTargetBitmap
                canvas.Measure(new Size(canvas.ActualWidth, canvas.ActualHeight));
                canvas.Arrange(new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight));
                canvas.UpdateLayout();

                // Create a new RenderTargetBitmap object and render the canvas to it
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
                renderBitmap.Render(canvas);

                // Create a new BitmapEncoder based on the selected file format
                BitmapEncoder encoder;
                switch (dialog.FilterIndex)
                {
                    case 1:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case 2:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 3:
                        encoder = new GifBitmapEncoder();
                        break;
                    default:
                        encoder = new JpegBitmapEncoder();
                        break;
                }

                // Add the RenderTargetBitmap to the BitmapEncoder
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                // Save the file using the selected filename
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                // Erase the canvas
                canvas.Children.Clear();
                // canvas.Background = default;
            }
        }


        // 
    }
}
