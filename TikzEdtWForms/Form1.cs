﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TikzEdt.ViewModels;
using TikzEdt;
using System.Diagnostics;

namespace TikzEdtWForms
{
    public partial class Form1 : Form
    {
        MainWindowVM<TextEditorDocumentWrapper> TheVM = new MainWindowVM<TextEditorDocumentWrapper>(TheCompiler.Instance);

        RasterControl rasterControl1;
        SnippetList snippetList1;
        //PdfOverlay pdfOverlay1;

        public Form1()
        {
            (GlobalUI.UI as GlobalUIWinForms).MainForm = this;

            InitializeComponent();

            if (DesignMode)
                return;

            TextEditorDocumentWrapper.TheOneAndOnly = txtCode;
            
            // The order should be exactly the same as that in the OverlayToolType enum!!!
            ToolButtons = new List<ToolStripButton> { cmdMove, cmdNode, cmdEdge, cmdPath, cmdSmoothCurve, cmdBezier, cmdRectangle, cmdEllipse, cmdGrid, cmdArc, cmdArcEdit };
            ToolPaneButtons = new List<ToolStripButton> { cmdSnippets, cmdFiles, cmdDynPreamble };

            splitContainer2.Panel2.BackColor = Color.DarkGray;
            rasterControl1 = new RasterControl();
            splitContainer2.Panel2.Controls.Add(rasterControl1);
      //      tikzDisplay1.Visible = false;
     //       tikzDisplay1.Parent = rasterControl1;


       /*     pdfOverlay1 = new PdfOverlay();
            pdfOverlay1.Left = 0;
            pdfOverlay1.Top = 0;
            pdfOverlay1.Parent = tikzDisplay1;
            pdfOverlay1.BringToFront();
            pdfOverlay1.Visible = true; */
            //pdfOverlay1.Rasterizer = rasterControl1.TheRasterModel;
            rasterControl1.Rasterizer = rasterControl1.TheRasterModel;

            rasterControl1.Visible = true;
            //rasterControl1.Anchor = AnchorStyles.None;
            //rasterControl1.OverrideWithZeroGridWidth = true;
            //rasterControl1.Left = 0;
            //rasterControl1.Top = 0;

            splitContainer2.Panel2.Resize += new EventHandler(Panel2_Resize);
            rasterControl1.Resize += new EventHandler(Panel2_Resize);
      //      tikzDisplay1.Resize += new EventHandler(Panel2_Resize);

            cmbEdgeStyle.TextChanged += (s, e) => TheVM.TheDocument.EdgeStyle = cmbEdgeStyle.Text;
            cmbNodeStyle.TextChanged += (s, e) => TheVM.TheDocument.NodeStyle = cmbNodeStyle.Text;
            cmbEdgeStyle.DropDown += (s, e) => { cmbEdgeStyle.Items.Clear(); cmbEdgeStyle.Items.AddRange(TheVM.TheDocument.TikzStyles.ToArray()); };
            cmbNodeStyle.DropDown += (s, e) => { cmbNodeStyle.Items.Clear(); cmbNodeStyle.Items.AddRange(TheVM.TheDocument.TikzStyles.ToArray()); };

            snippetList1 = new SnippetList();
            snippetList1.Parent = splitContainer1.Panel1;
            snippetList1.Visible = true;
            snippetList1.Dock = DockStyle.Fill;
            snippetList1.OnInsert += new EventHandler<TikzEdt.Snippets.InsertEventArgs>(snippetList1_OnInsert);
            snippetList1.OnUseStyles += new EventHandler<TikzEdt.Snippets.UseStylesEventArgs>(snippetList1_OnUseStyles);



            //TheVM.NewCommandHandler(this, new System.Windows.Input.ExecutedRoutedEventArgs()) ;
            TheVM.CreateNewFile(false);
            
            // add bindings
            var bs = new BindingSource(TheVM, null);
            Binding b = new Binding("Document", bs, "TheDocument.Document", true, DataSourceUpdateMode.Never);
            b.Format += new ConvertEventHandler((s,e) => 
            {
                TextEditorDocumentWrapper W = e.Value as TextEditorDocumentWrapper;
                if (W == null) e.Value = null;
                else e.Value = W.Document;
            });
      //      txtCode.DataBindings.Add(b);
            txtCode.Document = TheVM.TheDocument.Document.Document;

            b = new Binding("ReloadPdf", bs, "TheDocument.ReloadPdf", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);
            //TheVM.TheDocument.
            //tikzDisplay1.Bou
            b = new Binding("PdfPath", bs, "TheDocument.PdfPath", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);
        //    b = new Binding("Resolution", bs, "TheDocument.Resolution", false, DataSourceUpdateMode.Never);
       //     tikzDisplay1.DataBindings.Add(b);

            
            b = new Binding("Resolution", bs, "TheDocument.Resolution", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);
            b = new Binding("BB", bs, "TheDocument.CurrentBB", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);

        //    b = new Binding("BB", bs, "TheDocument.CurrentBB", false, DataSourceUpdateMode.Never);
        //    pdfOverlay1.DataBindings.Add(b);
            b = new Binding("Tool", bs, "CurrentTool", false, DataSourceUpdateMode.OnPropertyChanged);
            rasterControl1.DataBindings.Add(b);
     //       b = new Binding("Resolution", bs, "TheDocument.Resolution", false, DataSourceUpdateMode.Never);
      //      pdfOverlay1.DataBindings.Add(b);
            b = new Binding("AllowEditing", bs, "TheDocument.AllowEditing", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);

            b = new Binding("EdgeStyle", bs, "TheDocument.EdgeStyle", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);
            b = new Binding("NodeStyle", bs, "TheDocument.NodeStyle", false, DataSourceUpdateMode.Never);
            rasterControl1.DataBindings.Add(b);

      //      b = new Binding("Text", bs, "TheDocument.EdgeStyle", false, DataSourceUpdateMode.Never);
      //      cmbEdgeStyle.DataBindings.Add(b);
      //      b = new Binding("Text", bs, "TheDocument.NodeStyle", false, DataSourceUpdateMode.Never);
      //      cmbNodeStyle.DataBindings.Add(b);

            //b = new Binding("ParseTree", bs, "TheDocument.ParseTree", false, DataSourceUpdateMode.Never);
            //pdfOverlay1.DataBindings.Add(b);
            //TheVM.TheDocument.Parse
            //pdfOverlay1.Pars

            var sp = BindingFactory.CreateProvider(TheVM, "TheDocument", vm => vm.TheDocument);
            BindingFactory.CreateBindingSP(sp, "ParseTree", doc => rasterControl1.ParseTree = doc.ParseTree, () => rasterControl1.ParseTree = null);
            var errlistsp = BindingFactory.CreateProviderSP(sp, "TexErrors", doc => doc.TexErrors);
            BindingFactory.CreateCollectionBindingSP(errlistsp, (s, e) => FillErrorsList());

            BindingFactory.CreateBinding(TheVM, "CurrentTool",
                vm =>
                {
                    ToolButtons.Each((tsb, i) => tsb.Checked = ((int)vm.CurrentTool == i));
                }, null);


            
/* * 
 * */
            txtCode.SetHighlighting("Tikz");
            //txtCode.
        }

