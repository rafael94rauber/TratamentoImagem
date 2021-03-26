using Bem.TratamentoImagem;

namespace TestesDLL
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new ConverterPdf();
            //converter.ExecutarConversao();
            converter.converterImagemParaPdfA();
        }
    }
}
