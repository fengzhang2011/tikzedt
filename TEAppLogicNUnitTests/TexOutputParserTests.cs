﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TikzEdt;
using TikzEdt.ViewModels;
using NUnit.Framework;
using System.Threading;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows.Threading;

namespace TEAppLogicNUnitTests
{
    [TestFixture, RequiresSTA]
    class TexOutputParserTests
    {

        string TexOutput_NoError=
@"
This is pdfTeX, Version 3.1415926-2.3-1.40.12 (MiKTeX 2.9)
entering extended mode
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.tex
LaTeX2e <2011/06/27>
Babel <v3.8m> and hyphenation patterns for english, afrikaans, ancientgreek, arabic, armenian, assamese, basque, bengali, bokmal, bulgarian, catalan, coptic, croatian, czech, danish, dutch, esperanto, estonian, farsi, finnish, french, galician, german, german-x-2009-06-19, greek, gujarati, hindi, hungarian, icelandic, indonesian, interlingua, irish, italian, kannada, kurmanji, lao, latin, latvian, lithuanian, malayalam, marathi, mongolian, mongolianlmc, monogreek, ngerman, ngerman-x-2009-06-19, nynorsk, oriya, panjabi, pinyin, polish, portuguese, romanian, russian, sanskrit, serbian, slovak, slovenian, spanish, swedish, swissgerman, tamil, telugu, turkish, turkmen, ukenglish, ukrainian, uppersorbian, usenglishmax, welsh, loaded.
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.aux)
ABD: EveryShipout initializing macros
(""C:\Program Files (x86)\MiKTeX 2.9\tex\context\base\supp-pdf.mkii""
[Loading MPS to PDF converter (version 2006.09.02).]
)
*geometry* driver: auto-detecting
*geometry* detected driver: pdftex
Preview: Fontsize 10pt
Preview: PDFoutput 1
Preview: Tightpage -32891 -32891 32891 32891
[1] (C:\Users\thomas\AppData\Local\Temp\temp_preview10552.aux) )
Output written on temp_preview10552.pdf (1 page, 1253 bytes).
Transcript written on temp_preview10552.log.
";

        string TexOutput_Err1 = 
    @"
This is pdfTeX, Version 3.1415926-2.3-1.40.12 (MiKTeX 2.9)
entering extended mode
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.tex
LaTeX2e <2011/06/27>
Babel <v3.8m> and hyphenation patterns for english, afrikaans, ancientgreek, arabic, armenian, assamese, basque, bengali, bokmal, bulgarian, catalan, coptic, croatian, czech, danish, dutch, esperanto, estonian, farsi, finnish, french, galician, german, german-x-2009-06-19, greek, gujarati, hindi, hungarian, icelandic, indonesian, interlingua, irish, italian, kannada, kurmanji, lao, latin, latvian, lithuanian, malayalam, marathi, mongolian, mongolianlmc, monogreek, ngerman, ngerman-x-2009-06-19, nynorsk, oriya, panjabi, pinyin, polish, portuguese, romanian, russian, sanskrit, serbian, slovak, slovenian, spanish, swedish, swissgerman, tamil, telugu, turkish, turkmen, ukenglish, ukrainian, uppersorbian, usenglishmax, welsh, loaded.
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.aux)
ABD: EveryShipout initializing macros
(""C:\Program Files (x86)\MiKTeX 2.9\tex\context\base\supp-pdf.mkii""
[Loading MPS to PDF converter (version 2006.09.02).]
)
*geometry* driver: auto-detecting
*geometry* detected driver: pdftex
Preview: Fontsize 10pt
Preview: PDFoutput 1

! Package tikz Error: Giving up on this path. Did you forget a semicolon?.

See the tikz package documentation for explanation.
Type  H <return>  for immediate help.
 ...                                              
                                                  
l.7 \usetikzlibrary
                   {calc}
!  ==> Fatal error occurred, no output PDF file produced!
Transcript written on temp_preview10552.log.


";


        string TexOutput_Err2 =
@"This is pdfTeX, Version 3.1415926-2.3-1.40.12 (MiKTeX 2.9)
entering extended mode
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.tex
LaTeX2e <2011/06/27>
Babel <v3.8m> and hyphenation patterns for english, afrikaans, ancientgreek, arabic, armenian, assamese, basque, bengali, bokmal, bulgarian, catalan, coptic, croatian, czech, danish, dutch, esperanto, estonian, farsi, finnish, french, galician, german, german-x-2009-06-19, greek, gujarati, hindi, hungarian, icelandic, indonesian, interlingua, irish, italian, kannada, kurmanji, lao, latin, latvian, lithuanian, malayalam, marathi, mongolian, mongolianlmc, monogreek, ngerman, ngerman-x-2009-06-19, nynorsk, oriya, panjabi, pinyin, polish, portuguese, romanian, russian, sanskrit, serbian, slovak, slovenian, spanish, swedish, swissgerman, tamil, telugu, turkish, turkmen, ukenglish, ukrainian, uppersorbian, usenglishmax, welsh, loaded.
(C:\Users\thomas\AppData\Local\Temp\temp_preview10552.aux)
ABD: EveryShipout initializing macros
(""C:\Program Files (x86)\MiKTeX 2.9\tex\context\base\supp-pdf.mkii""
[Loading MPS to PDF converter (version 2006.09.02).]
)
*geometry* driver: auto-detecting
*geometry* detected driver: pdftex
Preview: Fontsize 10pt
Preview: PDFoutput 1
! Undefined control sequence.
l.9 \sdfsdf
           
!  ==> Fatal error occurred, no output PDF file produced!
Transcript written on temp_preview10552.log.
";

        [TestFixtureSetUp]
        public static void MyClassInitialize()
        {

        }

        TexOutputParser top;
        TexCompiler.Job job;
        List<TexOutputParser.TexError> errors;

        [SetUp]
        public void TestSetup()
        {
            top = new TexOutputParser();
            errors = new List<TexOutputParser.TexError>();
            job = new TexCompiler.Job();
            top.OnTexError += (s,e,t) => errors.Add(e);
        }

        [Test]
        public void parseOutputTest1()
        {
            top.addLine(TexOutput_NoError);
            bool ret = top.parseOutput(job);

            Assert.AreEqual(errors.Count, 0);            
        }

        [Test]
        public void parseOutputTest2()
        {
            top.addLine(TexOutput_Err1);
            bool ret = top.parseOutput(job);

            Assert.AreNotEqual(errors.Count, 0);
            Assert.AreEqual(errors[0].linenr, 7);
            Assert.IsTrue(errors[0].severity == Severity.ERROR);
        }

        [Test]
        public void parseOutputTest3()
        {
            top.addLine(TexOutput_Err2);
            bool ret = top.parseOutput(job);

            Assert.AreNotEqual(errors.Count, 0);
            Assert.AreEqual(errors[0].linenr, 9);
            Assert.IsTrue(errors[0].Message.Contains("control sequence"));
            Assert.IsTrue(errors[0].severity == Severity.ERROR);
        }

    }
}
