using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    class Lox
    {
        static bool hadError = false;

        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                Environment.Exit(64);
            }else if(args.Length == 1)
            {
                runFile(args[0]);
            }else
            {
                runPrompt();
            }
        }

        static void runFile(string path)
        {
            if(File.Exists(path))
            {
                byte[] Filebytes = File.ReadAllBytes(path);
                run(Encoding.BigEndianUnicode.GetString(Filebytes));

                if (hadError)
                {
                    Environment.Exit(64);
                }

            }
        }

        static void runPrompt()
        {
            for(; ;)
            {
                Console.Write("> ");
                run(Console.ReadLine());
                hadError = false;

            }
        }

        static void run(string fileBytes)
        {
            List<Token> tokens = Token.Tokenize(fileBytes);
            //Scanner scanner = new Scanner(fileBytes);
            //List<Token> tokens = scanner.scanTokens();

            for(int i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine(tokens.ElementAt(i).toString());
            }

        }

        public static void error(int line, String message)
        {
            report(line, "", message);
        }

        private static void report(int line, String where, String message)
        {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}
