namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public class RectangleArea
    {
        public RectangleArea(int x, int y, int width, int height, double area, int pixel, double ar)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Area = area;
            Pixel = pixel;
            Ar = ar;
        }

        // Auto-Initialized properties
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Area { get; set; }
        public double Pixel { get; set; }
        public double Ar { get; set; }
    }
}
