using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace WpfApp1
{
    public class WindowsOcr
    {
        /// <summary>
        /// 指定した画像内に含まれる文字情報を取得します
        /// </summary>
        /// <param name="filename">画像ファイル名</param>
        /// <returns>OCR結果</returns>
        public string Recognize(MemoryStream memoryStream)
        {
            Task<OcrResult> result = OcrMain(memoryStream);
            result.Wait();
            return result.Result.Text;
        }

        private async Task<OcrResult> OcrMain(MemoryStream memoryStream)
        {
            OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US"));
            var bitmap = await LoadImage(memoryStream);
            var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
            return ocrResult;
        }
        private async Task<SoftwareBitmap> LoadImage(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;

            var stream = await ConvertToRandomAccessStream(memoryStream);
            var bitmap = await LoadImage(stream);
            return bitmap;
        }
        private async Task<SoftwareBitmap> LoadImage(IRandomAccessStream stream)
        {
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return bitmap;
        }
        private async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(memoryStream.ToArray()));
            task.Start();
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }
    }
}