        void snippetList1_OnUseStyles(object sender, TikzEdt.Snippets.UseStylesEventArgs e)
        {
            TheVM.TheDocument.InsertUseTikzLibrary(e.dependencies);

            if (!String.IsNullOrWhiteSpace(cmbEdgeStyle.Text) && e.InAddition)
                cmbEdgeStyle.Text = Helper.MergeStyles(cmbEdgeStyle.Text, e.edgestyle);
            else
                cmbEdgeStyle.Text = e.edgestyle;

            if (!String.IsNullOrWhiteSpace(cmbNodeStyle.Text) && e.InAddition)
                cmbNodeStyle.Text = Helper.MergeStyles(cmbNodeStyle.Text, e.nodestyle);
            else
                cmbNodeStyle.Text = e.nodestyle;
        }

        void snippetList1_OnInsert(object sender, TikzEdt.Snippets.InsertEventArgs e)
        {
            txtCode.Document.Insert(txtCode.ActiveTextAreaControl.Caret.Offset, e.code);
        }

        //List<object> MyBindings = new List<object>();

        void Panel2_Resize(object sender, EventArgs e)
        {
            //rasterControl1.Left = (splitContainer2.Panel2.ClientSize.Width - rasterControl1.Width) / 2;
            //rasterControl1.Top = (splitContainer2.Panel2.Height - rasterControl1.Height) / 2;
            //tikzDisplay1.Left = (rasterControl1.ClientSize.Width - tikzDisplay1.Width) / 2;
            //tikzDisplay1.Top = (rasterControl1.ClientSize.Height - tikzDisplay1.Height) / 2;

            rasterControl1.Left = (splitContainer2.Panel2.ClientSize.Width - rasterControl1.Width) / 2;
            rasterControl1.Top = (splitContainer2.Panel2.ClientSize.Height - rasterControl1.Height) / 2;


        }

