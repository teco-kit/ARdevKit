using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ARdevKit.Controller.Connections;
using System.Text.RegularExpressions;

namespace ARdevKit.Controller.EditorController
{
    /// <summary>
    /// enables manipulation of the actual HTMLFile written Text which can be shown by the Webbrowser
    /// </summary>
    internal class WebSiteHTMLManager : HttpServer
    {
        /// <summary>
        /// A list of all Bitmaps, which are included in the Websites
        /// </summary>
        public List<System.Drawing.Bitmap> previews; 
        /// <summary>
        /// The website texts
        /// </summary>
        private string[] websiteTexts;
        /// <summary>
        /// The main container width
        /// </summary>
        private uint mainContainerWidth;
        /// <summary>
        /// The main container heigth
        /// </summary>
        private uint mainContainerHeigth;
        /// <summary>
        /// The containment wrapper
        /// </summary>
        private readonly Regex containmentWrapper = new Regex(@"<div id=""containment-wrapper""[^>]*>");

        /// <summary>
        /// Gets the width of the main container.
        /// </summary>
        /// <value>
        /// The width of the main container.
        /// </value>
        public uint MainContainerWidth 
        { 
            get { return mainContainerWidth; }
        }
        /// <summary>
        /// Gets the main container heigth.
        /// </summary>
        /// <value>
        /// The main container heigth.
        /// </value>
        public uint MainContainerHeigth
        {
            get { return mainContainerHeigth; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSiteHTMLManager"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public WebSiteHTMLManager(int port) : base(port)
        {
            websiteTexts = new string[10];
            for (int i = 0; i < 9; i++)
			{
			    websiteTexts[i] = ARdevKit.Properties.Resources.HTMLPreviewPage;
			}
            previews = new List<System.Drawing.Bitmap>();
        }
        /// <summary>
        /// builds a html document or loads the existing one at the 
        /// <para>websitePath</para> and constructs
        /// the according htmlManager
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="port">The port.</param>
        public WebSiteHTMLManager(string projectPath, int port) : base(port)
        {
            int i = 0;
            while (File.Exists(projectPath + "htmlWebsites/" + i + ".html"))
            {
                websiteTexts[i] = File.ReadAllText(projectPath);
                ++i;
            }
            previews = new List<System.Drawing.Bitmap>();
        }

        /// <summary>
        /// adds the HTMLText of the given <para>element</para> to the containment-wrapper
        /// </summary>
        /// <param name="element">the HtmlElement that should be added</param>
        /// <param name="index">The index.</param>
        public void addElementAt(HtmlElement element, int index)
        {
            string conWrap = containmentWrapper.Match(websiteTexts[index]).Value;
            string[] splittedPage = containmentWrapper.Split(websiteTexts[index]);
            websiteTexts[index] = splittedPage[0] +conWrap + element.OuterHtml + splittedPage[1];
        }

        /// <summary>
        /// removes the HTMLText of the given <para>element</para> from the htmlDoc
        /// </summary>
        /// <param name="element">the HtmlElement that should be removed</param>
        /// <param name="index">The index.</param>
        public void removeElementAt(HtmlElement element, int index)
        {
            websiteTexts[index] = websiteTexts[index].Replace(element.OuterHtml, "");
        }

        /// <summary>
        /// Replaces the <see cref="oldElement"/> with the <see cref="newElement"/> at <see cref="index"/>.
        /// </summary>
        /// <param name="oldElement">The old element.</param>
        /// <param name="newElement">The new element.</param>
        /// <param name="index">The index.</param>
        public void replaceElementsAt(HtmlElement oldElement, HtmlElement newElement, int index)
        {
            websiteTexts[index] = websiteTexts[index].Replace(oldElement.OuterHtml, newElement.OuterHtml);
        }

        /// <summary>
        /// Changes the size of the main container.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void changeMainContainerSize(uint width, uint height)
        {
            for (int i = 0; i < 9; i++)
            {
                websiteTexts[i] = containmentWrapper.Replace(websiteTexts[i], 
                    String.Format(@"<div id=""containment-wrapper"" title=""containment-wrapper"" style = "" 
                    width: {0}px; height: {1}px; margin-left: -{2}px; margin-top: -{3}px"" >", 
                    width, height, width / 2, height / 2));
            }  
        }

        /// <summary>
        /// removes the word "selected" from within the MainContainer
        /// </summary>
        /// <param name="index">index of the Page, which should be affected</param>
        public void deleteSelection(int index)
        {
            string conWrap = containmentWrapper.Match(websiteTexts[index]).Value;
            string[] splittedPage = containmentWrapper.Split(websiteTexts[index]);
            websiteTexts[index] = splittedPage[0] + conWrap + splittedPage[1].Replace("selected", "");
        }

        /// <summary>
        /// overrides the chosen page with the default PreviewPage
        /// </summary>
        /// <param name="index">specifies which Website is affected</param>
        public void resetPage(int index)
        {
            string conWrap = containmentWrapper.Match(websiteTexts[index]).Value;
            string[] splittedPage = containmentWrapper.Split(websiteTexts[index]);
            websiteTexts[index] = splittedPage[0] + conWrap + "</div>\n</body>\n</html>";
        }

        /// <summary>
        /// receives the Request of the WebBrowser which shows the Preview of the scene,
        /// the index of the scene is indicated by a 0 to 9 number seperated with a "/" after the
        /// domain name. this index is set as the index of the scene which is teargeted by
        /// all further manipulation, eg. 
        /// <see cref="removeElement" /> or 
        /// <see cref="addElement" />
        /// </summary>
        /// <param name="p">the HttpProcessor which receives the Requests</param>
        public override void handleGETRequest(HttpProcessor p)
        {
            try
            {
                string extension = p.http_url.Substring(1);
                foreach (var bitmap in previews)
                {
                    if ((string)bitmap.Tag == extension)
                    {
                        System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
                        byte[] toWrite = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
		                p.writeSuccess("image/bmp");
                        p.outputStream.BaseStream.Write(toWrite, 0, toWrite.Length);
                        return;
                    }
		 
                }
                int index = Convert.ToInt32(extension);
                if (index > 9 || index < 0)
                {
                    p.writeFailure();
                }
                else
                {
                    p.writeSuccess();
                    p.outputStream.Write(websiteTexts[index]);
                }
            }
            catch (System.FormatException e)
            {
                p.writeFailure();
            }   
        }

        /// <summary>
        /// this server does not support post requests
        /// </summary>
        /// <param name="p">The HttpProcessor.</param>
        /// <param name="inputData">The input data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            throw new NotImplementedException();
        }
    }
}
