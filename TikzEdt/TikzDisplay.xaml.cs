﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PDFLibNet;

namespace TikzEdt
{
    /// <summary>
    /// Interaction logic for TikzDisplay.xaml 
    /// NOT THREAD-SAFE SO FAR
    /// </summary>
    public partial class TikzDisplay : UserControl
    {
        //public enum CompileEventType {Start, Error, Success, Status};
        //public delegate void CompileEventHandler(string Message, CompileEventType type);
        //public event CompileEventHandler OnCompileEvent;
        //public delegate void TexOutputHandler(string Message);
        //public event TexOutputHandler OnTexOutput;

        /*readonly public static DependencyProperty CompilingProperty = DependencyProperty.Register(
                "Compiling", typeof(bool), typeof(TikzDisplay));
        public bool Compiling
        {
            //if pre-compiling was started, isRunning stays true. No other compiliation can be started.
            //could CompilingProperty and isRunning be merged?
            get { return (bool)GetValue(CompilingProperty); }
            set { }
        }*/

  /*      public void Compile(string code, Rect BB, bool IsStandAlone)
        {
            if (IsStandAlone)
                nextToCompile = code;
            else nextToCompile = @"%&" + Consts.cTempFile + "\r\n\\begin{document}\r\n" + code + "\r\n" + Properties.Settings.Default.Tex_Postamble;  
            nextBB = BB;
            doCompile();
        }
   * */

        private double _Resolution = Consts.ptspertikzunit;
        public double Resolution
        {
            get { return _Resolution; }
            set {
                if (value > 0)
                {
                    _Resolution = value;
                    RecalcSize();
                }
            }
        }

        //protected Process texProcess = new Process();
        //protected String nextToCompile = "";
        Rect currentBB;
        //protected bool isRunning = false;
        //PDFLibNet.PDFWrapper mypdfDoc = null;
        PdfToBmp myPdfBmpDoc = new PdfToBmp();

        
        TexCompiler _TexCompilerToListen;
        public TexCompiler TexCompilerToListen
        {
            get { return _TexCompilerToListen; }
            set 
            {
                if (_TexCompilerToListen != null)
                    _TexCompilerToListen.JobSucceeded -= new TexCompiler.JobEventHandler(TexCompilerToListen_JobSucceeded);
                _TexCompilerToListen = value;
                if (_TexCompilerToListen != null)
                    _TexCompilerToListen.JobSucceeded += new TexCompiler.JobEventHandler(TexCompilerToListen_JobSucceeded);
            }
        }

        void TexCompilerToListen_JobSucceeded(object sender, TexCompiler.Job job)
        {
            // reload the pdf upon successful compilation
            if (!job.GeneratePrecompiledHeaders)
            {
                string pdfpath = Helper.RemoveFileExtension(job.path) + ".pdf";
                if (job.hasBB)
                {
                    currentBB = job.BB;
                    // very small pdfs are cut off
                    //if (currentBB.Width < 0.05 || currentBB.Height < 0.05)
                    //    currentBB.Inflate(0.05, 0.05);
                }
                else
                    currentBB = new Rect(0, 0, 0, 0);
                RefreshPDF(pdfpath);
            }
        }
        
        //System.Windows.Forms.Control dummy = new System.Windows.Forms.Control();

        /// <summary>
        /// If the compilation gets stuck (actually it shouldn't), 
        /// one can call this method to kill the pdflatex-process.
        /// </summary>
  /*      public void AbortCompilation()
        {
            try
            {
                if (!texProcess.HasExited)
                    texProcess.Kill();
            }
            catch (InvalidOperationException)
            {
                isRunning = false;
                //process has already terminated. that is okay.
            }
        }
        */

