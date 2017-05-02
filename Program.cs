using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Configuration;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Util;

namespace CamMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Mat frame = new Mat();
            Bitmap bitmap;
            MemoryStream memStream = new MemoryStream();

            try
            {
                VideoCapture vidCap = new VideoCapture();


                vidCap.Open(0);
                if (!vidCap.IsOpened())
                {
                    Console.WriteLine("No capture devices found");
                    return;
                }
                Console.WriteLine("Device found: " + vidCap.Guid);
                // obtain frame from camera
                vidCap.Grab();
                frame = vidCap.RetrieveMat();
                // overlay rectange on Mat
                frame.Rectangle(new Rect(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(frame.Width, 35)), new Scalar(25.0, 25.0, 25.0), -1);
                // overlay date, time and identifier as text
                frame.PutText(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + ConfigurationManager.AppSettings["identifierString"] + "-" + Environment.MachineName, new OpenCvSharp.Point(20, 23), OpenCvSharp.HersheyFonts.HersheyPlain, 1.2, new Scalar(255.0, 255.0, 255.0), 1);

                // convert to bitmap and format as PNG.
                bitmap = BitmapConverter.ToBitmap(frame);
                
                bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);

                bitmap.Dispose();
                frame.Release();
                vidCap.Release();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }


            try
            {
                // Set up FTP request and upload image
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ConfigurationManager.AppSettings["FTPServerUrl"] + ConfigurationManager.AppSettings["FTPUploadDir"] + ConfigurationManager.AppSettings["imageFileName"]);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FTPUsername"], ConfigurationManager.AppSettings["FTPPassword"]);

                Stream requestStream = request.GetRequestStream();
                byte[] fileContents = memStream.ToArray();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                memStream.Dispose();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                response.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            
        }
    }
}