        void Instance_OnCompileEvent(object sender, TexCompiler.CompileEventArgs e)
        {
            AddStatusLine(e.Message, e.Type == TexCompiler.CompileEventType.Error);
            if (e.Type == TexCompiler.CompileEventType.Start)
                txtTexOutput.Clear();
        }

        void AddStatusLine(string StatusLine, bool IsError)
        {
            this.BeginInvoke(new InvokeDelegate(() =>
            {
                txtStatus.SelectionLength = 0;
                txtStatus.SelectionColor = IsError ? Color.Red : Color.Black;
                txtStatus.AppendText(StatusLine + Environment.NewLine);
                txtStatus.SelectionStart = txtStatus.Text.Length - 1;
                txtStatus.ScrollToCaret();
            }));
        }

        void Instance_OnTexOutput(object sender, TexCompiler.CompileEventArgs e)
        {
            //this.BeginInvoke(new InvokeDelegate(() =>
            //{
                txtTexOutput.AppendText(e.Message + Environment.NewLine);
            //}));
        }
        public delegate void InvokeDelegate();
        void UI_OnGlobalStatus(object sender, GlobalStatusEventData e)
        {
            AddStatusLine(e.StatusLine, e.IsError);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            GlobalUI.UI.OnGlobalStatus += new EventHandler<GlobalStatusEventData>(UI_OnGlobalStatus);
            TheCompiler.Instance.OnTexOutput += new EventHandler<TexCompiler.CompileEventArgs>(Instance_OnTexOutput);
            TheCompiler.Instance.OnCompileEvent += new EventHandler<TexCompiler.CompileEventArgs>(Instance_OnCompileEvent);

            //txtCode.SetHighlighting("C#");
            //ICSharpCode.TextEditor.Document.IDocument doc;
            //doc.
            //ICSharpCode.TextEditor.Document.DocumentFactory df = new ICSharpCode.TextEditor.Document.DocumentFactory();
            //ICSharpCode.TextEditor.Document.IDocument doc =  df.CreateDocument();
            //doc.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy();

            //txtCode.Document = doc;
            
            //doc.Insert(0, "gjhghjgjhG");
            //doc.TextContentChanged += new EventHandler(doc_TextContentChanged);
            //doc.DocumentChanged += new ICSharpCode.TextEditor.Document.DocumentEventHandler(doc_DocumentChanged);
            
            //txtCode.Refresh();
            //txtCode.LoadFile(
        }

