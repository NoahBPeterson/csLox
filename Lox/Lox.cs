using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Lox
{
    class Lox
    {
        private static Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                System.Environment.Exit(64);
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
                    System.Environment.Exit(65);
                }
                if(hadRuntimeError)
                {
                    System.Environment.Exit(70);
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
            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.parse();

            if (hadError) return;

            interpreter.interpret(statements);

            /*Console.WriteLine(new AstPrinter().print(expression));

            Scanner scanner = new Scanner(fileBytes);
            List<Token> tokens = scanner.scanTokens();
            for(int i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine(tokens.ElementAt(i).toString());
            }*/

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

        public static void error(Token token, String message)
        {
            if (token.type == Token.TokenType.EOF)
            {
                report(token.line, " at end", message);
            }else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void runtimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }
    }
}
