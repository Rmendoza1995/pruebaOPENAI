using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OpenAISummarizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var data2 = "";
            var valordetexto = "";
            var valordetiporeporte = "";
            var valordefecha = "";
            List<Salidajson> rawjson = new List<Salidajson>();

            var url1 = @"C:\inetpub\wwwroot\enlistado\josnoparaopenai\Reportestap.json";

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr1 = new StreamReader(url1))
                {
                    // Read the stream to a string, and write the string to the console.
                    data2 = sr1.ReadToEnd();
                    sr1.Close();
                }
            }
            catch (Exception ex)
            {
            }
            var json = data2;


            List<Data> textOrigen = new List<Data>();
            textOrigen = JsonConvert.DeserializeObject<List<Data>>(json);





            foreach (Data role in textOrigen)
            {
                if (role.Texto != "")
                {
                    valordetexto = role.Texto;
                    valordetiporeporte = role.TipoReporte;
                    valordefecha = role.Fecha;
                }


                var laOpinionQueja = valordetexto;
                Console.WriteLine("\n Texto: " + laOpinionQueja);
                Thread.Sleep(16000);

                var summarizedText = SummarizeText(laOpinionQueja, "Summarize the following review in five words: ");
                Console.WriteLine("\n Resumen corto: " + summarizedText);


                var extradatosreument = SummarizeText(laOpinionQueja, "Summarize the following review in one bullet point: ");
                Console.WriteLine("\n Resumen: " + extradatosreument);

                var sentimiento = SummarizeText(laOpinionQueja, "Classify sentiment in one word: ");
                Console.WriteLine("\n Sentimiento: " + sentimiento);



                Console.WriteLine("\n\n\n \n\n\n");
                Console.WriteLine("\n\n\n \n\n\n");

                if (summarizedText != null) { summarizedText = summarizedText.Replace("\n\n", string.Empty); } else { summarizedText = "La solicitud de API falló con el código de estado: TooManyRequests"; }
                if (valordetiporeporte != null) { valordetiporeporte = valordetiporeporte.Replace("\n\n", string.Empty); } else { valordetiporeporte = "La solicitud de API falló con el código de estado: TooManyRequests"; }
                if (extradatosreument != null) { extradatosreument = extradatosreument.Replace("\n\n", string.Empty); } else { extradatosreument = "La solicitud de API falló con el código de estado: TooManyRequests"; }
                if (sentimiento != null) { sentimiento = sentimiento.Replace("\n\n", string.Empty); } else { sentimiento = "La solicitud de API falló con el código de estado: TooManyRequests"; }



                rawjson.Add(new Salidajson
                {
                    TipoReporte = valordetiporeporte,
                    Fecha = valordefecha,
                    TextoCompleto = valordetexto,
                    Resumen = extradatosreument,
                    TextoCorto = summarizedText,
                    Sentimiento = sentimiento
                });





            }
            var taskpostasync = JsonConvert.SerializeObject(rawjson);
            DateTime date1 = DateTime.Now;
            string path = @"C:\inetpub\wwwroot\enlistado\ReporteResumen" + date1.ToString("yyyyMMdd") + ".json";
            System.IO.File.WriteAllText(path, taskpostasync);
        }

        static string SummarizeText(string elTexto, string laInstruccion)
        {
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

                HttpResponseMessage response = null;
                try
                {
                    response = client.PostAsync("https://api.openai.com/v1/engines/text-davinci-003/completions",
                   new StringContent(
                           JsonConvert.SerializeObject(
                               new
                               {
                                   prompt = laInstruccion + elTexto.Trim(),
                                   temperature = 1,
                                   max_tokens = 476
                               }),

                               Encoding.UTF8,
                               "application/json")

                   ).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        var responseJson = JsonConvert.DeserializeObject<dynamic>(responseString);
                        return responseJson.choices[0].text;


                    }
                    else
                    {
                        Console.WriteLine("API request failed with status code: " + response.StatusCode);
                        return null;
                    }
                }
                catch (Exception ex)
                {

                    DateTime date1 = DateTime.Now;

                    string path = @"C:\inetpub\wwwroot\validaIA\log" + date1.ToString("yyyyMMdd") + ".txt";
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(date1.ToString("G"));
                        sw.WriteLine("Llamado:");
                        sw.WriteLine("Excepción:");
                        sw.WriteLine(ex.ToString());
                        sw.WriteLine("----------");
                    }
                    return ex.ToString();
                }



            }
        }
    }
    public class Data
    {
        public string? TipoReporte { get; set; }
        public string? Fecha { get; set; }
        public string? Texto { get; set; }
        public string? Resumen { get; set; }
        public string? Sentimiento { get; set; }
    }
    public class Salidajson
    {
        public string? TipoReporte { get; set; }
        public string? Fecha { get; set; }
        public string? TextoCorto { get; set; }
        public string? TextoCompleto { get; set; }
        public string? Resumen { get; set; }
        public string? Sentimiento { get; set; }
    }


}
