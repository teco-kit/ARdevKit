using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARdevKit.Controller.EditorController
{
    /// <summary>
    /// enables manipulation of the actual HTMLFile written Text which can be shown by the Webbrowser
    /// </summary>
    internal class WebSiteHTMLManager
    {
        private string websitePath;
        private string websiteText;

        /// <summary>
        /// builds a html document at the <para>websitePath</para> and constructs 
        /// the according htmlManager 
        /// </summary>
        /// <param name="websitePath">describes where the htmlDoc is stored</param>
        public WebSiteHTMLManager(string websitePath)
        {
            buildNewWebsiteAt(websitePath);
        }

        /// <summary>
        /// builds a new Website from the template at the given <para>websitePath</para>
        /// </summary>
        /// <param name="websitePath">describes where the htmlDoc is stored</param>
        public void buildNewWebsiteAt(string websitePath)
        {
            websiteText = ARdevKit.Properties.Resources.HTMLPreviewPage;
            File.WriteAllText(websitePath, websiteText);
        }
 
        /// <summary>
        /// adds the HTMLText of the given <para>element</para> to the containment-wrapper
        /// </summary>
        /// <param name="element">the HtmlElement that should be added</param>
        public void addElement(HtmlElement element)
        {
            websiteText = websiteText.Insert(websiteText.IndexOf("<div id=\"containment-wrapper\">") + 30, element.OuterHtml);
            File.WriteAllText(websitePath, websiteText);
        }

        /// <summary>
        /// removes the HTMLText of the given <para>element</para> from the htmlDoc
        /// </summary>
        /// <param name="element">the HtmlElement that should be removed</param>
        public void removeElement(HtmlElement element)
        {
            websiteText = websiteText.Remove(websiteText.IndexOf(element.OuterHtml), element.OuterHtml.Count());
            File.WriteAllText(websitePath, websiteText);
        }
    }
}
