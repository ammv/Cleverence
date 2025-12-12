using System.Reflection;

namespace Cleverence.TextCompression.ConsoleApp
{
    /// <summary>
    /// The main program class responsible for handling user commands and performing operations such as listing available transformers, compressing, and decompressing data.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of the application which processes command-line arguments to perform actions like showing help, listing transformers, compressing, or decompressing files.
        /// </summary>
        /// <param name="args">Command line arguments passed by the user.</param>
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("help"))
                ShowHelp();
            else if (args[0].Equals("list"))
                ListImplementations();
            else if (args[0].Equals("compress") && args.Length >= 4)
                PerformCompression(args[1], args[2], args[3]);
            else if (args[0].Equals("decompress") && args.Length >= 4)
                PerformDecompression(args[1], args[2], args[3]);
            else
                Console.WriteLine("Invalid command or arguments.");
        }

        /// <summary>
        /// Displays help information on how to use the application.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("TextCompressionApp help\tShow this help message");
            Console.WriteLine("TextCompressionApp list\tList available string compression methods");
            Console.WriteLine("TextCompressionApp compress CompressionMethod InputFilePath OutputFilePath");
            Console.WriteLine("TextCompressionApp decompress CompressionMethod InputFilePath OutputFilePath");
        }

        /// <summary>
        /// Lists all available string transformers implemented in the current assembly.
        /// </summary>
        private static void ListImplementations()
        {
            Type interfaceType = typeof(IStringTransformer);

            var assembly = Assembly.GetAssembly(interfaceType);
            var types = assembly.GetTypes().Where(interfaceType.IsAssignableFrom);

            foreach (var type in types.Except(new[] { interfaceType }))
            {
                Console.WriteLine(type.Name);
            }
        }

        /// <summary>
        /// Performs compression of a file's content using the specified transformer.
        /// </summary>
        /// <param name="transformerName">The name of the transformer implementation to be used.</param>
        /// <param name="inputFilePath">The path to the input file containing uncompressed data.</param>
        /// <param name="outputFilePath">The path where the compressed data will be saved.</param>
        private static void PerformCompression(string transformerName, string inputFilePath, string outputFilePath)
        {
            try
            {
                Type transformerType = GetTransformerByName(transformerName);

                if (transformerType != null)
                {
                    var instance = Activator.CreateInstance(transformerType) as IStringTransformer;

                    if (instance != null)
                    {
                        string fileContent = File.ReadAllText(inputFilePath);
                        string compressedData = instance.Compress(fileContent);

                        File.WriteAllText(outputFilePath, compressedData);
                        Console.WriteLine($"Successfully compressed '{inputFilePath}' into '{outputFilePath}'.");
                    }
                    else
                    {
                        throw new Exception("Could not create an instance of the specified transformer.");
                    }
                }
                else
                {
                    throw new Exception("Specified transformer does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during compression: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs decompression of a file's content using the specified transformer.
        /// </summary>
        /// <param name="transformerName">The name of the transformer implementation to be used.</param>
        /// <param name="inputFilePath">The path to the input file containing compressed data.</param>
        /// <param name="outputFilePath">The path where the decompressed data will be saved.</param>
        private static void PerformDecompression(string transformerName, string inputFilePath, string outputFilePath)
        {
            try
            {
                Type transformerType = GetTransformerByName(transformerName);

                if (transformerType != null)
                {
                    var instance = Activator.CreateInstance(transformerType) as IStringTransformer;

                    if (instance != null)
                    {
                        string fileContent = File.ReadAllText(inputFilePath);
                        string decompressedData = instance.Decompress(fileContent);

                        File.WriteAllText(outputFilePath, decompressedData);
                        Console.WriteLine($"Successfully decompressed '{inputFilePath}' into '{outputFilePath}'.");
                    }
                    else
                    {
                        throw new Exception("Could not create an instance of the specified transformer.");
                    }
                }
                else
                {
                    throw new Exception("Specified transformer does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during decompression: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a transformer type based on its name from the current assembly.
        /// </summary>
        /// <param name="transformerName">The name of the transformer to retrieve.</param>
        /// <returns>The corresponding Type object representing the transformer, or null if it doesn't exist.</returns>
        private static Type GetTransformerByName(string transformerName)
        {
            return Assembly.GetAssembly(typeof(IStringTransformer))
                          .GetTypes()
                          .FirstOrDefault(t => t.Name.Equals(transformerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}