using rdiff.net.logic;
using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using rdiff.net.models;
using Newtonsoft.Json;

namespace rdiff.net
{
    class Program
    {
        static int Main(string[] args)
        {
            var signatureCommand = new Command("signature", "Calculates siganture for given file.")
            {
                new Argument<string>("filePath" , "Path to file for which the signature is calculated."),
                new Argument<string>("outputFilePath" , "Where to save signature output."),
                new Option<int>("-b", description: "Signature block size, example: signature -b 1024 file.txt", getDefaultValue: () => Consts.DEFAULT_BLOCK_LENGTH),
                new Option<int>("-S", description: "Set signature strength, example: signature -S 32 file.txt", getDefaultValue: () => Consts.DEFAULT_STRONG_SIGNATURE_LENGTH)
            };
            signatureCommand.Handler = CommandHandler.Create<int, int, FileInfo, FileInfo, IConsole>(HandleSignature);

            var deltaCommand = new Command("delta", "Calcuates delat for given signature file and new file.")
            {
                new Argument<string>("signatureFilePath", description: "Signature file."),
                new Argument<string>("newFilePath", description: "File which will be compared to delta."),
                new Argument<string>("deltaOutputFilePath", description: "Delta result file path.")
            };
            deltaCommand.Handler = CommandHandler.Create<FileInfo, FileInfo, FileInfo, IConsole>(HandleDelta);

            var cmd = new RootCommand { signatureCommand, deltaCommand };

            return cmd.Invoke(args);
        }

        private static void HandleSignature(int b, int S, FileInfo filePath, FileInfo outputFilePath, IConsole console)
        {
            if(!filePath.Exists)
            {
                console.Error.Write("Input file does not exist or is not accessible.");
                return;
            }

            using var fileReader = new FileBytesReader(filePath);
            var signatureCalculation = new SignatureCalculation();
            var resultSignature = signatureCalculation.CalculateSignature(fileReader, b, S);

            try
            {
                using var writeStream = outputFilePath.OpenWrite();
                Serialize(resultSignature, writeStream);
            }
            catch (Exception exc)
            {
                console.Error.Write($"Problem saving the result to: {outputFilePath.FullName}, reason: {exc.Message}");
                throw;
            }
        }

        private static void HandleDelta(FileInfo signatureFilePath, FileInfo newFilePath, FileInfo deltaOutputFilePath, IConsole console)
        {
            if (!signatureFilePath.Exists)
            {
                console.Error.Write("Signature file does not exist or is not accessible.");
                return;
            }

            if (!newFilePath.Exists)
            {
                console.Error.Write("New file does not exist or is not accessible.");
                return;
            }
            using var signatureFileStream = signatureFilePath.OpenRead();
            using var newFile = new FileBytesReader(newFilePath);
            var signature = Deserialize<Signature>(signatureFileStream);
            var deltaCalculation = new DeltaCalculation();
            var resultDelta = deltaCalculation.CalculateDelta(signature, newFile);

            try
            {
                using var writeStream = deltaOutputFilePath.OpenWrite();
                Serialize(resultDelta, writeStream);
            }
            catch (Exception exc)
            {
                console.Error.Write($"Problem saving the result delta to: {deltaOutputFilePath.FullName}, reason: {exc.Message}");
                throw;
            }
        }

        public static void Serialize(object value, Stream s)
        {
            using (StreamWriter writer = new StreamWriter(s))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }

        public static T Deserialize<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }
    }
}
