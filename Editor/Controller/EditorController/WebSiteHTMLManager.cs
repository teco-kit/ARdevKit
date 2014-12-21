using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ARdevKit.Controller.Connections;

namespace ARdevKit.Controller.EditorController
{
    /// <summary>
    /// enables manipulation of the actual HTMLFile written Text which can be shown by the Webbrowser
    /// </summary>
    internal class WebSiteHTMLManager : HttpServer
    {
        private string[] websiteTexts;
        int currentIndex;
        
        public WebSiteHTMLManager(int port) : base(port)
        {
            websiteTexts = new string[10];
            for (int i = 0; i < 9; i++)
			{
			    websiteTexts[0] = ARdevKit.Properties.Resources.HTMLPreviewPage;
			}
            currentIndex = 0;
        }
        /// <summary>
        /// builds a html document or loads the existing one at the <para>websitePath</para> and constructs 
        /// the according htmlManager 
        /// </summary>
        /// <param name="websitePath">describes where the htmlDoc is stored</param>
        public WebSiteHTMLManager(string projectPath, int port) : base(port)
        {
            int i = 0;
            while (File.Exists(projectPath + "htmlWebsites/" + i + ".html"))
            {
                websiteTexts[i] = File.ReadAllText(projectPath);
                ++i;
            }       
        }
 
        /// <summary>
        /// adds the HTMLText of the given <para>element</para> to the containment-wrapper
        /// </summary>
        /// <param name="element">the HtmlElement that should be added</param>
        public void addElement(HtmlElement element)
        {
            websiteTexts[currentIndex] = websiteTexts[currentIndex].Insert(websiteTexts[currentIndex].IndexOf("<div id=\"containment-wrapper\">") + 30, element.OuterHtml);
        }

        /// <summary>
        /// removes the HTMLText of the given <para>element</para> from the htmlDoc
        /// </summary>
        /// <param name="element">the HtmlElement that should be removed</param>
        public void removeElement(HtmlElement element)
        {
            websiteTexts[currentIndex] = websiteTexts[currentIndex].Remove(websiteTexts[currentIndex].IndexOf(element.OuterHtml), element.OuterHtml.Count());
        }

        /// <summary>
        /// receives the Request of the WebBrowser which shows the Preview of the scene,
        /// the index of the scene is indicated by a 0 to 9 number seperated with a "/" after the 
        /// domain name. this index is set as the index of the scene which is teargeted by
        /// all further manipulation, eg. <see cref="removeElement"/> or <see cref="addElement"/>
        /// </summary>
        /// <param name="p">the HttpProcessor which receives the Requests</param>
        public override void handleGETRequest(HttpProcessor p)
        {
            try
            {
                int index = Convert.ToInt32(p.http_url.Substring(1));
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
        /// <param name="p"></param>
        /// <param name="inputData"></param>
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            throw new NotImplementedException();
        }
    }
}
