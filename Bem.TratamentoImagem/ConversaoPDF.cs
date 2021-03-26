using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
namespace Bem.TratamentoImagem
{
    public class ConversaoPDF
    {
        private PdfDocument _pdfDocument = null;
        private PdfDocument _newPdf = null;
        private readonly ImageCodecInfo _format = null;
        private readonly EncoderParameters _encoders = null;
        private readonly ConverterPdfPadraoA _ConverterPdfPadraoA;

        public string Keywords { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Cidade { get; set; }

        private ColorMatrix _grayScaleColorMatrix
        {
            get
            {
                return new ColorMatrix(
                   new float[][]
                   {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                   });
            }
        }

        public ConversaoPDF()
        {
            _newPdf = new PdfDocument();
            (_format, _encoders) = GetEncoderParameters();
            _ConverterPdfPadraoA = new ConverterPdfPadraoA(@"C:\LIXO\rauber\");
        }

        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public Bitmap Base64StringToBitmap(string base64String)
        {
            Bitmap bmpReturn = null;

            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer)
            {
                Position = 0
            };
            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);
            memoryStream.Close();
            return bmpReturn;
        }

        public Stream Base64ToStream(string base64)
        {
            return new MemoryStream(Convert.FromBase64String(base64));
        }

        public string ConverterImagemParaPdf(string base64)
        {
            PdfSection section = _newPdf.Sections.Add();
            PdfPageBase page = _newPdf.Pages.Add();

            //Load a tiff image from system
            PdfImage image = PdfImage.FromStream(Base64ToStream(base64));//FromFile(@"C:\LIXO\rauber\ccb.jpg");

            //Set image display location and size in PDF
            float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
            float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
            float fitRate = Math.Max(widthFitRate, heightFitRate);
            float fitWidth = image.PhysicalDimension.Width / fitRate;
            float fitHeight = image.PhysicalDimension.Height / fitRate;
            page.Canvas.DrawImage(image, 0, 0, fitWidth, fitHeight);

            //save and launch the file
            File.Delete(@"C:\LIXO\rauber\image to pdf.pdf");
            _newPdf.SaveToFile(@"C:\LIXO\rauber\image to pdf.pdf");
            _newPdf.Close();

            // retorna a versão PDF da imagem em base64
            return Convert.ToBase64String(File.ReadAllBytes(@"C:\LIXO\rauber\image to pdf.pdf"));
        }

        public ConversaoPDF(string fullPath) : this() =>
            LoadFromPath(fullPath);

        public ConversaoPDF(byte[] file) : this() =>
            LoadFromByteArray(file);

        public ConversaoPDF(ConverterPdfPadraoA pdfToPdfA2BConverter) : this() =>
            _ConverterPdfPadraoA = pdfToPdfA2BConverter;

        public void LoadFromPath(string fullPath) =>
            _pdfDocument = new PdfDocument($"{fullPath}");

        public void LoadFromByteArray(byte[] file) =>
            _pdfDocument = new PdfDocument(file);

        public void InsertImageInPdf(Bitmap image)
        {
            PdfPageBase page = _newPdf.Pages.Add(PdfPageSize.A4, new PdfMargins(0));

            float widthFitRate = image.PhysicalDimension.Width / (page.Canvas.ClientSize.Width);
            float heightFitRate = (image.PhysicalDimension.Height) / (page.Canvas.ClientSize.Height);
            float fitRate = Math.Max(widthFitRate, heightFitRate);
            float fitWidth = (image.PhysicalDimension.Width / fitRate);
            float fitHeight = (image.PhysicalDimension.Height / fitRate);
            SizeF size = new SizeF(fitWidth, fitHeight);
            page.Canvas.DrawImage(GetPdfImage(image), new PointF(0, 0), size);
        }

        public void InsertImageInPdf(IList<Bitmap> images)
        {
            foreach (var image in images)
                InsertImageInPdf(image);
        }

        public void ApplyGrayScaleInPdf()
        {
            for (int i = 0; i < _pdfDocument.Pages.Count; i++)
            {
                var image = ConvertToImage(i);
                ApplyGrayScaleInImage(image);
                InsertImageInPdf(image);
            }
        }

        public void ApplyGrayScaleInImage(Bitmap image)
        {
            var attr = new ImageAttributes();
            attr.SetColorMatrix(_grayScaleColorMatrix);
            SetScaleColor(image, attr);
        }

        public void ApplyGrayScaleInImage(IList<Bitmap> images)
        {
            foreach (var image in images)
                ApplyGrayScaleInImage(image);
        }

        public byte[] GetFileByteArray()
        {
            MemoryStream stream = SaveToStream();
            return stream.ToArray();
        }

        private Bitmap ConvertToImage(int pageNumber) =>
            _pdfDocument.SaveAsImage(pageNumber, 400, 400) as Bitmap;

        private void SetScaleColor(Bitmap image, ImageAttributes attributes)
        {
            var graphics = Graphics.FromImage(image);
            graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            graphics.Dispose();
        }

        private MemoryStream SaveToStream()
        {
            MemoryStream s = new MemoryStream();
            using (var stream = new MemoryStream())
            {
                var pdf = _newPdf.Pages.Count > 0 ? _newPdf : _pdfDocument;
                pdf.FileInfo.IncrementalUpdate = false;
                pdf.CompressionLevel = PdfCompressionLevel.Best;
                pdf.SaveToStream(stream, FileFormat.PDF);
                stream.WriteTo(s);

                return s;
            }
        }

        private PdfImage GetPdfImage(Bitmap image)
        {
            var imageStream = new MemoryStream();
            image.Save(imageStream, _format, _encoders);

            return PdfImage.FromStream(imageStream);
        }

        private (ImageCodecInfo, EncoderParameters) GetEncoderParameters()
        {
            EncoderParameters encoderParameters = new EncoderParameters(1);
            EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, 10L);
            encoderParameters.Param[0] = encoderParameter;

            return (GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format) =>
            ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID.Equals(format.Guid));

        public byte[] GetPdfA2b() =>
            _ConverterPdfPadraoA.ConvertPdfToPdfA2b(GetFileByteArray(), Title, Author, Cidade);
    }
}
