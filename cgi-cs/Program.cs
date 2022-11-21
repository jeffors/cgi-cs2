using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace cgi_cs
{
    internal class Program
    {
        const int min = 100;
        const int max = 140;
        private static int? attempts = null;
        private static int? hiddenNumber = null;
        private static int inputNumber = 0;
        private static bool blockedForm = false;
        private static bool havePost = false;
        private static bool ErrorDecrypt = false;

        [DllImport("kernel32", SetLastError = true)]
        static extern int SetConsoleMode(int hConsoleHandle, int dwMode);

        private static string PostData;
        private static int PostLength;

        private static void GatherPostThread()
        {
            if (PostLength > 2048) PostLength = 2048;  // Max length for POST data for security.
            for (; PostLength > 0; PostLength--)
                PostData += Convert.ToChar(Console.Read()).ToString();
        }

        private static NameValueCollection GetInputNumber(string PostData)
        {
            NameValueCollection qscoll = HttpUtility.ParseQueryString(PostData);
            return qscoll;
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
                    return $"<p>• Попытки закончились и вы проиграли. Загаданное число: {hiddenNumber}</p>";
                }
                else if (inputNumber < hiddenNumber)
                {
                    return $"<p>• Загаданное число больше <mark>{inputNumber}</mark>. Попыток осталось: {attempts}</p>";
                }
                else if (inputNumber > hiddenNumber)
                {
                    return $"<p>• Загаданное число меньше <mark>{inputNumber}</mark>. Попыток осталось: {attempts}</p>";
                }
            }
            else
            {
                return $"<p>• Введённое число <mark>{inputNumber}</mark> не входит в диапазон. Попыток осталось: {attempts}</p>";
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
            if (blockedForm)
            {
                Console.Write(@$"<body>
  <div class=""main"">
    <div class=""container"">
      <h1 class=""mt-3"" id=""title"">Игра ""Угадай число""</h1>
      <p class=""lead"" id=""range"">Компьютер загадал число от [{min};{max}]</p><p id=""tries"">Задача: отгадать число за {attempts} попыток</p>
      <form action=""index.cgi"" id=""input-group"" method=""post"">
        <div class=""input-group"">
          <input type=""number"" id=""input-number"" class=""form-control"" placeholder=""Число"" name=""input_number"" disabled>
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""hidden_number"" value=""{Encrypt(hiddenNumber.ToString())}"">
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""attempts"" value=""{Encrypt(attempts.ToString())}"">
          <button class=""btn btn-primary"" type=""submit"" id=""check"" disabled>Проверить</button>
          <a class=""btn btn-outline-secondary"" href=""/cgi/"">Заново</a>        
        </div>
      </form>
      <hr/>

      ");
            }
            else
            {
                Console.Write(@$"<body>
  <div class=""main"">
    <div class=""container"">
      <h1 class=""mt-3"" id=""title"">Игра ""Угадай число""</h1>
      <p class=""lead"" id=""range"">Компьютер загадал число от [{min};{max}]</p><p id=""tries"">Задача: отгадать число за {attempts} попыток</p>
      <form action=""index.cgi"" id=""input-group"" method=""post"">
        <div class=""input-group"">
          <input type=""number"" id=""input-number"" class=""form-control"" placeholder=""Число"" name=""input_number"" autofocus>
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""hidden_number"" value=""{Encrypt(hiddenNumber.ToString())}"">
          <input type=""hidden"" id=""input-number"" class=""form-control"" name=""attempts"" value=""{Encrypt(attempts.ToString())}"">
          <button class=""btn btn-primary"" type=""submit"" id=""check"">Проверить</button>
          <a class=""btn btn-outline-secondary"" href=""/cgi/"">Заново</a>        
        </div>
      </form>
      <hr/>

      ");
            }
        }

        

        private static string Encrypt(string textToEncrypt)
        {
            try
            {
                
                string toReturn = "";
                string publickey = "12345678";
                string secretkey = "87654321";

                byte[] secretKeyByte = { };
                byte[] publickeyByte = { };

                secretKeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                publickeyByte = System.Text.Encoding.UTF8.GetBytes(publickey);

                MemoryStream ms = null;
                CryptoStream cs = null;

                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
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
                string publickey = "12345678";
                string secretkey = "87654321";

                byte[] secretKeyByte = { };
                byte[] publicKeyByte = { };

                secretKeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                publicKeyByte = System.Text.Encoding.UTF8.GetBytes(publickey);

                MemoryStream ms = null;
                CryptoStream cs = null;

                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));

                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
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
        static void Main(string[] args)
        {
            


            Random rnd = new Random();

            
            string outputText = "";



            ThreadStart ThreadDelegate = new ThreadStart(GatherPostThread);
            Thread PostThread = new Thread(ThreadDelegate);
            PostLength = Convert.ToInt32(System.Environment.GetEnvironmentVariable("CONTENT_LENGTH"));
            int LengthCompare = PostLength;

            if (PostLength > 0) PostThread.Start();

            while (PostLength > 0)
            {
                Thread.Sleep(100);
                if (PostLength < LengthCompare)
                    LengthCompare = PostLength;
                else
                {
                    PostData += "Error with POST data or connection problem.";
                    break;
                }
            }

            StringBuilder sb = new StringBuilder("<br/>");

            try
            {
                var qs = GetInputNumber(PostData);

                foreach (string s in qs.AllKeys)
                {
                    sb.Append(s + " - " + qs[s] + "<br />");
                }

                

                if (int.TryParse(qs["input_number"], out var postNumber))
                {
                    inputNumber = postNumber;
                }

                havePost = true;
            }
            catch 
            {
                havePost = false;
            }

            if (havePost)
            {
                try
                {
                    var qs = GetInputNumber(PostData);

                    if (!string.IsNullOrWhiteSpace(qs["hidden_number"]) && int.TryParse(Decrypt(qs["hidden_number"]), out var postHiddenNumber))
                    {
                        hiddenNumber = postHiddenNumber;
                    }

                    if (!string.IsNullOrWhiteSpace(qs["attempts"]) && int.TryParse(Decrypt(qs["attempts"]), out var postAttempts))
                    {
                        attempts = postAttempts;
                    }
                }
                catch
                {
                    blockedForm = true;
                    ErrorDecrypt = true;
                }
            }

            if (hiddenNumber == null)
            {
                hiddenNumber = rnd.Next(100, 140);
            }

            if (attempts == null)
            {
                attempts = (int)Math.Ceiling(Math.Log2(160 - 100 + 1));
            }
            else
            {
                
                outputText = GetOutput(inputNumber, hiddenNumber);
            }

            string encrypted = "";
            try
            {
                //encrypted = Encrypt();
                
            }
            catch (Exception ex)
            {
                encrypted = ex.Message + "\n";
            }

            string decrypted = "";
            try
            {
                //decrypted = Decrypt();
            }
            catch (Exception ex)
            {
                decrypted = ex.Message + "\n";
            }

            SetConsoleMode(3, 0);

            PrintHead();

            PrintHeader();









            if (ErrorDecrypt)
            {
                outputText = "<p>• Кажется, вас зовут Калитаев Александр Николаевич. А студенты вас зовут просто занудой</p>";
            }

            

            Console.Write($@"<h3>Ваши действия:</h3>
      <div class=""row"" id=""custom-actions"">
        {sb.ToString()} 
        
        {inputNumber} {hiddenNumber} {attempts}
        {outputText}
{encrypted}
{decrypted}
      </div>

    </div>


  </div>


  <script src=""js/bootstrap.min.js""></script>
  <!-- <script src=""js/game.js""></script> -->
</body>

</html>");


            Environment.Exit(0);
        }  // End of Main().
    }



}