using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Lox
{
    class Lox
    {
        private static Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            if(args.Length > 2)
            {
                Console.WriteLine("Usage: Lox [script]");
                System.Environment.Exit(64);
            }else if(args.Length == 1)
            {
                runFile(args[0]);
            }else if(args.Length == 2)
            {
                Thread timeoutThread = new Thread(Timeout.sleep);
                string[] timeout = args[1].Split('=');
                int timeToLive = -1;
                if (timeout.Length == 2) 
                {
                    if (timeout[0].ToLower().Equals("timeout"))
                    {
                        try
                        {
                            timeToLive = Int32.Parse(timeout[1]);
                        }catch (FormatException)
                        {
                            Console.WriteLine($"Unable to parse '{timeout[1]}");
                        }
                    }
                }
                if(timeToLive != -1)
                    timeoutThread.Start(timeToLive);

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
                string sourceFile = File.ReadAllText(path);
                run(sourceFile);

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
                hadRuntimeError = false;
            }
        }

        static void run(string fileBytes)
        {
            List<Token> tokens = Token.Tokenize(fileBytes);
            Parser parser = new Parser(tokens);
            List<Statement>  statements = parser.parse();

            if (hadError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.resolve(statements);
            if (hadError || hadRuntimeError) return;

            interpreter.interpret(statements);

            //Console.WriteLine(new AstPrinter().print(expression));

            /*Scanner scanner = new Scanner(fileBytes);
            List<Token> tokens = scanner.scanTokens();
            for(int i = 0; i < tokens.Count; i++)
            {
                Console.WriteLine(tokens.ElementAt(i).toString());
            }*/

        }

        public static void error(int line, String message)
        {
            report(line, -1, "", message);
        }

        public static void error(int line, int character, String message)
        {
            report(line, character, "", message);
        }

        private static void report(int line, int character, String where, String message)
        {
            string build = "[line " + line;
            if (character != -1) build += ":" + character;
            build += "] Error" + where + ": " + message;
            Console.WriteLine(build);
            hadError = true;
        }

        public static void error(Token token, String message)
        {
            if (token.type == Token.TokenType.EOF)
            {
                report(token.line, -1, " at end", message);
            }else
            {
                report(token.line, token.characters, " at '" + token.lexeme + "'", message);
            }
        }

        public static void warn(Token token, String message)
        {
            if (token.type == Token.TokenType.EOF)
            {
                Console.WriteLine("[line " + token.line + "] Warning at end: "+message);
            }
            else
            {
                Console.WriteLine("[line " + token.line + ":" + token.characters + "] Warning at '" + token.lexeme + "' "+ message);
            }
        }

        public static void runtimeError(Exceptions.RuntimeError error)
        {
            string err = error.Message;
            if (error.token.line != -1)
            {
                err += "\n[line " + error.token.line;
                if(error.token.characters != -1)
                {
                    err += ":" + error.token.characters;
                }
                err += "]";
            }
            Console.WriteLine(err);
            hadRuntimeError = true;
        }
    }

    class Timeout
    {
        public static void sleep(object n)
        {
            int i = -1;
            if (n is int)
            {
                i = (int)n;
            }else
            {
                return;
            }

            Thread.Sleep(i * 1000);
            Console.Error.WriteLineAsync("Error: Program timed out after " + i + " seconds.");
            System.Environment.Exit(64);
        }
    }
}
