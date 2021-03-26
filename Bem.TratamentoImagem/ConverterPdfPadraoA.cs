using Ghostscript.NET.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bem.TratamentoImagem
{
    public class ConverterPdfPadraoA
    {
        private readonly string _diretorioTemporario;
        public ConverterPdfPadraoA(string diretorioTemporario)
        {
            _diretorioTemporario = diretorioTemporario;
        }

        public byte[] ConvertPdfToPdfA2b(byte[] pdf, string titulo, string codigoInstituicaoFinanceira, string cidade)
        {
            string inputFile = Path.Combine(_diretorioTemporario, $"{titulo}.pdf");
            string outputFile = Path.Combine(_diretorioTemporario, $"{titulo}-NEW.pdf");
            
            //para teste, so deleta antes de começar 
            File.Delete(inputFile);
            File.Delete(outputFile);

            File.WriteAllBytes(inputFile, pdf);

            using GhostscriptProcessor ghostscript = new GhostscriptProcessor();
            var fileMetadata = GetMetadatasFile(titulo, codigoInstituicaoFinanceira, cidade);
            var t = new[] {
                    "gs",
                    "-dPDFA=3",
                    "-dBATCH",
                    "-dNOPAUSE",
                    "-dUseCIEColor",
                    "-sProcessColorModel=DeviceCMYK",
                    "-sDEVICE=pdfwrite",
                    "-sPDFACompatibilityPolicy=2",
                    $"-sOutputFile={outputFile}",
                    $@"{fileMetadata}",
                    $"{inputFile}",
                };

            ghostscript.Process(t);
            File.Delete(fileMetadata);

            var pdfa = File.ReadAllBytes(outputFile);

            //SEMPRE DELETAR, COMENTADO PARA TESTAR, E PODER VER O DOCUMENTO GERADO(FISICO)
            //File.Delete(inputFile);
            //File.Delete(outputFile);

            return pdfa;
        }

        private string GetMetadatasFile(string title, string author, string cidade)
        {
            var conteudo = $"[ /Title ({title})\r\n " +
                                $"/Author ({author})\r\n " +
                                "/Subject (Bem Promotora)\r\n " +
                                "/Creator (Bem Promotora)\r\n " +
                                $"/ModDate (D:{DateTime.Now.ToString("yyyyMMddHHmmsszzz")})\r\n " +
                                "/Producer (Bem Promotora)\r\n " +
                                $"/cidade_da_assinatura ({cidade})\r\n " +
                                $"/data_hora_criacao ({DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")})\r\n " +
                                $"/CreationDate (D:{DateTime.Now.ToString("yyyyMMddHHmmsszzz")})\r\n" +
                                "/DOCINFO\r\n " +
                            "pdfmark\r\n";

            var filePath = Path.Combine(_diretorioTemporario, "mydocinfo.pdfmark");
            StreamWriter sr = new StreamWriter(filePath);
            sr.WriteLine(conteudo);
            sr.Close();

            return filePath;
        }
    }
}
