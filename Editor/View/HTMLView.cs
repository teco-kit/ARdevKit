using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARdevKit.View
{
    public class HTMLView : WebBrowser
    {
        public HTMLView()
        {
        }
        //new Eventhandler to trigger Events through calls from other classes
        public void triggerClick(object sender, EventArgs e)
        {
            this.OnClick(e);
        }
        public void triggerDoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(e);
        }
       
        //public void triggerMouseEnter(object sender, EventArgs e)
        //{
        //    base.OnMouseEnter(e);
        //}
        ////new Eventhandler to trigger MouseEvents through calls from other classes
        //public void triggerMouseLeave(object sender, EventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //}
        public void triggerMouseClick(object sender, MouseEventArgs e)
        {
            this.OnMouseClick(e);
        }
        public void triggerMouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.OnMouseDoubleClick(e);
        }
        public void triggerMouseMove(object sender, MouseEventArgs e)
        {
            this.OnMouseMove(e);
        }
        public void triggerMouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }
        public void triggerMouseUp(object sender, MouseEventArgs e)
        {
            this.OnMouseUp(e);
        }
    }
}
