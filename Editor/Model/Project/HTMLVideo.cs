using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project
{
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class HtmlVideo : AbstractHtmlElement
    {
        public override System.Drawing.Bitmap getPreview(string projectPath)
        {
            return ARdevKit.Properties.Resources.HtmlVideoAugmention_small_;
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
            return ObjectCopier.Clone<HtmlVideo>(this);
        }
    }
}
