using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace cgi_cs
{
    internal class Program
    {
        private const int min = 100;
        private const int max = 140;

        private static int? attempts = null;
        private static int? hiddenNumber = null;
        private static int inputNumber = 0;

        private static bool blockedForm = false;
        private static bool havePost = false;
        private static bool ErrorDecrypt = false;

        private static string publickey = "90576154";
        private static string secretkey = "57900907";

        [DllImport("kernel32", SetLastError = true)]
        private static extern int SetConsoleMode(int hConsoleHandle, int dwMode);

        private static string? PostData;
        private static int PostLength;

        private static void GatherPostThread()
        {
            if (PostLength > 2048)
            {
                PostLength = 2048;
            }

            for (; PostLength > 0; PostLength--)
            {
                PostData += Convert.ToChar(Console.Read()).ToString();
            }
        }

        private static NameValueCollection? GetPostCollection(string PostData)
        {
            try
            {
                NameValueCollection qscoll = HttpUtility.ParseQueryString(PostData);
                havePost = true;
                return qscoll;
            }
            catch
            {
                havePost = false;
                return null;
            }


        }

        private static string GetOutput(int inputNumber, int? hiddenNumber)
        {
            if (inputNumber >= min && inputNumber <= max && attempts > 0)
            {
                attempts--;
                if (inputNumber == hiddenNumber)
                {
                    blockedForm = true;
                    return $"<p>• Вы выиграли игру. Загаданное число: <mark>{inputNumber}</mark>.</p>";
                }
                else if (attempts == 0)
                {
                    blockedForm = true;
                    return $"<p>• Попытки закончились и вы проиграли. Загаданное число: {hiddenNumber}.</p>";
                }
                else if (inputNumber < hiddenNumber)
                {
                    return $"<p>• Загаданное число больше <mark>{inputNumber}</mark>.</p>";
                }
                else if (inputNumber > hiddenNumber)
                {
                    return $"<p>• Загаданное число меньше <mark>{inputNumber}</mark>.</p>";
                }
            }
            else
            {
                return $"<p>• Введённое число <mark>{inputNumber}</mark> не входит в диапазон.</p>";
            }

            return "";
        }

        private static void PrintHead()
        {
            Console.Write("Content-Type: text/html; charset=windows-1251\n\n");




            Console.Write(@$"
<!doctype html>
<html lang=""ru"">

<head>
  <meta charset=""windows-1251"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>Угадай число</title>
  <link rel=""stylesheet"" href=""css/bootstrap.min.css"">
  <link rel=""stylesheet"" href=""css/style.css"">
</head>");
        }

        private static void PrintHeader()
        {
            string disabledAttribute = "";
            if (blockedForm)
            {
                disabledAttribute = "disabled";
            }

            Console.Write(@$"<body>
  <div class=""main"">
    <div class=""container"">
      <h1 class=""mt-3"" id=""title"">Игра ""Угадай число""</h1>
      <p class=""lead"" id=""range"">Компьютер загадал число от [{min};{max}]</p><p id=""tries"">Осталось попыток: {attempts}</p>
      <form action=""index.cgi"" id=""input-group"" method=""post"">
        <div class=""input-group"">
          <input type=""number"" id=""input-number"" class=""form-control"" placeholder=""Число"" name=""input_number"" {disabledAttribute} autofocus>
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""hidden_number"" value=""{Encrypt(hiddenNumber.ToString())}"">
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""attempts"" value=""{Encrypt(attempts.ToString())}"">
          <button class=""btn btn-primary"" type=""submit"" id=""check"" {disabledAttribute}>Проверить</button>
          <a class=""btn btn-outline-secondary"" href=""{System.Environment.GetEnvironmentVariable("SCRIPT_NAME")}"">Заново</a>        
        </div>
      </form>
      <hr/>

      ");

            
            
        }

        public static void PrintStatus(string outputText)
        {
            Console.Write($@"<h3>Текущий статус:</h3>
      <div class=""row"" id=""custom-actions"">
               
        {outputText}

      </div>

    </div>


  </div>


  <script src=""js/bootstrap.min.js""></script>
  <!-- <script src=""js/game.js""></script> -->
</body>

</html>");
        }



        private static string Encrypt(string textToEncrypt)
        {
            try
            {

                string toReturn = "";
                

                byte[] secretKeyByte = { };
                byte[] publickeyByte = { };

                secretKeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                publickeyByte = System.Text.Encoding.UTF8.GetBytes(publickey);

                MemoryStream? ms = null;
                CryptoStream? cs = null;

                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeyByte, secretKeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    toReturn = Convert.ToBase64String(ms.ToArray());
                }
                return toReturn;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private static string Decrypt(string textToDecrypt)
        {
            try
            {
                string toReturn = "";
                

                byte[] secretKeyByte = { };
                byte[] publicKeyByte = { };

                secretKeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                publicKeyByte = System.Text.Encoding.UTF8.GetBytes(publickey);

                MemoryStream? ms = null;
                CryptoStream? cs = null;

                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));

                using (DESCryptoServiceProvider des = new())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publicKeyByte, secretKeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    toReturn = encoding.GetString(ms.ToArray());
                }
                return toReturn;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message, ex.InnerException);
            }
        }


        [STAThread]
        private static void Main(string[] args)
        {
            Random rnd = new();

            string outputText = "<p>• Началась новая игра.</p>";

            ThreadStart ThreadDelegate = new(GatherPostThread);
            Thread PostThread = new(ThreadDelegate);
            PostLength = Convert.ToInt32(System.Environment.GetEnvironmentVariable("CONTENT_LENGTH"));
            int LengthCompare = PostLength;

            if (PostLength > 0)
            {
                PostThread.Start();
            }

            while (PostLength > 0)
            {
                Thread.Sleep(100);
                if (PostLength < LengthCompare)
                {
                    LengthCompare = PostLength;
                }
                else
                {
                    PostData += "Error with POST data or connection problem.";
                    break;
                }
            }

            NameValueCollection? qs = GetPostCollection(PostData);

            if (havePost && int.TryParse(qs["input_number"], out int postNumber))
            {
                inputNumber = postNumber;
            }


            if (havePost)
            {
                try
                {

                    hiddenNumber = int.Parse(Decrypt(qs["hidden_number"]));
                    attempts = int.Parse(Decrypt(qs["attempts"]));

                    outputText = GetOutput(inputNumber, hiddenNumber);
                }
                catch
                {
                    blockedForm = true;
                    ErrorDecrypt = true;
                }
            }
            else
            {
                hiddenNumber = rnd.Next(min, max);
                attempts = (int)Math.Ceiling(Math.Log2(max - min + 1));
            }

            _ = SetConsoleMode(3, 0);

            PrintHead();

            PrintHeader();
                        
            if (ErrorDecrypt)
            {
                outputText = "<p>• Произошла попытка взлома. Ввод чисел заблокирован до полного сброса.</p>";
            }

            PrintStatus(outputText); 

            Environment.Exit(0);
        }  
    }



}