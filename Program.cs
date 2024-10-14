#pragma warning disable CS8602 
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Element;

namespace PdfRegexChecker
{
    public class Program
    {
        private static readonly string NEWLINE = Environment.NewLine;
        static int GetHelp()
        {
            Console.WriteLine("PDF Utility");
                
            Console.WriteLine(NEWLINE + "s, search <file name> <regex expression>" 
                + NEWLINE 
                + "\tInteractively create a simple PDF.");

            Console.WriteLine(NEWLINE + "c, create [file name]" 
                + NEWLINE 
                + "\tInteractively create a simple PDF.");

            Console.WriteLine(NEWLINE 
                + "ef, encode-file <file name> [out file name]" 
                + NEWLINE 
                + "\tBase64 encode PDF.");

            Console.WriteLine(NEWLINE
                + "df, decode-file <file name> [out file name]" 
                + NEWLINE 
                + "\tDecode base64 PDF. The out file name defaults to newfile.pdf.");
            
            Console.WriteLine(NEWLINE 
                + "ed, encode-directory <directory name> [out directory name]" 
                + NEWLINE 
                + "\tBase64 encode directory of PDFs.");
            
            Console.WriteLine(NEWLINE 
                + "dd, decode-directory <directory name> [out directory name]" 
                + NEWLINE 
                + "\tDecode base64 directory of PDFs.");
    
            Console.WriteLine(NEWLINE 
                + "h, help" 
                + NEWLINE + "\tGet help.");

            return 0;
        }

        static int Search(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Missing file name.");

                return 1;
            }

            var fileName = args[1];
                
            var reader = new PdfReader(fileName);
    
            var d = new PdfDocument(reader);

            var pageCount = d.GetNumberOfPages();

            for(int i = 0; i < pageCount; i++)
            {
                Console.WriteLine("\n{2}\nSearching page {0} of {1}\n{2}\n", 
                    i + 1, pageCount, 
                    new string('-', 80));

                var page = d.GetPage(i + 1);

                var text = PdfTextExtractor.GetTextFromPage(page);

                Console.WriteLine(text);

                string pattern = null;

                if(args.Length == 3)
                {
                    var oldColor = Console.ForegroundColor;

                    Console.ForegroundColor = ConsoleColor.Green;

                    pattern = args[2];
                    
                    Console.WriteLine("\n{1}\nFound matches on expression{0}\n{1}\n", 
                        pattern, 
                        new string('-', 80));

                    Regex.Matches(text, pattern)
                        .ToList()
                        .ForEach(x => Console.WriteLine(x));

                    Console.ForegroundColor = oldColor;
                }
            }

            return 0;
        }

        static int Encode(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing file path.");

                return 1;
            }
            
            var s = File.ReadAllBytes(args[1]);

            var encoded = Convert.ToBase64String(s);

            Console.WriteLine(encoded);

