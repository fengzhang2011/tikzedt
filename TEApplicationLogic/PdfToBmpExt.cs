﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace TikzEdt
{
    public interface IPdfToBmp<T>
    {
        T GetBitmap(double Resolution, bool Transparent = true);
        
        bool UnloadPdf();
        bool LoadPdf(string cfile);

    }

    public class PdfToBmpExtBase
    {
        protected string PdfFile;
        protected string PngFile;
        protected bool CallExternalRenderer(double Resolution, bool Transparent = true)
        {
            //try
            //{
            PngFile = PdfFile+".png";
            double dpi = 72 * Resolution / Consts.ptspertikzunit;
            //dpi = Math.Round(dpi, 10);
            string transp_arg = Transparent ? " -a" : "";
            var psi = new ProcessStartInfo()
                {
                    FileName = "mudraw.exe",
                    Arguments = "-o " + "\"" + PngFile + "\" -r " + dpi + transp_arg + " \"" + PdfFile + "\" 1",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            GlobalUI.UI.AddStatusLine(this, "Rendering: " + psi.FileName + " " + psi.Arguments);
            Process p = Process.Start(psi);

            p.WaitForExit();
            return (p.ExitCode == 0);
        }

        public bool  UnloadPdf()
        {
 	        return true;
        }

        public bool LoadPdf(string cfile)
        {
 	        PdfFile = cfile;
            return true;
        }
    }

    public class PdfToBmpExtWinForms : PdfToBmpExtBase, IPdfToBmp<Bitmap>
    {
        public Bitmap GetBitmap(double Resolution, bool Transparent = true)
        {
            if (CallExternalRenderer(Resolution, Transparent) && File.Exists(PngFile))
            {
                try
                {
                    Bitmap ret = null;
                    using (var fs = new FileStream(PngFile, FileMode.Open))
                    {
                        ret = (Bitmap)Image.FromStream(fs);
                    }
                    return ret;
                }
                catch (Exception ) {}
            }


            return null;
        }
    }

}
