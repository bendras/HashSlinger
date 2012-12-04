using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bendras.HashFile;
using CommandLine;
using HashSlingerCore;

namespace HashSlinger
{
    public class RequestProcessor
    {

        #region Ctor

        public RequestProcessor(Options commandLineOptions)
        {
            ProcessRequest(commandLineOptions);
        }

        #endregion


        private void ProcessRequest(Options commandLineOptions)
        {
            StreamHasher fileHashMaker;
            switch (commandLineOptions.HashAgorithm.ToUpper())
            {
                case "MD160":
                    fileHashMaker = new MD160Hasher();
                    break;
                case "SHA1":
                    fileHashMaker = new SHA1Hasher();
                    break;
                case "SHA256":
                    fileHashMaker = new SHA256Hasher();
                    break;
                case "SHA384":
                    fileHashMaker = new SHA384Hasher();
                    break;
                case "SHA512":
                    fileHashMaker = new SHA512Hasher();
                    break;
                case "MD5":
                default:
                    fileHashMaker = new MD5Hasher();
                    break;
            }

            fileHashMaker.HashBlockProcessed += fileHashMaker_HashBlockProcessed;

            List<String[]> inputFiles = new List<String[]>();
            if (commandLineOptions.Concatenate)
            {
                // Files will be treated as a single stream -
                // copy all filenames into a string array,
                // then add the array to the List
                String[] files = new String[commandLineOptions.Items.Count];
                for (int loop = 0; loop < commandLineOptions.Items.Count; loop++)
                {
                    files[loop] = commandLineOptions.Items[loop];
                }
                inputFiles.Add(files);
            }
            else
            {
                // Each file treated as a separate entity -
                // copy each filename into a separate string array,
                // then add each array to the List
                foreach (String fileToProcess in commandLineOptions.Items)
                {
                    String[] file = new String[] { fileToProcess };
                    inputFiles.Add(file);
                }
            }
            foreach (String[] fileEntry in inputFiles)
            {
                byte[] fileHash = fileHashMaker.ComputeFileHash(fileEntry, (int)commandLineOptions.BlockSize);
                Console.WriteLine(commandLineOptions.HashAgorithm.ToUpper() + ": " + BitConverter.ToString(fileHash));

                if (!string.IsNullOrWhiteSpace(commandLineOptions.AppendToHashFile))
                {
                    var settings = HashFile.OpenFile(commandLineOptions.AppendToHashFile);
                    settings.Add(fileEntry[0], BitConverter.ToString(fileHash).Replace("-", string.Empty), commandLineOptions.HashAgorithm.ToUpper());
                    settings.Save();
                }
            }
        }

        void fileHashMaker_HashBlockProcessed(object sender, HasherEventArgs e)
        {
            int koef;
            string format;
            if (e.StreamBytes > 1000000)
            {
                koef = 1000000;
                format = "{0} of {1}MB Progress";
            }
            else
            {
                koef = 1000;
                format = "{0} of {1}KB Progress";
            }


            Console.Title = string.Format(format, e.BytesProcessed / koef, e.StreamBytes / koef);
        }
    }
}
