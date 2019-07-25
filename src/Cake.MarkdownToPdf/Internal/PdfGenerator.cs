using Cake.Core.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Cake.MarkdownToPdf.Internal
{
    /// <summary>
    /// Converts the html into a pdf file
    /// </summary>
    public class PdfGenerator
    {
        /// <summary>
        /// Gets the console output of the converter tool
        /// </summary>
        public string ExecutionOutputText { get; private set; }

        /// <summary>
        /// Converts the given html file to a pdf file using the settings
        /// </summary>
        /// <param name="htmlFile">File path to the generated html file</param>
        /// <param name="outputFile">Name of the pdf tile to be generated</param>
        /// <param name="settings"></param>
        /// <param name="baseDirectory">Directory of the addin</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public int ConvertToPdf(string htmlFile, string outputFile, PdfSettings settings, string baseDirectory, ICakeLog log)
        {
            var sb = new StringBuilder();

            sb.Append($"--image-dpi {settings.ImageDpi} ");
            sb.Append($"--image-quality {settings.ImageQuality} ");
            sb.Append($"--page-size {settings.PageSize} ");
            sb.Append($"--orientation {settings.Orientation} ");
            sb.Append("--print-media-type ");

            if (settings.Margins.Left > 0)
                sb.Append($"--margin-left {settings.Margins.Left}mm ");
            if (settings.Margins.Right > 0)
                sb.Append($"--margin-right {settings.Margins.Right}mm ");
            if (settings.Margins.Top > 0)
                sb.Append($"--margin-top {settings.Margins.Top}mm ");
            if (settings.Margins.Bottom > 0)
                sb.Append($"--margin-bottom {settings.Margins.Bottom}mm ");

            // don' use --disable-smart-shrinking
            // this zooms/fits the content "correct" to the page but the font kerning is a mess
            // use --zom 1.3 instead
            sb.Append("--zoom 1.3 ");
            sb.Append("--dpi 300 ");

            // in and out files
            sb.Append($"\"{htmlFile}\" \"{outputFile}\" ");

            var outDir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            return Convert(settings.PathToWkhtmltopdf, sb.ToString(), baseDirectory, log);
        }

        private int Convert(string wkhtmltopdfPath, string switches, string baseDirectory, ICakeLog log)
        {
            if (string.IsNullOrEmpty(wkhtmltopdfPath))
                wkhtmltopdfPath = Path.Combine(baseDirectory, "wkhtmltopdf.exe");

            log.Debug("Run html to pdf converter: " + wkhtmltopdfPath);
            log.Debug("Using switches: " + switches);

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = wkhtmltopdfPath,
                    Arguments = switches,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            var output = new StringBuilder();
            ExecutionOutputText = "";

            void OnOutputDataReceived(object o, DataReceivedEventArgs e) => output.AppendLine(e.Data);

            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnOutputDataReceived;

            try
            {
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool result = process.WaitForExit(3 * 60 * 1000);

                if (!result)
                    return -10;
            }
            catch (Exception e)
            {
                output.AppendLine(e.ToString());
                log.Error(e);
                return -1;
            }
            finally
            {
                process.OutputDataReceived -= OnOutputDataReceived;
                process.ErrorDataReceived -= OnOutputDataReceived;
            }

            ExecutionOutputText = output.ToString();

            return process.ExitCode;
        }
    }
}
