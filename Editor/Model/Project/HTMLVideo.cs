using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project
{
    public class HtmlVideo : AbstractHtmlElement
    {
        public override System.Drawing.Bitmap getPreview(string projectPath)
        {
            throw new NotImplementedException();
        }

        public override System.Drawing.Bitmap getIcon()
        {
            return ARdevKit.Properties.Resources.HtmlVideoAugmention_small_;
        }

        public override void CleanUp()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
