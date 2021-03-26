using System;
using System.IO;

namespace Bem.TratamentoImagem
{
    public class ConverterPdf
    {
        private ConversaoPDF _pdfConverter;

        public void ExecutarConversao()
        {
            string conteudoBase64 = LerConteudoArquivo(@"C:\LIXO\stringBase64_PDF.txt");
            converterPdfParaPdfA(conteudoBase64);
        }

        public string LerConteudoArquivo(string caminhoLeitura)
        {
            using StreamReader sr = new StreamReader(caminhoLeitura);
            // Read the stream to a string, and write the string to the console.
            string line = sr.ReadToEnd();
            return line;
        }

        /// <summary>
        /// retorna o pdf/a em uma string base64
        /// </summary>                
        public string converterPdfParaPdfA(string conteudoBase64)
        {
            _pdfConverter = new ConversaoPDF();
            _pdfConverter.LoadFromByteArray(Convert.FromBase64String(conteudoBase64));
            _pdfConverter.Author = "RAUBER_AUTOR";
            _pdfConverter.Title = "RAUBER_TITULO";
            _pdfConverter.Cidade = "RAUBER_CIDADE";
            _pdfConverter.ApplyGrayScaleInPdf();

            return Convert.ToBase64String(_pdfConverter.GetPdfA2b());
        }

        public void converterImagemParaPdfA()
        {
            string conteudoBase64 = LerConteudoArquivo(@"C:\LIXO\stringBase64_IMAGEM.txt");
            _pdfConverter = new ConversaoPDF();
            var pdfDaImagem = _pdfConverter.ConverterImagemParaPdf(conteudoBase64);

            converterPdfParaPdfA(pdfDaImagem);
        }
    }
}