            return 0;
        }

        static int Decode(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing file path.");

                return 1;
            }
            else
            {
                var s = File.ReadAllText(args[1]);

                var decoded = Convert.FromBase64String(s);

                Console.WriteLine(decoded);

                return 0;
            }
        }

        static int Create(string[] args)
        {
            var fileName = args.Length > 1 ? args[1] : $"newfile.pdf";

            using var document = new Document(new PdfDocument(new PdfWriter(fileName)));
        
            var reading = true;

            Console.WriteLine("Enter text for paragraphy annotation." 
                + NEWLINE + "Apply with Enter key." 
                + NEWLINE + "Enter \"EXIT\" to finish.");

            while(reading)
            {
                var input = Console.ReadLine() ?? "";

                if(input.Equals("EXIT"))
                {
                    reading = false;

                    return 0;
                }
                else 
                {
                    document.Add(new Paragraph(input));
                }

                Console.WriteLine("Enter \"EXIT\" to finish.");
            }

            return 1;
        }

        static int EncodeDirectory(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing directory path.");

                return 1;
            }

            var readDirectory = args[1];
            
            if(Directory.Exists(readDirectory))
            {
                var files = Directory.GetFiles(readDirectory);

                string writeDirectory = null;

                if(files.Length > 0)
                {
                    if(args.Length == 3)
                    {
                        writeDirectory = args[2];

                        if(!Directory.Exists(writeDirectory))
                        {
                            Console.WriteLine("Write to directory {0} does not exist. Create y/n?", writeDirectory);
                            
                            var gettingInput = true;

                            while(gettingInput)
                            {
                                var input = Console.ReadLine();

                                if(input.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Creating directory ...");

                                    var currentDirectory = Directory.GetCurrentDirectory();

                                    writeDirectory = Path.Combine(currentDirectory, writeDirectory);

                                    Directory.CreateDirectory(writeDirectory);

                                    Console.WriteLine("Directory created.");

                                    gettingInput = false;
                                }
                                else if(input.Equals("n", StringComparison.OrdinalIgnoreCase))
                                {
                                    gettingInput = false;

                                    writeDirectory = null;
                                }
                                else
                                {
                                    Console.WriteLine("Enter Y/n...");
                                }
                            }                                
                        }
                    }

                    for(int i = 0; i < files.Length; i++)
                    {
                        var s = File.ReadAllBytes(files[i]);

                        var encoded = Convert.ToBase64String(s);

                        if(writeDirectory is null)
                        {
                            Console.WriteLine("{0} of {1}    Encoding file: {2}", 
                                i + 1, 
                                files.Length, 
                                files[i]);

                            Console.WriteLine(encoded);
                        }                   
                        else
                        {
                            var fileName = Path.GetFileName(files[i]) + ".txt";
                            
                            var filePath = Path.Combine(writeDirectory, fileName);

                            File.WriteAllText(filePath, encoded);
                        }
                    }

                    return 0;
                }
                else
                {
                    Console.WriteLine("No files in {0}.", readDirectory);

                    return 0;
                }
            }
            else
            {
                Console.WriteLine("{0} does not exist.", readDirectory);

                return 1;
            }
        }
        
        static int DecodeDirectory(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing directory path.");

                return 1;
            }

            var readDirectory = args[1];
            
            if(Directory.Exists(readDirectory))
            {
                var files = Directory.GetFiles(readDirectory);

                string writeDirectory = null;

                if(files.Length > 0)
                {
                    if(args.Length == 3)
                    {
                        writeDirectory = args[2];
                        
                        if(!Directory.Exists(writeDirectory))
                        {
                            Console.WriteLine("Write to directory {0} does not exist. Create y/n?", writeDirectory);
                            
                            var gettingInput = true;

                            while(gettingInput)
                            {
                                var input = Console.ReadLine();

                                if(input.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Creating directory ...");

                                    var currentDirectory = Directory.GetCurrentDirectory();

                                    writeDirectory = Path.Combine(currentDirectory, writeDirectory);

                                    Directory.CreateDirectory(writeDirectory);

                                    Console.WriteLine("Directory created.");

                                    gettingInput = false;
                                }
                                else if(input.Equals("n", StringComparison.OrdinalIgnoreCase))
                                {
                                    gettingInput = false;

                                    writeDirectory = null;
                                }
                                else
                                {
                                    Console.WriteLine("Enter Y/n...");
                                }
                            }                                
                        }
                    }

                    for(int i = 0; i < files.Length; i++)
                    {
                        var s = File.ReadAllText(files[i]);

                        var decoded = Convert.FromBase64String(s);

                        if(writeDirectory is null)
                        {
                            Console.WriteLine("{0} of {1}    Decoding file: {2}", 
                                i + 1, 
                                files.Length, 
                                files[i]);

                            Console.WriteLine(decoded);

                        }                   
                        else
                        {
                            var fileName = Path.GetFileName(files[i]) + ".pdf";
                            
                            var filePath = Path.Combine(writeDirectory, fileName);

                            File.WriteAllBytes(filePath, decoded);
                        }                            
                    }

                    return 0;
                }
                else
                {
                    Console.WriteLine("No files {0}.", readDirectory);

                    return 0;
                }
            }
            else
            {
                Console.WriteLine("{0} does not exist.", readDirectory);

                return 1;
            } 
        }

        class BlockStatPack
        {
            public string[] AllowList { get; set; }
            public string[] DisAllowList { get; set; }
        }

        static int Read(string[] args)
        {
            Func<string, bool> IsValidBlockData = (blockData) =>
            {
                if(string.IsNullOrEmpty(blockData))
                {
                    return false;
                }
                var data = blockData.Split(' ').Where(x => int.TryParse(x, out var _));

                if(data.Count() == 3)
                {
                    return true;
                }

                return false;
            };
            
            if(File.Exists("searchmeta.txt"))
            {
                var text = File.ReadAllLines("searchmeta.txt");
                
                for(int i = 0; i < text.Length;) 
                {
                    if(IsValidBlockData(text[i]))
                    {
                        var blockTokens = text[i]
                            .Split(' ')
                            .Select(x => int.Parse(x))
                            .ToArray();

                        var id = blockTokens[0];

                        if(id != i + 1)
                        {
                            var oldColor = Console.ForegroundColor;

                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine("Block id: {0} may be invalid.", id);

                            Console.ForegroundColor = oldColor;
                        }

                        var allowCount = blockTokens[1];

                        var disallowCount = blockTokens[2];

                        var allow = text.Skip(i + 1).Take(allowCount);

                        var disallow = text.Skip(i + 1 + allowCount).Take(disallowCount);

                        Console.WriteLine("Allow List:");

                        foreach(var exp in allow)
                        {
                            Console.WriteLine(exp);
                        }

                        Console.WriteLine("Disallow List:");

                        foreach(var exp in disallow)
                        {
                            Console.WriteLine(exp);
                        }

                        i = 1 + i + allowCount + disallowCount;
                    }
                    else
                    {
                        Console.WriteLine("Error! Block data invalid.");
                    }
                }

                return 0;   
            }
            else{
                Console.WriteLine("File does not exist.");

                return 1;
            }
        }

        public static int Main(string[] args)
        {
            #if DEBUG
            args = ["search", "yev_105_receipt.pdf", @"[$][0-9]{2,4}.[0-9]{2}"];
            args = ["r"];
            #endif

            if(args.Length < 1)
            {
                Console.WriteLine("Not enough arguments");

                return 1;
            }

            if(args[0] == "r")
            {
                return Read(args);
            }

            if(args[0] == "h" || args[0] == "help")
            {
                return GetHelp();
            }

            else if(args[0] == "s" || args[0] == "search")
            {
                return Search(args);
            }

            else if (args[0] == "c" || args[0] == "create")
            {
                return Create(args);
            }
            

            else if (args[0] == "ef" || args[0] == "encode-file")
            {
                return Encode(args);
            }

            else if (args[0] == "df" || args[0] == "decode-file")
            {
                return Decode(args);
            }

            else if (args[0] == "ed" || args[0] == "encode-directory")
            {
                return EncodeDirectory(args);
            }

            else if (args[0] == "dd" || args[0] == "decode-directory")
            {
                return DecodeDirectory(args);    
            }

            return 1;
        }
    }
}