using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARdevKit.Model.Project;
using System.Collections;
using ARdevKit;
using ARdevKit.Controller.EditorController;
using ARdevKit.View;
using ARdevKit.Properties;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;

/// <summary>
/// The class PreviewController manages all things which are in contact with the PreviewhtmlView. Here are all methods, who influence the PreviewhtmlView.
/// </summary>
public class HTMLPreviewController
{
    #region static HTML expressions
    static string blank = "<HTML></HTML>";
    
    #endregion

    #region fields with getters and setters
    private EditorWindow editorWindow;
    
    private WebBrowser htmlPreview;

    private double scale;
    
    public AbstractTrackable trackable { get; set; }

    public MetaCategory currentMetaCategory { get; set; }

    public int index { get; set; }

    #endregion

    public HTMLPreviewController(EditorWindow editorWindow)
    {
        // TODO: Complete member initialization
        this.editorWindow = editorWindow;
        this.htmlPreview = this.editorWindow.HTMLPreview;
        this.htmlPreview.Url = new System.Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()) + "\\res\\HTMLPreview\\HTMLPreviewPage.html", System.UriKind.Absolute);
        //this.htmlPreview.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(htmlPreview_DocumentCompleted);
        //this.htmlPreview.Navigate(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\Resources\\HTMLPreviewPage.html"));
        //this.htmlPreview.Document.Write(ARdevKit.Properties.Resources.HTMLPreviewPage);
        //HtmlElement test = this.htmlPreview.Document.CreateElement("h1");
        //test.InnerText = "TESTESTESTESTEST";
        //this.htmlPreview.Document.Body.AppendChild(test);
        //this.htmlPreview.Navigate("about:blank");
        //HtmlDocument doc = this.htmlPreview.Document;
        //doc.Write(String.Empty);
        //this.htmlPreview.DocumentText = "";
        //htmlPreview.Url = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\Resources\\HTMLPreviewPage.html");
        //htmlPreview.Navigate(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "\\res\\HTMLPreview\\HTMLPreviewPage.html"));
        
        //HtmlElement page = htmlPreview.Document.CreateElement("div");
        //page.Id = "page";
        //htmlPreview.Document.Body.AppendChild(page);
        //HtmlElement conwrap = htmlPreview.Document.CreateElement("div");
        //conwrap.Id = "containment-wrapper";
        //htmlPreview.Document.Body.FirstChild.AppendChild(conwrap);
        this.currentMetaCategory = new MetaCategory();
        this.index = 0;
        this.trackable = null;
        this.editorWindow.project.Trackables.Add(trackable);
        this.editorWindow.Tsm_editor_menu_edit_paste.Click += new System.EventHandler(this.paste_augmentation_center);
        this.editorWindow.Tsm_editor_menu_edit_copie.Click += new System.EventHandler(this.copy_augmentation);
        this.editorWindow.Tsm_editor_menu_edit_delete.Click += new System.EventHandler(this.delete_current_element);    
    }

    #region Eventhandler
    private void htmlPreview_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
        HTMLView htmlPreview = sender as HTMLView;
    }
    private void delete_current_element(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void copy_augmentation(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void paste_augmentation_center(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void paste_augmentation(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    #endregion

    public void addPreviewable(IPreviewable currentElement, Vector3D v)
    {
        if (currentElement == null)
            throw new ArgumentException("parameter currentElement was null");
        if (v == null)
            throw new ArgumentException("parameter v was null");

        if (currentElement is AbstractTrackable && trackable == null)
        {
            this.editorWindow.Tsm_editor_menu_edit_delete.Enabled = true;
            Vector3D center = new Vector3D(0, 0, 0);
            center.Y = htmlPreview.Document.All["containment-wrapper"].OffsetRectangle.Height;
            center.X = htmlPreview.Document.Body.ClientRectangle.Width / 2;
            while (true)
            {
                //ask the user for the picture (if the trackable is a picturemarker)
                bool isInitOk = currentElement.initElement(editorWindow);
                if (!isInitOk)
                {
                    break;
                }
                if (isInitOk)
                {
                    //set the vector to the trackable
                    ((AbstractTrackable)currentElement).vector = center;

                    if (!this.editorWindow.project.existTrackable(currentElement))
                    {
                        //disables all trackable elements excepted the on that was added.
                        foreach (SceneElementCategory c in this.editorWindow.ElementCategories)
                        {
                            if (c.Name == "Trackables")
                            {
                                foreach (SceneElement e in c.SceneElements)
                                {
                                    this.editorWindow.ElementSelectionController.setElementEnable(e.Prototype.GetType(), false);
                                }
                            }
                        }
                        this.editorWindow.ElementSelectionController.setElementEnable(currentElement.GetType(), true);

                        this.trackable = (AbstractTrackable)currentElement;
                        this.editorWindow.project.Trackables[index] = (AbstractTrackable)currentElement;
                        this.addHTMLElement(currentElement, center);
                        setCurrentElement(currentElement);
                        editorWindow.PropertyGrid1.SelectedObject = currentElement;
                        break;
                    }
                }
            }
        }
        else if (currentElement is AbstractAugmentation && trackable != null && this.editorWindow.project.Trackables[index].Augmentations.Count < 3)
        {
            bool isInitOk = currentElement.initElement(editorWindow);
            if (isInitOk)
            {
                //set references 
                trackable.Augmentations.Add((AbstractAugmentation)currentElement);

                this.addHTMLElement(currentElement, v);

                //set the vector and the trackable in <see cref="AbstractAugmentation"/>
                this.setCoordinates(currentElement, v);
                ((AbstractAugmentation)currentElement).Trackable = this.trackable;

                //set the new box to the front
                //this.findElement(currentElement).SetAttribute("z-index", Int16.MaxValue.ToString());
                setCurrentElement(currentElement);
            }
        }
    }

    private HtmlElement findElement(IPreviewable prev)
    {
        if (prev == null) 
        {
            throw new ArgumentException("parameter prev was null.");
        }
        if (prev is Abstract2DTrackable)
        {
            return htmlPreview.Document.Body.Children[((Abstract2DTrackable)prev).SensorCosID];
        }
        if (prev is AbstractSource)
        {
            prev = ((AbstractSource)prev).Augmentation;
        }
        if (prev is AbstractAugmentation)
        {
            return htmlPreview.Document.Body.Children[((AbstractAugmentation)prev).ID];
        }
        return null;           
    }

    private void setCoordinates(IPreviewable prev, Vector3D newV)
    {
        throw new NotImplementedException();
        if (prev == null)
            throw new ArgumentException("parameter prev was null.");
        if (newV == null)
            throw new ArgumentException("parameter newV was null.");

        if (prev is Chart)
        {
            ((Chart)prev).Positioning.Left = (int)newV.X;
            ((Chart)prev).Positioning.Top = (int)newV.Y;

            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (int)((newV.X - Convert.ToInt32(htmlPreview.Document.GetElementById("containment-wrapper").GetAttribute("width")) / 2));
            result.Y = (int)((Convert.ToInt32(htmlPreview.Document.GetElementById("containment-wrapper").GetAttribute("height")) / 2 - newV.Y));
            ((AbstractAugmentation)prev).Translation = result;
        }
        else
        {
            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (int)((newV.X - editorWindow.project.Screensize.Width / 2) / scale / 1.6);
            result.Y = (int)((editorWindow.project.Screensize.Height / 2 - newV.Y) / scale / 1.6);
            ((AbstractAugmentation)prev).Translation = result;
        }
    }

    private void addHTMLElement(IPreviewable currentElement, Vector3D center)
    {
        if (currentElement is Abstract2DTrackable)
        {
            Abstract2DTrackable trackable = ((Abstract2DTrackable)currentElement);
            HtmlElement htmlTrackable = htmlPreview.Document.CreateElement("div");
            int height, width;

            htmlTrackable.Id = trackable.SensorCosID;
            htmlTrackable.SetAttribute("class", "trackable");
            if(trackable.Size == 0)
            {
                height = trackable.HeightMM;
                width = trackable.HeightMM;
            } else {
                height = width = trackable.Size;
            }
            htmlTrackable.Style = String.Format("background-size: 100% 100%; width: {0}; height: {1}; left: {3}; top: {4}; background-image:{5}; z-index: {6}",
            width, height, 
            editorWindow.project.Screensize.Width / 2, editorWindow.project.Screensize.Height / 2, 
            trackable.getPreview(editorWindow.project.ProjectPath), Int16.MinValue);
            htmlPreview.Document.GetElementById("containment-wrapper").AppendChild(htmlTrackable);
        }
        else if (currentElement is Chart)
        {
            Chart chart = ((Chart) currentElement);
            HtmlElement htmlChart = htmlPreview.Document.CreateElement("div");
            htmlChart.Id = chart.ID;
            htmlChart.SetAttribute("class", "ui-widget-content augmentation");
            htmlChart.Style = String.Format("width: {0}; height: {1}; left: {3}; top: {4}", chart.Width, chart.Height, chart.Positioning.Left, chart.Positioning.Top);
            htmlPreview.Document.GetElementById("containment-wrapper").AppendChild(htmlChart);
        }else if (currentElement is Abstract2DAugmentation)
        {
            Abstract2DAugmentation augmentation = ((Abstract2DAugmentation)currentElement);
            HtmlElement htmlAugmentation = htmlPreview.Document.CreateElement("div");
            Vector3D htmlCoordinate = nativeToHtmlCoordinates(augmentation.Translation);
            htmlAugmentation.Id = augmentation.ID;
            htmlAugmentation.SetAttribute("class", "ui-widget-content augmentation");
            htmlAugmentation.Style = String.Format("background-size: 100% 100%; width: {0}; height: {1}; left: {3}; top: {4}; background-image:{5}; z-index: {6}",
                augmentation.Width, augmentation.Height,
                htmlCoordinate.X, htmlCoordinate.Y,
                trackable.getPreview(editorWindow.project.ProjectPath), htmlCoordinate.Y);
            htmlPreview.Document.GetElementById("containment-wrapper").AppendChild(htmlAugmentation);
        } else
        {
            throw new NotSupportedException("Other then Abstract2DAugmention/Abstract2DTrackable not yet supported");
        }
    }

    private Vector3D nativeToHtmlCoordinates(Vector3D native)
    {
        if(native == null)
        {
            throw new ArgumentNullException();
        }
        Vector3D result = new Vector3D(0, 0, native.Z);
        result.X = (int)((native.X + editorWindow.project.Screensize.Width / 2));
        result.Y = (int)((editorWindow.project.Screensize.Height / 2 + native.Y));
        return result;
    }

    private Vector3D htmlToNativeCoordinates(Vector3D html)
    {
        if (html == null)
        {
            throw new ArgumentNullException();
        }
        Vector3D result = new Vector3D(0, 0, html.Z);
        result.X = (int)((html.X - editorWindow.project.Screensize.Width / 2));
        result.Y = (int)((editorWindow.project.Screensize.Height / 2 - html.Y));
        return result;
    }
    public void setCurrentElement(IPreviewable currentElement)
    {
        if (currentElement != null)
        {
            if (this.editorWindow.CurrentElement != currentElement)
            {
                //reset the z-index: property of the elements in the htmlPreview
                if (this.editorWindow.CurrentElement is AbstractAugmentation)
                {
                    findElement(this.editorWindow.CurrentElement).SetAttribute("z-index", ((AbstractAugmentation)this.editorWindow.CurrentElement).Translation.Z.ToString());
                }

                if (this.editorWindow.CurrentElement is AbstractTrackable)
                {
                    findElement(this.editorWindow.CurrentElement).SetAttribute("z-index", Int16.MinValue.ToString());
                }

                if (this.editorWindow.CurrentElement is AbstractSource)
                {
                    findElement(this.editorWindow.CurrentElement).SetAttribute("z-index", ((AbstractSource)this.editorWindow.CurrentElement).Augmentation.Translation.Z.ToString());
                }
                
                //reset the current element in the EditorWindow
                if (currentElement is AbstractSource)
                {
                    this.editorWindow.CurrentElement = ((AbstractSource)currentElement).Augmentation;
                }
                else
                {
                    this.editorWindow.CurrentElement = currentElement;
                }

                //enable the possibilty to copy the augmentation, and disable to copy a trackable 
                if (this.editorWindow.CurrentElement is AbstractAugmentation)
                {
                    this.editorWindow.Tsm_editor_menu_edit_copie.Enabled = true;
                }
                else if (this.editorWindow.CurrentElement is AbstractTrackable)
                {
                    this.editorWindow.Tsm_editor_menu_edit_copie.Enabled = false;
                }

                //foreach (Control comp in this.panel.Controls)
                //{
                //    if (((PictureBox)comp).BorderStyle == BorderStyle.Fixed3D)
                //    {
                //        ((PictureBox)comp).BorderStyle = BorderStyle.None;
                //        ((PictureBox)comp).Refresh();
                //    }
                //}
                //findBox(this.editorWindow.CurrentElement).BorderStyle = BorderStyle.Fixed3D;
                //findBox(this.editorWindow.CurrentElement).Refresh();
                //if (typeof(AbstractAugmentation).IsAssignableFrom(this.editorWindow.CurrentElement.GetType()))
                //{
                //    findBox(this.editorWindow.CurrentElement).BringToFront();
                //}
            }
            editorWindow.PropertyGrid1.SelectedObject = currentElement;
        }
        //if there is no currentElement we'll mark the box and set the currentElement in EditorWindow.
        else
        {
            this.editorWindow.CurrentElement = null;
            //foreach (Control comp in this.panel.Controls)
            //{
            //    if (((PictureBox)comp).BorderStyle == BorderStyle.Fixed3D)
            //    {
            //        ((PictureBox)comp).BorderStyle = BorderStyle.None;
            //        ((PictureBox)comp).Refresh();
            //    }
            //}
            this.editorWindow.Tsm_editor_menu_edit_copie.Enabled = false;
        }
        updateElementCombobox(trackable);
        editorWindow.Cmb_editor_properties_objectSelection.SelectedItem = currentElement;
        if (this.editorWindow.CurrentElement != null)
        {
            this.editorWindow.Tsm_editor_menu_edit_delete.Enabled = true;
        }
    }
    
    internal void updatePreviewPanel()
    {
        //throw new NotImplementedException();
        //this.htmlPreview.DocumentText="";
        //this.editorWindow.project.Trackables.Add(trackable);
        //updateElementCombobox(trackable);
        //ContextMenu cm = new ContextMenu();
        //cm.MenuItems.Add("einfügen", new EventHandler(this.paste_augmentation));
        //cm.MenuItems[0].Enabled = false;
        //this.htmlPreview.ContextMenu = cm;
    }


    private void updateElementCombobox(AbstractTrackable trackable)
    {
        //throw new NotImplementedException();
        if (trackable != null)
        {
            if (editorWindow.Cmb_editor_properties_objectSelection.Items.Count != 1 + trackable.Augmentations.Count + editorWindow.project.Sources.Count)
            {
                editorWindow.Cmb_editor_properties_objectSelection.Items.Clear();
                editorWindow.Cmb_editor_properties_objectSelection.Items.Add(trackable);
                foreach (AbstractAugmentation a in trackable.Augmentations)
                {
                    editorWindow.Cmb_editor_properties_objectSelection.Items.Add(a);
                }
                foreach (AbstractSource s in editorWindow.project.Sources)
                {
                    editorWindow.Cmb_editor_properties_objectSelection.Items.Add(s);
                }
            }
        }
        else
        {
            editorWindow.Cmb_editor_properties_objectSelection.Items.Clear();
        }
    }

    internal void reloadPreviewPanel(int index)
    {
        throw new NotImplementedException();
        if (index < this.editorWindow.project.Trackables.Count)
        {
            this.index = index;
            this.trackable = this.editorWindow.project.Trackables[index];

        }
    }

    private void buildPage(AbstractTrackable trackable)
    {
        throw new NotImplementedException();
        if(trackable.Augmentations.Count > 0)
        {
            foreach (AbstractAugmentation aug in trackable.Augmentations)
            {
                
            }
        }
    }

    internal void removePreviewable(AbstractTrackable abstractTrackable)
    {
        throw new NotImplementedException();
    }

    internal Image scaleBitmap(Bitmap bitmap, int p1, int p2)
    {
        throw new NotImplementedException();
    }

    internal void rescalePreviewPanel()
    {
        throw new NotImplementedException();
    }


}