        /// <summary>
        /// The main routine, starts the compilation of the Tikz-Picture.
        /// If necessary it initiates compilation of the precompiled headers.
        /// </summary>
    /*    protected void doCompile()
        {
            if (isRunning || nextToCompile == "")
            {
                return;
            }
            isRunning = true;
            SetValue(CompilingProperty, true);

            if (!File.Exists(Consts.cTempFile + ".fmt"))
            {
                OnCompileEvent("Generating precompiled headers.... please restart in some moments", CompileEventType.Status); 
                Helper.GeneratePrecompiledHeaders();
                //GeneratePrecompiledHeaders() has no callback function. thus:                
                isRunning = false;
                return;
            }

            // save into temporary textfile
            // add bounding box, if bounding box provided has size other than 0
            bool lsucceeded= true;
            string codetowrite;
            if (nextBB.Width * nextBB.Height == 0)
                codetowrite = nextToCompile;
            else
                codetowrite = writeBBtoTikz(nextToCompile, nextBB, out lsucceeded);

            StreamWriter s = new StreamWriter(Consts.cTempFile + ".tex");
 
            s.WriteLine(codetowrite);
            s.Close();
            nextToCompile = "";
            if (lsucceeded)
                compilingBB = nextBB;
            else compilingBB = new Rect(0, 0, 0, 0);

            // call pdflatex         
            OnCompileEvent("Compiling document for preview: " + texProcess.StartInfo.FileName + " " + texProcess.StartInfo.Arguments, CompileEventType.Start);
            
            //clear error windows
            ((MainWindow)Application.Current.Windows[0]).txtTexout.Document.Blocks.Clear();
            ((MainWindow)Application.Current.Windows[0]).clearProblemMarkers();
            

            try
            {
                if (texProcess.HasExited == true)
                {
                    texProcess.CancelOutputRead();
                    texProcess.CancelErrorRead();
                }
            }
            catch (InvalidOperationException)
            {
                //on first call when texProcess was not started, HasExited raises exception.
            }

            texProcess.Start();
            texProcess.BeginOutputReadLine();
            texProcess.BeginErrorReadLine();
            
        }
     * */
        /// <summary>
        /// Adds a rectangle to the Tikzcode in the size specified by BB. 
        /// The rectangle is added as the last command before the \end{tikzpicture} 
        /// </summary>
        /// <param name="code">The Tikz Code. Must contain an "\end{tikzpicture}" </param>
        /// <param name="BB">The bounding box (= size of rectangle to be written) </param>
        /// <param name="succeeded">Returns success, i.e., whether the string "\end{tikzpicture}" has been found</param>
        /// <returns>The Tikzcode, with the "\draw rectangle ...." inserted </returns>
 /*       string writeBBtoTikz(string code, Rect BB, out bool succeeded)
        {
            // hack
            string cend = @"\end{tikzpicture}"; // hack
            string[] tok = code.Split(new string[] { cend }, StringSplitOptions.None);
            succeeded = (tok.Length == 2 && nextBB.Width * nextBB.Height > 0); //TODO: check
            if (succeeded)
                return tok[0] + @"\draw (" + BB.X + "," + BB.Y + ") rectangle (" + (BB.X + BB.Width).ToString() + "," + (BB.Y + BB.Height).ToString() + ");\r\n " + cend + tok[1];
            else
                return code;
        }*/

        public TikzDisplay()
        {
            InitializeComponent();

            /* texProcess.EnableRaisingEvents = true;
            texProcess.StartInfo.Arguments = "-halt-on-error " + Consts.cTempFile + ".tex";
            texProcess.StartInfo.FileName = "pdflatex";
            texProcess.StartInfo.CreateNoWindow = true;
            texProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            texProcess.StartInfo.UseShellExecute = false;
            texProcess.StartInfo.RedirectStandardOutput = true;
            texProcess.StartInfo.RedirectStandardError = true;
            
            // texProcess.SynchronizingObject = (System.ComponentModel.ISynchronizeInvoke) this;
            texProcess.Exited += new EventHandler(texProcess_Exited);
            texProcess.OutputDataReceived += new DataReceivedEventHandler(texProcess_OutputDataReceived);
            texProcess.ErrorDataReceived += new DataReceivedEventHandler(texProcess_ErrorDataReceived); */
        }

 /*       void texProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string s = "ErrorDataReceived: " + e.Data;
            Dispatcher.Invoke(
                new Action(
                    delegate()
                    {
                        if (OnTexOutput != null)
                            OnTexOutput(s);
                    }
                )
            );
        }
        
        void texProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(
                new Action(
                    delegate()
                    {
                        if (OnTexOutput != null)
                            OnTexOutput(e.Data);
                    }
                )
            );
        } */

        /// <summary>
        /// Reload the PDF file. This is called only when the pdf file changes on disk.
        /// It is not called, for example, when the pdf just needs to be redrawn, e.g., due to 
        /// a changed display size.
        /// </summary>
        void RefreshPDF(string cFile)
        {
            //mypdfDoc.LoadPdf(Consts.cTempFile + ".pdf");
            if (cFile == "")
            {
                lblUnavailable.Visibility = Visibility.Visible;
                image1.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblUnavailable.Visibility = Visibility.Collapsed;
                myPdfBmpDoc.LoadPdf(cFile);                
                image1.Visibility = Visibility.Visible;                
            }
            RecalcSize();            
        }
        public void SetUnavailable()
        {
            RefreshPDF("");

            myPdfBmpDoc.UnloadPdf();
            //here it would be nice to release the handle to the pdf document
            //so it can be deleted. but how?
        }

