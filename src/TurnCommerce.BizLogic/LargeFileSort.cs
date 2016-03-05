using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnCommerce.BizLogic
{
    public class LargeFileSort
    {
        /// <summary>
        /// Ensure the existence of the directory C:\TEMP2\exercise02.
        /// Place the file bigfile.txt.gz at the location C:\TEMP2\exercise02. 
        /// The answer file is wrote to C:\TEMP2\exercise02\answer.txt.gz.
        /// </summary>
        /// <returns></returns>
        public async Task<string> RunAsync()
        {
            string inputPath = @"C:\TEMP2\exercise02\bigfile.txt.gz";
            string outputPath = @"C:\TEMP2\exercise02\bigfile.txt";

            if (!File.Exists(inputPath))
            {
                return $"{inputPath} does not exist. Place bigfile.txt.gz at C:\\TEMP2\\exercise02 and run again.";
            }

            using (var input = File.OpenRead(inputPath))
            using (var output = File.OpenWrite(outputPath))
            using (var gz = new GZipStream(input, CompressionMode.Decompress))
            {
                await gz.CopyToAsync(output);
            }

            const string bigfile = @"C:\TEMP2\exercise02\bigfile.txt";

            var set = new SortedList<string, short>();

            int fileCounter = 1;

            List<Task> fileTasks = new List<Task>(245);
            List<string> filenames = new List<string>(245);

            using (StreamReader reader = new StreamReader(bigfile))
            {
                int i = 1;
                string line;
                string filename;

                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        set.Add(line, 0);

                        if (i == 5000)
                        {
                            i = 1;

                            filename = $@"C:\TEMP2\exercise02\file_{fileCounter.ToString("000")}.txt";

                            filenames.Add(filename);
                            fileTasks.Add(WriteFileAsync(filename, new List<string>(set.Keys)));

                            set.Clear();

                            fileCounter++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                filename = $@"C:\TEMP2\exercise02\file_{fileCounter.ToString("000")}.txt";
                filenames.Add(filename);
                fileTasks.Add(WriteFileAsync(filename, new List<string>(set.Keys)));

                set.Clear();
            }

            await Task.WhenAll(fileTasks);

            var fileArray = new Dictionary<string, StreamReader>(245);
            StringBuilder answer = new StringBuilder();
            string answerAsString;

            try
            {
                var mergedLinesArray = new SortedList<string, int>();
                var lineFileLookup = new Dictionary<string, string>();

                foreach (string filename in filenames)
                {
                    StreamReader stream;

                    // store a reference to the stream by the name of the file
                    fileArray[filename] = (stream = new StreamReader(filename));

                    string line = await stream.ReadLineAsync();

                    // SortedDictionary does not allow for duplicates so keep track of how many times we process a line
                    mergedLinesArray.Add(line, 0);

                    // store a reference to the name of the file by the line
                    lineFileLookup[line] = filename;
                }

                while (mergedLinesArray.Count > 0)
                {
                    string smallestLine = mergedLinesArray.Keys.First();
                    mergedLinesArray.Remove(smallestLine);

                    // find the filename of the current smallest line
                    string lookupFilename = lineFileLookup[smallestLine];
                    lineFileLookup.Remove(smallestLine);

                    answer.Append(smallestLine.Substring(28, 1));

                    // get the stream to read the next line of the file from where we got smallest line
                    StreamReader sr = fileArray[lookupFilename];
                    if (sr.EndOfStream)
                    {
                        continue;
                    }

                    // get the next line of the file
                    string nextline = await sr.ReadLineAsync();
                    lineFileLookup[nextline] = lookupFilename;

                    // store the line in our sorted list
                    mergedLinesArray.Add(nextline, 0);
                }
            }
            finally
            {
                answerAsString = answer.ToString();

                using (var outStream = new MemoryStream())
                {
                    using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                    {
                        using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(answerAsString)))
                        {
                            mStream.CopyTo(tinyStream);
                        }
                    }

                    var compressed = outStream.ToArray();

                    using (FileStream fs = File.Create(@"C:\TEMP2\exercise02\answer.txt.gz"))
                    {
                        await fs.WriteAsync(compressed, 0, compressed.Length);

                        await fs.FlushAsync();
                    }
                }
            }

            foreach (var kvp in fileArray)
            {
                kvp.Value.Close();
                kvp.Value.Dispose();
            }

            return answer.ToString();
        }

        private Task WriteFileAsync(string filename, IList<string> lines)
        {
            return Task.Run(() =>
            {
                int lastLine = lines.Count - 1;
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < lines.Count; i++)
                {
                    if (i == lastLine)
                    {
                        // do not write a newline as the last line of the file
                        sb.Append(lines[i]);
                    }
                    else
                    {
                        sb.AppendLine(lines[i]);
                    }
                }

                File.WriteAllText(filename, sb.ToString());
            });
        }
    }
}