        void doc_DocumentChanged(object sender, ICSharpCode.TextEditor.Document.DocumentEventArgs e)
        {
            GlobalUI.UI.AddStatusLine(this, "changed");
        }

        void doc_TextContentChanged(object sender, EventArgs e)
        {
            GlobalUI.UI.AddStatusLine(this, "changed");
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            MessageBox.Show(TheVM.TheDocument.Document.Text);
        }

        private void recompileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheVM.TheDocument.Recompile();
        }

        List<ToolStripButton> ToolButtons;
        private void cmdToolClick_Click(object sender, EventArgs e)
        {
            OverlayToolType t = (OverlayToolType)ToolButtons.IndexOf(sender as ToolStripButton);
            TheVM.CurrentTool = t;
        }

        private void cmdSavePdf_Click(object sender, EventArgs e)
        {
            TheVM.TheDocument.SavePdf(false);
        }

        private void cmdCompile_Click(object sender, EventArgs e)
        {
            TheVM.TheDocument.Recompile();
        }

        private void cmdAbortCompile_Click(object sender, EventArgs e)
        {
            TheCompiler.Instance.AbortCompilation();
        }

        private void regeneratePrecompiledHeadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheCompiler.GeneratePrecompiledHeaders();
        }

        private void openInExternalViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string PdfPath = TheVM.TheDocument.SavePdf(false);

            if (PdfPath == "") return;

        //    if (Properties.Settings.Default.Path_externalviewer.Trim() == "")
            {
                System.Diagnostics.Process.Start(PdfPath);
            }
        //    else
            {
         //       System.Diagnostics.Process.Start(Properties.Settings.Default.Path_externalviewer, PdfPath);
            }
        }

        void FillErrorsList()
        {
            //return;
            lstErrors.Items.Clear();

            foreach (var err in TheVM.TheDocument.TexErrors)
            {
                ListViewItem lvi = new ListViewItem(err.Message);
                lvi.SubItems.Add(err.Line.ToString());
                lvi.SubItems.Add(err.Pos.ToString());
                lvi.SubItems.Add(err.causingSourceFile);

                switch (err.severity)
                {
                    case Severity.PARSERERROR: // todo: set status image
                    case Severity.ERROR:
                        lvi.ImageIndex = 0;
                        break;
                    case Severity.PARSERWARNING:
                    case Severity.WARNING:
                        lvi.ImageIndex = 1;
                        break;
                }

                lstErrors.Items.Add(lvi);
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            TheVM.CreateNewFile();
        }

        private void cmdNewInNew(object sender, EventArgs e)
        {
            TheVM.CreateNewFile(true);
        }

        private void cmdOpen_Click(object sender, EventArgs e)
        {
            TheVM.Open();
        }

        private void cmdOpenInNew_Click(object sender, EventArgs e)
        {
            TheVM.Open(true);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            TheVM.Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheVM.SaveAs();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        List<ToolStripButton> ToolPaneButtons;
        private void cmdSnippets_Click(object sender, EventArgs e)
        {
            foreach (var b in ToolPaneButtons)
            {
                if (b != sender)
                    b.Checked = false;
            }

            splitContainer1.Panel1Collapsed = ToolPaneButtons.TrueForAll(b => !b.Checked);

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            cmbNodeStyle.Text = "";
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            cmbEdgeStyle.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!DesignMode)
            {
                //ad// dockManager.SaveLayout(Helper.GetLayoutConfigFilepath());
                Properties.Settings.Default.Save();

                if (!TheVM.TheDocument.TryDisposeFile())
                    e.Cancel = true;
                else
                {
                    // Set closing flag
         //           isClosing = true;
                    //FindDialog.txtCode = null;
                    //     if (FindDialog != null)
                    //        FindDialog.Close();

                    // Exit running pdflatex instances
                    TheCompiler.Instance.AbortCompilation();
                    TikzToBMPFactory.Instance.AbortCompilation();
                }
            }
        }
    }
}