        /// <summary>
        /// This is called when PDFLatex has exited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
   /*     void texProcess_Exited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(
            delegate()
            {
                //call OnTexOutput once more to make sure line de-breaking buffer is processed.
                Dispatcher.Invoke(
                    new Action(
                        delegate()
                        {
                            if (OnTexOutput != null)
                                OnTexOutput("");
                        }
                    )
                );


                if (texProcess.ExitCode == 0)
                {
                    currentBB = compilingBB;
                    RefreshPDF(Consts.cTempFile + ".pdf");
                    //string texout = texProcess.StandardOutput.ReadToEnd();
                    OnCompileEvent("Compilation done", CompileEventType.Success);
                }
                else
                {
                    //string err = texProcess.StandardOutput.ReadToEnd();
                    OnCompileEvent("Compilation failed wih exit code " + texProcess.ExitCode, CompileEventType.Error);
                }
                
                isRunning = false;
                SetValue(CompilingProperty, false);
                
                // compile pending requests
                //if (nextToCompile != "")
                //    doCompile();


                //this is bad. lines that are still "on its way" will be discarded.
                //cf. example on http://msdn.microsoft.com/de-de/library/system.diagnostics.process.beginoutputreadline(VS.80).aspx
                //there is no CancelOutputRead() call.
                ///texProcess.CancelOutputRead();
            }
            ));
        }*/


        /// <summary>
        /// Size is given by resolution * bounding box, if present.
        /// </summary>
        void RecalcSize()
        {
            if (currentBB.Width * currentBB.Height > 0)
            {
                Width = currentBB.Width * Resolution;
                if (Width < 5)  // a bit hacky
                    Width = 5;
                Height = currentBB.Height * Resolution;
                if (Height < 5)
                    Height = 5;
            }
            else
            {
                Width = double.NaN; // auto height/width
                Height = double.NaN;
            }

            RedrawBMP();
        }

        /// <summary>
        /// This method draws the currently loaded Pdf into a bitmap, and displays this bitmap in the image control.
        /// It is called, e.g., when the size of the TikzDisplay control changes
        /// Warning: It does _not_ reload the Pdf. 
        /// </summary>
        void RedrawBMP()
        {
            
            if (myPdfBmpDoc != null)
            {
                
                //check version of PDFLibNet, if the old 1.0.6.6 use old GetBitmap
                System.Reflection.Assembly a = System.Reflection.Assembly.GetAssembly(typeof(PDFWrapper));
                if(a.GetName().Version.Major == 1)
                    if(a.GetName().Version.Minor == 0)
                        if(a.GetName().Version.Build == 6)
                            if (a.GetName().Version.Revision == 6)
                            {
                                BitmapSource bitmap = myPdfBmpDoc.GetBitmap(Resolution, currentBB.Width * currentBB.Height > 0); // mypdfDoc.GetBitmap(currentBB, Resolution);                
                                if (bitmap != null)
                                  image1.Source = bitmap;
                                return;
                            }
                 
                //otherwise the new Bitmap2 function
                
                image1.Source = null;
                //myPdfBmpDoc.GetBitmap2(Resolution, currentBB.Width * currentBB.Height > 0); ;
                image1.Source = myPdfBmpDoc.GetBitmap2(Resolution, currentBB.Width * currentBB.Height > 0); ;

                /*GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();*/
            }

        }

