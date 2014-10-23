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
        this.currentMetaCategory = new MetaCategory();
        this.index = 0;
        this.trackable = null;
        this.editorWindow.project.Trackables.Add(trackable);
        this.editorWindow.Tsm_editor_menu_edit_paste.Click += new System.EventHandler(this.paste_augmentation_center);
        this.editorWindow.Tsm_editor_menu_edit_copie.Click += new System.EventHandler(this.copy_augmentation);
        this.editorWindow.Tsm_editor_menu_edit_delete.Click += new System.EventHandler(this.delete_current_element);
    }

    #region Eventhandler
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
            center.Y = htmlPreview.Document.Body.ClientRectangle.Height / 2;
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
                this.findElement(currentElement).SetAttribute("z-index", Int16.MaxValue.ToString());
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

    private void setCoordinates(IPreviewable currentElement, Vector3D v)
    {
        throw new NotImplementedException();
    }

    private void addHTMLElement(IPreviewable currentElement, Vector3D center)
    {
        throw new NotImplementedException();
    }

    public void setCurrentElement(IPreviewable currentElement)
    {
        //if there is a currentElement we musst set the box of the actual currentElement to normal
        //the box of the new currentElement will be set to the new Borderstyle.
        if (currentElement != null)
        {
            if (this.editorWindow.CurrentElement != currentElement)
            {
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
                
                if (currentElement is AbstractSource)
                {
                    this.editorWindow.CurrentElement = ((AbstractSource)currentElement).Augmentation;
                }
                else
                {
                    this.editorWindow.CurrentElement = currentElement;
                }

                if (this.editorWindow.CurrentElement is AbstractAugmentation)
                {
                    this.editorWindow.Tsm_editor_menu_edit_copie.Enabled = true;
                }
                else if (this.editorWindow.CurrentElement is AbstractTrackable)
                {
                    this.editorWindow.Tsm_editor_menu_edit_copie.Enabled = false;
                }

                foreach (Control comp in this.panel.Controls)
                {
                    if (((PictureBox)comp).BorderStyle == BorderStyle.Fixed3D)
                    {
                        ((PictureBox)comp).BorderStyle = BorderStyle.None;
                        ((PictureBox)comp).Refresh();
                    }
                }
                findBox(this.editorWindow.CurrentElement).BorderStyle = BorderStyle.Fixed3D;
                findBox(this.editorWindow.CurrentElement).Refresh();
                if (typeof(AbstractAugmentation).IsAssignableFrom(this.editorWindow.CurrentElement.GetType()))
                {
                    findBox(this.editorWindow.CurrentElement).BringToFront();
                }
            }
            editorWindow.PropertyGrid1.SelectedObject = currentElement;
        }
        //if there is no currentElement we'll mark the box and set the currentElement in EditorWindow.
        else
        {
            this.editorWindow.CurrentElement = null;
            foreach (Control comp in this.panel.Controls)
            {
                if (((PictureBox)comp).BorderStyle == BorderStyle.Fixed3D)
                {
                    ((PictureBox)comp).BorderStyle = BorderStyle.None;
                    ((PictureBox)comp).Refresh();
                }
            }
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
        this.htmlPreview.DocumentText="";
        this.editorWindow.project.Trackables.Add(trackable);
        updateElementCombobox(trackable);
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