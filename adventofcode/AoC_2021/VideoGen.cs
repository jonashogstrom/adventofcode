using System;
using System.Drawing;
using System.Drawing.Imaging;
using Accord.Video.FFMPEG;

namespace AdventofCode.AoC_2021
{
    internal class VideoGen<T>
    {
        private readonly string _fileName;
        private readonly Func<T, Color> _valueToColor;
        private readonly int _width;
        private readonly int _height;
        private readonly VideoFileWriter _gen;
        private readonly int _scale;

        public VideoGen(string fileName, int width, int height, Func<T, Color> valueToColor)
        {
            _fileName = fileName;
            _valueToColor = valueToColor;
            _scale = 100 / width;
            _width = width * _scale;
            _height = height * _scale;
            _gen = new VideoFileWriter();
            _gen.Open(fileName, _width, _height, 10, VideoCodec.Default);
        }

        public void AddSparseMap(SparseBuffer<T> map)
        {
            var bitmap = MapToBitmap(map);
            _gen.WriteVideoFrame(bitmap);

        }

        private Bitmap MapToBitmap(SparseBuffer<T> map)
        {
            var image = new Bitmap(_width, _height, PixelFormat.Format24bppRgb);
            foreach (var c in map.Keys)
            {
                var value = map[c];
                var col = _valueToColor(value);

                for (int x = 0; x < _scale; x++)
                    for (int y = 0; y < _scale; y++)
                        image.SetPixel(c.Col * _scale + x, c.Row * _scale + y, col);

            }
            return image;
        }

        public void Flush()
        {
            _gen.Flush();
            _gen.Close();
        }

    }
}