        private void image1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //RedrawBMP();
        }

    }



    public class PdfToBmp
    {
        PDFWrapper mypdfDoc;
        

        public bool LoadPdf(string cfile)
        {

            if (mypdfDoc != null)
            {
                mypdfDoc.Dispose();
                mypdfDoc = null;
                
            }
            mypdfDoc = new PDFLibNet.PDFWrapper();
            
            mypdfDoc.UseMuPDF = true;

            if (!File.Exists(cfile))
                return false;
            //this line creates a handle
            //it can be closed with Dispose()
             bool ret = mypdfDoc.LoadPDF(cfile);             
             return ret;
        }

        public bool UnloadPdf()
        {
            if (mypdfDoc != null)
            {
                mypdfDoc.Dispose();
                mypdfDoc = null;
                return true;
            }
            return false;
        }

        /*public BitmapSource GetBitmap(Rect r, double Resolution)
        {
            if (mypdfDoc != null)
            {
                if (r.Width <= 0 || r.Height <= 0) // TODO: this should not be necessary
                    return null;
                Bitmap tmp = mypdfDoc.Pages[1].GetBitmap(72 * Resolution / Consts.ptspertikzunit);
                tmp.Save("temptem.bmp");
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, tmp.Height - Convert.ToInt32(r.Height * Resolution), Convert.ToInt32(r.Width * Resolution), Convert.ToInt32(r.Height * Resolution));
                Bitmap b = tmp.Clone(rect, tmp.PixelFormat);
                b.Save("temptemp.bmp");
                
                b.MakeTransparent(System.Drawing.Color.White);
                b.MakeTransparent(System.Drawing.Color.FromArgb(255, 253, 253, 253));
                b.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 254, 254));
                BitmapSource ret = loadBitmap(b);
                //b.Save("temptemp.bmp");
                b.Dispose();
                tmp.Dispose();
                return ret;
                //GC.Collect();
            }
            else return null;
        }*/
        #region TEST
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
        #endregion TEST

        public BitmapSource GetBitmap2(double Resolution, bool Transparent = true)
        {
            if (mypdfDoc != null && mypdfDoc.PageCount > 0)
            {
                mypdfDoc.RenderDPI = 72 * Resolution / Consts.ptspertikzunit;

                System.Windows.Forms.PictureBox pic = new System.Windows.Forms.PictureBox();
                mypdfDoc.CurrentPage = 1;
                mypdfDoc.RenderPage(pic.Handle);                
                

                /*Added since 1.0.6.2*/                                
                mypdfDoc.CurrentX = 0;
                mypdfDoc.CurrentY = 0;
                mypdfDoc.ClientBounds = new Rectangle(0, 0, mypdfDoc.PageWidth, mypdfDoc.PageHeight);

                if (mypdfDoc.PageWidth * mypdfDoc.PageHeight == 0)
                    return null;
                Bitmap _backbuffer = new Bitmap(mypdfDoc.PageWidth, mypdfDoc.PageHeight);
                using (Graphics g = Graphics.FromImage(_backbuffer))
                {
                    /*New thread safe method*/
                    mypdfDoc.DrawPageHDC(g.GetHdc());
                    g.ReleaseHdc();
                }
                pic.Dispose();
                
                if (Transparent)
                {
                    _backbuffer.MakeTransparent(System.Drawing.Color.White);
                    _backbuffer.MakeTransparent(System.Drawing.Color.FromArgb(255, 253, 253, 253));
                    _backbuffer.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 254, 254));
                }


                BitmapSource ret = loadBitmap(_backbuffer);
                _backbuffer.Dispose();                
                
                return ret;
            }
            else return null;
        }

        public BitmapSource getBitmapSourceFromBitmap(Bitmap bmp)
        {

            BitmapSource returnSource = null;

            try
            {

                returnSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(),

                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            }

            catch { returnSource = null; }

            return returnSource;

        }

        public BitmapSource GetBitmap(double Resolution, bool Transparent = true)
        {            
            if (mypdfDoc != null && mypdfDoc.PageCount >0)
            {

                Bitmap b;
                try
                {
                    //after this line pdf handle cannot be release with mypdfDoc.UnloadPdf();
                    //????;
                    b = mypdfDoc.Pages[1].GetBitmap(72 * Resolution / Consts.ptspertikzunit);                    
                    
                }
                catch (ArgumentException)
                {//this can happen if node position is very "big", like (500,500)
                    return null;
                }

                BitmapSource ret = null;

                if (b != null)
                {
                    if (Transparent)
                    {
                        b.MakeTransparent(System.Drawing.Color.White);
                        b.MakeTransparent(System.Drawing.Color.FromArgb(255, 253, 253, 253));
                        b.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 254, 254));
                    }
                    ret = loadBitmap(b);
                    b.Dispose();
                }
                else
                    b = b;
                
                return ret;                
            }
            else return null;
        }
        public bool IsEmpty()
        {
            if (mypdfDoc != null && mypdfDoc.Pages.Count == 0)
                return true;
            return false;
        }
        public void SaveBmp(string cFile, double Resolution)
        {
            if (mypdfDoc != null && mypdfDoc.Pages.Count > 0)
            {
                Bitmap b = mypdfDoc.Pages[1].GetBitmap(72 * Resolution / Consts.ptspertikzunit);
                b.MakeTransparent(System.Drawing.Color.White);                
                b.MakeTransparent(System.Drawing.Color.FromArgb(255, 253, 253, 253));
                b.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 254, 254));
                b.Save(cFile);
                b.Dispose();
                //GC.Collect();
            }            
        }


        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// This method converts a System.Drawing.Bitmap to a WPF Bitmap.
        /// (This is necessary since the WPF Image control only accepts WPF bitmaps)
        /// </summary>
        /// <param name="source">The System.Drawing.Bitmap</param>
        /// <returns>The same Bitmap, in Wpf format</returns>
        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {

            IntPtr ip = source.GetHbitmap();
            BitmapSource bs;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {                
                DeleteObject(ip);
            }

            return bs;
        }

        ~PdfToBmp()
        {
            if (mypdfDoc != null)
            {
                mypdfDoc.Dispose();
                mypdfDoc = null;
            }
        }
    }
}
