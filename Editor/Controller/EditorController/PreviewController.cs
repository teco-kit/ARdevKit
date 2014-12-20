﻿using System;
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
using ARdevKit.Model.Project.File;

namespace ARdevKit.Controller.EditorController
{
    /// <summary>
    /// The class PreviewController manages all things which are in contact with the PreviewPanel. Here are all methods, who influence the PreviewPanel.
    /// </summary>
    public class PreviewController
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The MetaCategory of the current element. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public MetaCategory currentMetaCategory { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The Trackable which hold the Augmentations and Sources. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public AbstractTrackable trackable { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The PreviewPanel which we need to add Previewables. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private WebBrowser htmlPreview;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   EditorWindow Instanz </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private EditorWindow ew;

        /// <summary>   The Index which Trackable out of Project we musst use </summary>
        public int index;

        /// <summary>
        /// The scale of the previewPanel. we need this to scale the distance and the size of the elements.
        /// </summary>
        private double scale;

        /// <summary>
        /// Gets or sets the copy.
        /// </summary>
        /// <value>
        /// The copy.
        /// </value>
        public AbstractAugmentation copy { get; set; }

        private List<WebSiteHTMLManager> Websites;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ew">EditorWindow Instanz.</param>
        /// <exception cref="System.ArgumentException">parameter ew was null.</exception>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public PreviewController(EditorWindow ew)
        {
            if (ew == null)
                throw new ArgumentException("parameter ew was null.");

            this.ew = ew;
            this.htmlPreview = this.ew.Html_preview;
            this.currentMetaCategory = new MetaCategory();
            this.index = 0;
            this.trackable = null;
            this.ew.project.Trackables.Add(trackable);
            this.Websites = new List<WebSiteHTMLManager>();

            this.ew.Tsm_editor_menu_edit_paste.Click += new System.EventHandler(this.paste_augmentation_center);
            this.ew.Tsm_editor_menu_edit_copie.Click += new System.EventHandler(this.copy_augmentation);
            this.ew.Tsm_editor_menu_edit_delete.Click += new System.EventHandler(this.delete_current_element);
        }



        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   (This method is obsolete) adds a preview able. </summary>
        ///
        /// <exception cref="NotImplementedException"> Thrown when the requested operation is
        /// unimplemented. </exception>
        ///
        /// <param name="p">    The Panel to process. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Obsolete("addPreviewable(IPreviewable p) : eache IPreviewable needs a Vector where the new"
                    + "Previewable should sit in the panel you should use addPreviewable(IPreviewable"
                    + "currentElement, Vector3d v) for Augmentations & Trackables", true)]
        public void addPreviewAble(IPreviewable p)
        { throw new NotImplementedException(); }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// add Trackable is the method for adding the trackable, each PreviewPanel can holding one
        /// Trackable.
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        /// <param name="v">The Vector3D to set the Trackable.</param>
        /// <exception cref="System.ArgumentException">
        /// parameter currentEelement was null
        /// or
        /// parameter v was null
        /// </exception>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void addPreviewable(IPreviewable currentElement, Vector3D v)
        {
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null");
            if (v == null)
                throw new ArgumentException("parameter v was null");

            if (currentElement is AbstractTrackable && trackable == null)
            {
                this.ew.Tsm_editor_menu_edit_delete.Enabled = true;
                Vector3D center = new Vector3D(0, 0, 0);
                center.Y = getMainContainerSize().Height / 2;
                center.X = getMainContainerSize().Width / 2;
                while (true)
                {
                    //ask the user for the picture (if the trackable is a picturemarker)
                    bool isInitOk = currentElement.initElement(ew);
                    if (!isInitOk)
                    {
                        break;
                    }
                    if (isInitOk)
                    {
                        //set the vector to the trackable
                        ((AbstractTrackable)currentElement).vector = center;

                        if (!this.ew.project.existTrackable(currentElement))
                        {
                            //diables all trackable elements excepted the on that was added.
                            foreach (SceneElementCategory c in this.ew.ElementCategories)
                            {
                                if (c.Name == "Trackables")
                                {
                                    foreach (SceneElement e in c.SceneElements)
                                    {
                                        this.ew.ElementSelectionController.setElementEnable(e.Prototype.GetType(), false);
                                    }
                                }
                            }
                            this.ew.ElementSelectionController.setElementEnable(currentElement.GetType(), true);

                            this.trackable = (AbstractTrackable)currentElement;
                            this.ew.project.Trackables[index] = (AbstractTrackable)currentElement;
                            this.addPictureBox(currentElement, center);
                            setCurrentElement(currentElement);
                            ew.PropertyGrid1.SelectedObject = currentElement;
                            break;
                        }
                    }
                }
            }
            else if (currentElement is AbstractAugmentation && trackable != null && this.ew.project.Trackables[index].Augmentations.Count < 3)
            {
                bool isInitOk = currentElement.initElement(ew);
                if (isInitOk)
                {
                    //set references 
                    trackable.Augmentations.Add((AbstractAugmentation)currentElement);

                    this.addPictureBox(currentElement, v);

                    //set the vector and the trackable in <see cref="AbstractAugmentation"/>
                    this.setCoordinates(currentElement, v);
                    ((AbstractAugmentation)currentElement).Trackable = this.trackable;

                    //set the new box to the front
                    this.findBox(currentElement).BringToFront();
                    setCurrentElement(currentElement);
                }
            }
        }

        /// <summary>
        /// add Source or augmentation, this method can only be used with the element, which is the
        /// over element by augmentation the overelement is Trackable. by Source the overelement is
        /// augmentation.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="currentElement">The current element.</param>
        /// <exception cref="System.ArgumentException">
        /// parameter source was null.
        /// or
        /// parameter currentElement was null.
        /// </exception>
        public void addSource(AbstractSource source, AbstractAugmentation currentElement)
        {
            if (source == null)
                throw new ArgumentException("parameter source was null.");
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            if (source != null && currentElement is AbstractDynamic2DAugmentation)
            {

                if (this.trackable != null && trackable.existAugmentation((AbstractAugmentation)currentElement)
                    && ((AbstractDynamic2DAugmentation)currentElement).Source == null)
                {
                    if (source is FileSource)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\highcharts";
                        openFileDialog.Title = "Daten auswählen";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            //set reference to the augmentations in Source
                            source.initElement(ew);
                            source.Augmentation = ((AbstractDynamic2DAugmentation)currentElement);

                            string newDataPath = Path.Combine(Environment.CurrentDirectory, "tmp", source.Augmentation.ID);
                            Helper.Copy(openFileDialog.FileName, newDataPath, "data" + Path.GetExtension(openFileDialog.FileName));
                            ((FileSource)source).Data = Path.Combine(newDataPath, "data" + Path.GetExtension(openFileDialog.FileName));

                            //add references in Augmentation, Picturebox + project.sources List.
                            ((AbstractDynamic2DAugmentation)currentElement).Source = source;


                            //make it possible to add a query to the source.
                            DialogResult dialogResult = MessageBox.Show("Möchten sie ein Query zu der Source öffnen?", "Query?", MessageBoxButtons.YesNo);

                            if (dialogResult == DialogResult.Yes)
                            {
                                openFileDialog = new OpenFileDialog();
                                openFileDialog.InitialDirectory = openFileDialog.FileName;
                                openFileDialog.Title = "Query File auswählen";
                                openFileDialog.Filter = "JavaScriptFile (*.js)|*.js";
                                if (openFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    string newQueryPath = Path.Combine(Environment.CurrentDirectory, "tmp", source.Augmentation.ID);
                                    Helper.Copy(openFileDialog.FileName, newQueryPath, "query.js");
                                    ((FileSource)source).Query = Path.Combine(newQueryPath, "query.js");

                                    Helper.Copy(Path.Combine(Environment.CurrentDirectory, "res","templates","chart(source).html"),newQueryPath,"chart.html");
                                }
                            }
                            this.reloadPreviewable(currentElement);
                        }
                    }
                    else
                    {
                        //set reference to the augmentations in Source
                        OpenFileDialog openFileDialog;
                        openFileDialog = new OpenFileDialog();
                        openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\highcharts";
                        openFileDialog.Title = "Query File auswählen";
                        openFileDialog.Filter = "JavaScriptFile (*.js)|*.js";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            source.initElement(ew);
                            source.Augmentation = ((AbstractDynamic2DAugmentation)currentElement);

                            string newQueryPath = Path.Combine(Environment.CurrentDirectory, "tmp\\" + source.Augmentation.ID);
                            ARdevKit.Model.Project.File.Helper.Copy(openFileDialog.FileName, newQueryPath, "query.js");
                            ((DbSource)source).Query = Path.Combine(newQueryPath, "query.js");
                            
                            Helper.Copy(Path.Combine(Environment.CurrentDirectory, "res", "templates", "chart(source).html"), newQueryPath, "chart.html");

                            //add references in Augmentation, Picturebox + project.sources List.
                            ((AbstractDynamic2DAugmentation)currentElement).Source = source;

                            this.reloadPreviewable(currentElement);
                        }



                        source.Augmentation = ((AbstractDynamic2DAugmentation)currentElement);
                    }
                    ew.PropertyGrid1.SelectedObject = source;
                    updateElementCombobox(trackable);
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = source;
                }
            }
        }


        /// <summary>
        /// Removes the choosen Source out of the Augmentation and also out of the sourcesList in Project.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="currentElement">The current element.</param>
        public void removeSource(AbstractSource source, IPreviewable currentElement)
        {
            if (source == null)
                throw new ArgumentException("parameter source was null.");
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            if (currentElement is AbstractAugmentation)
            {
                ((AbstractDynamic2DAugmentation)currentElement).Source = null;
                this.ew.project.Sources.Remove(source);
                this.findBox(currentElement).Image = this.getSizedBitmap(currentElement);
                this.findBox(currentElement).Refresh();
            }
            updateElementCombobox(trackable);
        }


        /// <summary>
        /// Removes the Previewable and the Objekt, what is linked to the Previewable.  
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        public void removePreviewable(IPreviewable currentElement)
        {
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            if (currentElement is AbstractTrackable && trackable != null)
            {
                this.removeAll();
                if (!this.ew.project.hasTrackable())
                {
                    this.ew.ElementSelectionController.setElementEnable(typeof(PictureMarker), true);
                    this.ew.ElementSelectionController.setElementEnable(typeof(IDMarker), true);
                    this.ew.ElementSelectionController.setElementEnable(typeof(ImageTrackable), true);
                }
                this.ew.Tsm_editor_menu_edit_delete.Enabled = false;
            }
            else if (currentElement is AbstractAugmentation && trackable != null)
            {
                this.panel.Controls.Remove(this.findBox((AbstractAugmentation)currentElement));
                this.ew.project.RemoveAugmentation((AbstractAugmentation)currentElement);
            }
            updateElementCombobox(trackable);
            this.ew.Tsm_editor_menu_edit_delete.Enabled = false;
        }


        /// <summary>
        /// Removes all Elements from the PreviewPanel and clears all references and delete the trackable from the list.
        /// </summary>
        private void removeAll()
        {
            this.panel.Controls.Clear();
            this.trackable = null;
            this.ew.project.Trackables[index] = null;
            updateElementCombobox(trackable);
        }


        /// <summary>
        /// updates the preview panel.
        /// </summary>
        public void updatePreviewPanel()
        {
            this.panel.Controls.Clear();
            this.ew.project.Trackables.Add(trackable);
            updateElementCombobox(trackable);
            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("einfügen", new EventHandler(this.paste_augmentation));
            cm.MenuItems[0].Enabled = false;
            this.panel.ContextMenu = cm;
        }

        /// <summary>
        /// load the project with the identical index to the previewPanel 
        /// (the index is the index of the trackable list in project)
        /// </summary>
        /// <param name="index">The index.</param>
        public void reloadPreviewPanel(int index)
        {
            //if it's a scene which exists reload scene
            if (index < this.ew.project.Trackables.Count)
            {

                this.index = index;
                this.trackable = this.ew.project.Trackables[index];
                this.panel.Controls.Clear();
                //makes differences between the kind of trackables
                if (trackable != null)
                {
                    this.addAllToPanel(this.ew.project.Trackables[index]);
                }
                if (this.trackable != null && trackable is IDMarker)
                {
                    this.ew.ElementSelectionController.setElementEnable(typeof(PictureMarker), false);
                    this.ew.ElementSelectionController.setElementEnable(typeof(ImageTrackable), false);
                }
                else if (this.trackable != null && trackable is PictureMarker)
                {
                    this.ew.ElementSelectionController.setElementEnable(typeof(IDMarker), false);
                    this.ew.ElementSelectionController.setElementEnable(typeof(ImageTrackable), false);
                }
                else if (this.trackable != null && this.trackable is ImageTrackable)
                {
                    this.ew.ElementSelectionController.setElementEnable(typeof(IDMarker), false);
                    this.ew.ElementSelectionController.setElementEnable(typeof(PictureMarker), false);
                }
            }
            //if the scene is empty create a new empty scene
            else if (index >= this.ew.project.Trackables.Count)
            {
                this.index = index;
                this.trackable = null;
                this.panel.Controls.Clear();
                this.ew.project.Trackables.Add(trackable);
            }
            //set currentElement, copyButton, deleteButton & property grid to null
            this.ew.CurrentElement = null;
            this.ew.Tsm_editor_menu_edit_delete.Enabled = false;
            this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
            ew.Cmb_editor_properties_objectSelection.Items.Clear();
            updateElementCombobox(trackable);
        }


        /// <summary>
        /// Add all existent Objects of the trackable in the Panel, this funktion is exists for change the trackable.
        /// </summary>
        /// <param name="trackable">The trackable.</param>
        private void addAllToPanel(AbstractTrackable trackable)
        {
            if (trackable.Augmentations.Count > 0)
            {
                foreach (AbstractAugmentation aug in trackable.Augmentations)
                {
                    this.scale = 100 / (double)((Abstract2DTrackable)this.trackable).Size / 1.6;
                    if (aug is Chart)
                    {
                        this.addPictureBox(aug, this.recalculateChartVector(aug.Translation));
                    }
                    else
                    {
                        this.addPictureBox(aug, this.recalculateVector(aug.Translation));
                    }

                    if (typeof(AbstractDynamic2DAugmentation).IsAssignableFrom(aug.GetType()) && ((AbstractDynamic2DAugmentation)aug).Source != null)
                    {
                        this.setSourcePreview(aug);
                    }
                }
            }
            this.addPictureBox(trackable, trackable.vector);
        }

        /// <summary>
        /// Reloads a single previewable.
        /// </summary>
        /// <param name="prev">The previous.</param>
        public void reloadPreviewable(AbstractAugmentation prev)
        {
            this.panel.Controls.Remove(this.findBox(prev));
            if (prev is Chart)
            {
                this.addPictureBox(prev, this.recalculateChartVector(prev.Translation));
            }
            else
            {
                this.addPictureBox(prev, this.recalculateVector(prev.Translation));
            }

            if (typeof(AbstractDynamic2DAugmentation).IsAssignableFrom(prev.GetType()) && ((AbstractDynamic2DAugmentation)prev).Source != null)
            {
                this.setSourcePreview(prev);
            }
        }


        /// <summary>
        /// Adds a PictureBox with for the currentElement to the aktuell Scene.
        /// </summary>
        /// <param name="prev">The previous.</param>
        /// <param name="vector">The vector.</param>
        /// <exception cref="System.ArgumentNullException">
        /// parameter prev was null
        /// or
        /// parameter vector was null
        /// </exception>
        public void addPictureBox(IPreviewable prev, Vector3D vector)
        {
            if (prev == null)
                throw new ArgumentException("parameter prev was null");

            if (vector == null)
                throw new ArgumentException("parameter vector was null");

            //creates the temporateBox with all variables, which'll be add than to the panel.
            PictureBox tempBox;
            tempBox = new PictureBox();
            tempBox.Image = this.scaleIPreviewable(prev);
            tempBox.SizeMode = PictureBoxSizeMode.AutoSize;

            tempBox.Location = new Point((int)(vector.X - tempBox.Size.Width / 2), (int)(vector.Y - tempBox.Size.Height / 2));

            tempBox.Tag = prev;
            ContextMenu cm = new ContextMenu();

            //adds drag&drop events for augmentations so that sources can be droped on them
            if (prev is AbstractAugmentation)
            {
                ((Control)tempBox).AllowDrop = true;
                DragEventHandler enterHandler = new DragEventHandler(onAugmentationEnter);
                DragEventHandler dropHandler = new DragEventHandler(onAugmentationDrop);
                tempBox.DragEnter += enterHandler;
                tempBox.DragDrop += dropHandler;
                //adds menuItems for the contextmenue
                cm.MenuItems.Add("kopieren", new EventHandler(this.copy_augmentation));
                
                //great extra work for Charts
                if (prev is Chart)
                {
                    cm.MenuItems.Add("Öffne Optionen", new EventHandler(this.openOptionsFile));
                    //declare local variables used to initialize the ChartPreview
                    string newPath = Path.Combine(Environment.CurrentDirectory, "tmp", ((Chart)prev).ID);
                    
                    initializeChartPreviewAt((Chart)prev, newPath);
                    WebBrowser wb = new WebBrowser();

                    //modify wb and navigate to desired HTML
                    wb.ScrollBarsEnabled = false;
                    wb.Navigate(new Uri(Path.Combine(newPath, "chart.html")));
                    //add it to pictureBox
                    tempBox.Controls.Add(wb);
                    wb.Location = new System.Drawing.Point(0, 0);
                    wb.Size = wb.Parent.Size;
                    wb.DocumentCompleted += deactivateWebView;
                }
            }
            tempBox.MouseClick += new MouseEventHandler(selectElement);
            cm.MenuItems.Add("löschen", new EventHandler(this.remove_by_click));
            cm.Tag = prev;
            cm.Popup += new EventHandler(this.popupContextMenu);
            tempBox.ContextMenu = cm;


            if (tempBox.Tag is AbstractAugmentation)
                tempBox.MouseMove += new MouseEventHandler(controlMouseMove);

            this.panel.Controls.Add(tempBox);

            if (prev is ImageAugmentation || prev is VideoAugmentation)
            {
                if (((AbstractAugmentation)prev).Rotation.Z != 0)
                {
                    this.rotateAugmentation(prev);
                }
            }
        }

        private void initializeChartPreviewAt(Chart chart, string newPath)
        {
            string res = Path.Combine(Environment.CurrentDirectory, "res");
            
            if(!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            string optionsPath;
            if(ew.project.ProjectPath != null)
            {
                optionsPath = Path.Combine(ew.project.ProjectPath, chart.ResFilePath);
            }
            else
            {
                optionsPath = chart.ResFilePath;
            }
            
            
            if ((chart.ResFilePath !=null && chart.ResFilePath != "") 
                && !File.Exists(Path.Combine(newPath, "options.js")))
            {
                if(Helper.Copy(optionsPath, newPath, "options.js"))
                {
                    chart.ResFilePath = Path.Combine(newPath, "options.js");
                }
            }
            if(chart.Source != null && chart.Source.Query != null
                && chart.Source.Query != "")
            {
                //set queryPath
                string queryPath;
                if (ew.project.ProjectPath != null)
                {
                    queryPath = Path.Combine(ew.project.ProjectPath, chart.Source.Query);
                }
                else
                {
                    queryPath = chart.Source.Query;
                }

                if (!File.Exists(Path.Combine(newPath, "chart.html")))
                {
                    Helper.Copy(Path.Combine(res,"templates", "chart(source).html"), newPath, "chart.html");
                }
                if(!File.Exists(Path.Combine(newPath, "query.js")))
                {
                    if (Helper.Copy(queryPath, newPath, "query.js"))
                    {
                        chart.Source.Query = Path.Combine(newPath, "query.js");
                    }
                }
                if(chart.Source is FileSource 
                    && ((FileSource)chart.Source).Data != null
                    && ((FileSource)chart.Source).Data != ""
                    && !File.Exists(Path.Combine(newPath, "data" 
                    + Path.GetExtension(((FileSource)chart.Source).Data))))
                {
                    //set dataPath
                    string dataPath;
                    if (ew.project.ProjectPath != null)
                    {
                        dataPath = Path.Combine(ew.project.ProjectPath, ((FileSource)chart.Source).Data);
                    }
                    else
                    {
                        dataPath = chart.Source.Query;
                    }

                    if (Helper.Copy(dataPath, newPath, "data" 
                        + Path.GetExtension(((FileSource)chart.Source).Data)))
                    {
                        ((FileSource)chart.Source).Data = Path.Combine(newPath,"data" 
                        + Path.GetExtension(((FileSource)chart.Source).Data));
                    }
                }
            }
            if (!File.Exists(Path.Combine(newPath, "chart.html")))
            {
                Helper.Copy(Path.Combine(res, "templates", "chart.html"), newPath);
            }
            if (!File.Exists(Path.Combine(newPath, "jquery-1.11.1.js")))
            {
                Helper.Copy(Path.Combine(res, "jquery", "jquery-1.11.1.js"), newPath);
            }
            if (!File.Exists(Path.Combine(newPath, "highcharts.js")))
            {
                Helper.Copy(Path.Combine(res, "highcharts", "highcharts.js"), newPath);
            }
        }

        private void deactivateWebView(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            if (wb.Parent.Tag is Chart)
            {
                Chart chart = (Chart)wb.Parent.Tag;
                string style = string.Format("height:{0}px; width:{1}px; positioning:{2}",
                    chart.Height, chart.Width, chart.Positioning.PositioningMode.ToString());
                wb.Document.All["chart"].Style = style;
                if(chart.Source != null)
                {
                    if(chart.Source is FileSource)
                    {
                        FileSource fs = (FileSource)chart.Source;
                        if (fs.Query != null || fs.Query != "")
                        {
                            if (fs.Data != null)
                            {
                                string data = "data" + Path.GetExtension(fs.Data);
                                Object[] path = new Object[1];
                                path[0] = (object)data;
                                wb.Document.InvokeScript("startChart", path);
                            }
                        }
                    }
                    if(chart.Source is DbSource)
                    {
                        DbSource ds = (DbSource)chart.Source;
                        if (ds.Query != null || ds.Query != "")
                        {
                            if (ds.Url != null)
                            {
                                Object[] path = new Object[1];
                                path[0] = (object)ds.Url;
                                wb.Document.InvokeScript("startChart", path);
                            }
                        }
                    }
                }
                else
                {
                    wb.Document.InvokeScript("startChart");
                }
            }
            ((Control)sender).Enabled = false;
        }

        /// <summary>
        /// Searchs in the Panel for the important PictureBox and gives this box back.
        /// </summary>
        /// <param name="prev">The previous.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">parameter prev was null.</exception>
        public PictureBox findBox(IPreviewable prev)
        {
            if (prev == null)
                throw new ArgumentException("parameter prev was null.");
            if (prev is AbstractTrackable)
            {
                foreach (Control comp in panel.Controls)
                {
                    if (comp.Tag == (AbstractTrackable)prev)
                    {
                        return (PictureBox)comp;
                    }
                }
            }
            else if (prev is AbstractAugmentation)
            {
                foreach (Control comp in panel.Controls)
                {
                    if (comp.Tag == (AbstractAugmentation)prev)
                    {
                        return (PictureBox)comp;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// sets the currentElement in EditorWindow an marks the PictureBox in the PreviewPanel.
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        public void setCurrentElement(IPreviewable currentElement)
        {
            //if there is a currentElement we musst set the box of the actual currentElement to normal
            //the box of the new currentElement will be set to the new Borderstyle.
            if (currentElement != null)
            {
                if (this.ew.CurrentElement != currentElement)
                {
                    if (currentElement is AbstractSource)
                    {
                        this.ew.CurrentElement = ((AbstractSource)currentElement).Augmentation;
                    }
                    else
                    {
                        this.ew.CurrentElement = currentElement;
                    }

                    if (this.ew.CurrentElement is AbstractAugmentation)
                    {
                        this.ew.Tsm_editor_menu_edit_copie.Enabled = true;
                    }
                    else if (this.ew.CurrentElement is AbstractTrackable)
                    {
                        this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
                    }

                    foreach (Control comp in this.panel.Controls)
                    {
                        if (((PictureBox)comp).BorderStyle == BorderStyle.FixedSingle)
                        {
                            ((PictureBox)comp).BorderStyle = BorderStyle.None;
                            ((PictureBox)comp).Refresh();
                        }
                    }
                    findBox(this.ew.CurrentElement).BorderStyle = BorderStyle.FixedSingle;
                    findBox(this.ew.CurrentElement).Refresh();
                    if (typeof(AbstractAugmentation).IsAssignableFrom(this.ew.CurrentElement.GetType()))
                    {
                        findBox(this.ew.CurrentElement).BringToFront();
                    }
                }
                ew.PropertyGrid1.SelectedObject = currentElement;
            }
            //if there is no currentElement we'll mark the box and set the currentElement in EditorWindow.
            else
            {
                this.ew.CurrentElement = null;
                foreach (Control comp in this.panel.Controls)
                {
                    if (((PictureBox)comp).BorderStyle == BorderStyle.FixedSingle)
                    {
                        ((PictureBox)comp).BorderStyle = BorderStyle.None;
                        ((PictureBox)comp).Refresh();
                    }
                }
                this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
            }
            updateElementCombobox(trackable);
            ew.Cmb_editor_properties_objectSelection.SelectedItem = currentElement;
            if (this.ew.CurrentElement != null)
            {
                this.ew.Tsm_editor_menu_edit_delete.Enabled = true;
            }
        }
        /// <summary>
        /// set the PictureBox of the Augmentation to a augmentationPreview with source icon
        /// this is only able when the Augmentation is a Chart (all other Augmentations accepts no Source)
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        private void setSourcePreview(IPreviewable currentElement)
        {
            PictureBox temp = this.findBox(currentElement);
            //Image image1 = this.getSizedBitmap(currentElement);
            //Image image2 = new Bitmap(ARdevKit.Properties.Resources.db_small);
            //Image newPic = new Bitmap(image1.Width, image1.Height);

            //Graphics graphic = Graphics.FromImage(newPic);
            //graphic.DrawImage(image1, new Rectangle(0, 0, image1.Width, image1.Height));
            //graphic.DrawImage(image2, new Rectangle(0, 0, image1.Width / 4, image1.Height / 4));
            //temp.Image = newPic;
            //adds the new ContextMenuItems for Source
            temp.ContextMenu.MenuItems.Add("Source anzeigen", new EventHandler(this.show_source_by_click));
            temp.ContextMenu.MenuItems.Add("Source löschen", new EventHandler(this.remove_source_by_click));
            temp.ContextMenu.MenuItems.Add("QueryFile öffnen", new EventHandler(this.openQueryFile));
            if (((AbstractDynamic2DAugmentation)currentElement).Source is FileSource)
            {
                temp.ContextMenu.MenuItems.Add("SourceFile öffnen", new EventHandler(this.openSourceFile));
                //if there is no Query added the QueryButton'll be disabled.
                if (((AbstractDynamic2DAugmentation)currentElement).Source.Query == null)
                {
                    temp.ContextMenu.MenuItems[5].Enabled = false;
                }
            }
            temp.Refresh();
        }
        /// <summary>
        /// Recalculates the vector in dependency to panel.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>the new Vector in dependency to the panel</returns>
        private Vector3D recalculateVector(Vector3D v)
        {
            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (panel.Width / 2 + v.X * 1.6 * scale);
            result.Y = (panel.Height / 2 - v.Y * 1.6 * scale);
            return result;
        }

        private Vector3D recalculateChartVector(Vector3D v)
        {
            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (panel.Width / 2 + v.X);
            result.Y = (panel.Height / 2 - v.Y);
            return result;
        }

        /// <summary>
        /// scales the Pictureboxes to their own scale size
        /// the size is in dependency to the scale, the sideScale of the images and and the scale of the augmentation.
        /// </summary>
        /// <param name="prev">The previous.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">parameter prev was null.</exception>
        /// <exception cref="System.InvalidOperationException">trackable was not set beforehand.</exception>
        public Bitmap scaleIPreviewable(IPreviewable prev)
        {
            if (prev == null)
                throw new ArgumentException("parameter prev was null.");

            if (trackable == null)
                throw new InvalidOperationException("trackable was not set beforehand.");

            int height = prev.getPreview(ew.project.ProjectPath).Height;
            int width = prev.getPreview(ew.project.ProjectPath).Width;
            double sideScale;

            if (((Abstract2DTrackable)this.trackable).Size == 0)
            {
                this.scale = 100;
            }
            else
            {
                this.scale = 100 / (double)((Abstract2DTrackable)this.trackable).Size / 1.6;
            }

            double scalex = width / scale;
            double scaley = height / scale;

            //scales the trackable to its standartsize in dependency to the sideScale
            if (prev is AbstractTrackable)
            {
                if (width > height)
                {
                    sideScale = scalex / scaley;
                    return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), (int)(100 * sideScale), 100);
                }
                else if (width <= height)
                {
                    sideScale = scaley / scalex;
                    return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), 100, (int)(100 * sideScale));
                }
                else { return null; }
            }
            //scales the augmentations in relation to the trackable, exception charts, these has a standartSize
            else if (prev is AbstractAugmentation)
            {
                if (prev is ImageAugmentation || prev is VideoAugmentation)
                {
                    if (((AbstractAugmentation)prev).Scaling.X == 0 && ((AbstractAugmentation)prev).Scaling.Y == 0
                            && ((AbstractAugmentation)prev).Scaling.Z == 0)
                    {
                        ((AbstractAugmentation)prev).Scaling = new Vector3D(1, 1, 1);
                    }
                    else if (((AbstractAugmentation)prev).Scaling.X <= 0 && ((AbstractAugmentation)prev).Scaling.Y != 0
                        && ((AbstractAugmentation)prev).Scaling.Z != 0)
                    {
                        ((AbstractAugmentation)prev).Scaling = new Vector3D(0.01,
                            ((AbstractAugmentation)prev).Scaling.Y, ((AbstractAugmentation)prev).Scaling.Z);
                    }
                    else if (((AbstractAugmentation)prev).Scaling.X != 0 && ((AbstractAugmentation)prev).Scaling.Y <= 0
                        && ((AbstractAugmentation)prev).Scaling.Z != 0)
                    {
                        ((AbstractAugmentation)prev).Scaling = new Vector3D(((AbstractAugmentation)prev).Scaling.X,
                            0.01, ((AbstractAugmentation)prev).Scaling.Z);
                    }
                    else if (((AbstractAugmentation)prev).Scaling.X != 0 && ((AbstractAugmentation)prev).Scaling.Y != 0
                        && ((AbstractAugmentation)prev).Scaling.Z <= 0)
                    {
                        ((AbstractAugmentation)prev).Scaling = new Vector3D(((AbstractAugmentation)prev).Scaling.X,
                            ((AbstractAugmentation)prev).Scaling.Y, 0.01);
                    }

                    if (width > height)
                    {
                        sideScale = scalex / scaley;
                        return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), (int)(scale * 100 * ((AbstractAugmentation)prev).Scaling.X * sideScale * sideScale * 1.3),
                                (int)(scale * 100 * ((AbstractAugmentation)prev).Scaling.Y * sideScale * 1.3));
                    }
                    else if (width <= height)
                    {
                        sideScale = scaley / scalex;
                        return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), (int)(scale * 100 * ((AbstractAugmentation)prev).Scaling.X * 1.3),
                                (int)(scale * 100 * ((AbstractAugmentation)prev).Scaling.Y * sideScale * 1.3));
                    }
                    else { return null; }
                }

                //if the currentElement is a chart chosse this. The chart Scaling is an exception in the calculation
                else if (prev is Chart)
                {
                    return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), ((Chart)prev).Width, ((Chart)prev).Height);
                }
                else { return null; }
            }
            else { return null; }
        }

        /// <summary>
        /// Scales the bitmap.
        /// dirty workaround: when the scaling might get too big, the original image is returned
        /// </summary>
        /// <param name="bit">The bit.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">parameter bit was null.</exception>
        public Bitmap scaleBitmap(Bitmap bit, int width, int height)
        {
            if (bit == null)
                throw new ArgumentException("parameter bit was null.");
            if (width == null)
                throw new ArgumentException("parameter width was null.");
            if (height == null)
                throw new ArgumentException("parameter height was null.");

            try
            {
                Bitmap resizedImg = new Bitmap(width, height);
                Bitmap img = bit;

                using (Graphics gNew = Graphics.FromImage(resizedImg))
                {
                    gNew.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gNew.DrawImage(img, new Rectangle(0, 0, width, height));
                }

                return resizedImg;
            }
            catch (ArgumentException ae)
            {
                Debug.WriteLine("Bitmap konnte nicht skaliert werden");
            }

            return bit;
        }

        /// <summary>
        /// Rescales the preview panel if the size was changed.
        /// </summary>

        public void rescalePreviewPanel()
        {
            int width = this.panel.Width;
            int height = this.panel.Height;

            foreach (AbstractTrackable trackable in this.ew.project.Trackables)
            {
                if (trackable != null)
                {
                    trackable.vector = new Vector3D(width / 2, height / 2, 0);
                    foreach (AbstractAugmentation aug in trackable.Augmentations)
                    {
                        if (aug is Chart)
                        {
                            ((Chart)aug).Positioning.Left = (int)((aug.Translation.X + panel.Width / 2));
                            ((Chart)aug).Positioning.Top = (int)((aug.Translation.Y + panel.Width / 2));
                        }
                    }
                }
            }
            int i = this.index;
            this.index = -1;
            this.reloadPreviewPanel(i);
        }

        /// <summary>
        /// Set all needed Coordinates for the augmentation.
        /// </summary>
        /// <param name="prev">The previous.</param>
        /// <param name="newV">The new v.</param>
        /// <exception cref="System.ArgumentException">
        /// parameter prev was null.
        /// or
        /// parameter newV was null.
        /// </exception>
        public void setCoordinates(IPreviewable prev, Vector3D newV)
        {
            if (prev == null)
                throw new ArgumentException("parameter prev was null.");
            if (newV == null)
                throw new ArgumentException("parameter newV was null.");

            if (prev is Chart)
            {
                ((Chart)prev).Positioning.Left = (int)newV.X;
                ((Chart)prev).Positioning.Top = (int)newV.Y;

                Vector3D result = new Vector3D(0, 0, 0);
                result.X = (int)((newV.X - panel.Width / 2));
                result.Y = (int)((panel.Height / 2 - newV.Y));
                ((AbstractAugmentation)prev).Translation = result;
            }
            else
            {
                Vector3D result = new Vector3D(0, 0, 0);
                result.X = (int)((newV.X - panel.Width / 2) / scale / 1.6);
                result.Y = (int)((panel.Height / 2 - newV.Y) / scale / 1.6);
                ((AbstractAugmentation)prev).Translation = result;
            }
        }

        /// <summary>
        /// This updates the position of the currentElement-Picturebox.
        /// </summary>
        public void updateTranslation()
        {
            AbstractAugmentation current;

            if (ew.CurrentElement is AbstractAugmentation)
                current = (AbstractAugmentation)ew.CurrentElement;
            else
                return;

            Vector3D tmp = recalculateVector(current.Translation);

            PictureBox box = findBox(current);
            box.Location = new Point((int)tmp.X - (box.Size.Width / 2), (int)tmp.Y - (box.Size.Height / 2));
        }

        /// <summary>
        /// Updates the element combobox.
        /// </summary>
        /// <param name="t">The t.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void updateElementCombobox(AbstractTrackable t)
        {
            if (t != null)
            {
                if (ew.Cmb_editor_properties_objectSelection.Items.Count != 1 + t.Augmentations.Count + ew.project.Sources.Count)
                {
                    ew.Cmb_editor_properties_objectSelection.Items.Clear();
                    ew.Cmb_editor_properties_objectSelection.Items.Add(t);
                    foreach (AbstractAugmentation a in t.Augmentations)
                    {
                        ew.Cmb_editor_properties_objectSelection.Items.Add(a);
                    }
                    foreach (AbstractSource s in ew.project.Sources)
                    {
                        ew.Cmb_editor_properties_objectSelection.Items.Add(s);
                    }
                }
            }
            else
            {
                ew.Cmb_editor_properties_objectSelection.Items.Clear();
            }
        }

        /// <summary>
        /// Rotates the augmentation, after you've changed the Rotation.Z Vector.
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        /// <exception cref="System.ArgumentException">parameter currentElement was null.</exception>
        public void rotateAugmentation(IPreviewable currentElement)
        {
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            IPreviewable prev = currentElement;
            int grad = -(int)((AbstractAugmentation)prev).Rotation.Z;
            PictureBox box = this.findBox(prev);
            Bitmap imgOriginal = this.getSizedBitmap(prev);

            Bitmap tempBitmap = new Bitmap((int)(imgOriginal.Width), (int)(imgOriginal.Height));
            tempBitmap.SetResolution(imgOriginal.HorizontalResolution, imgOriginal.HorizontalResolution);
            System.Drawing.Graphics Graph = Graphics.FromImage(tempBitmap);
            Matrix X = new Matrix();
            X.RotateAt(grad, new Point((imgOriginal.Width / 2), (imgOriginal.Height / 2)));
            Graph.Transform = X;
            Graph.DrawImageUnscaled(imgOriginal, new Point(0, 0));

            X.Dispose();
            box.Image = tempBitmap;
            X.Dispose();
        }

        /// <summary>
        /// Refreshs the Augmentation with the new Scale.
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">parameter currentElement was null.</exception>
        /// <exception cref="System.InvalidOperationException">trackable was not set beforehand</exception>
        public Bitmap getSizedBitmap(IPreviewable currentElement)
        {
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            if (this.trackable == null)
                throw new InvalidOperationException("trackable was not set beforehand");

            IPreviewable prev = currentElement;
            PictureBox box = this.findBox(prev);

            this.scale = 100 / (double)((Abstract2DTrackable)this.trackable).Size / 1.6;

            if (prev is AbstractAugmentation)
            {
                if (prev is ImageAugmentation || prev is VideoAugmentation)
                {
                    if (prev.getPreview(ew.project.ProjectPath).Width > prev.getPreview(ew.project.ProjectPath).Height)
                    {
                        double sideScale = (double)prev.getPreview(ew.project.ProjectPath).Width / (double)prev.getPreview(ew.project.ProjectPath).Height;
                        return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), (int)(100 * ((AbstractAugmentation)prev).Scaling.X * scale * sideScale * sideScale * 1.3),
                            (int)(100 * ((AbstractAugmentation)prev).Scaling.Y * scale * sideScale * 1.3));
                    }
                    else if (prev.getPreview(ew.project.ProjectPath).Width < prev.getPreview(ew.project.ProjectPath).Height)
                    {
                        double sideScale = (double)prev.getPreview(ew.project.ProjectPath).Height / (double)prev.getPreview(ew.project.ProjectPath).Width;
                        return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), (int)(100 * ((AbstractAugmentation)prev).Scaling.X * scale * 1.3),
                            (int)(100 * ((AbstractAugmentation)prev).Scaling.Y * scale * sideScale * 1.3));
                    }
                    else { return null; }

                }
                else if (prev is Chart)
                {
                    return this.scaleBitmap(prev.getPreview(ew.project.ProjectPath), ((Chart)prev).Width, ((Chart)prev).Height);
                }
                else { return null; }
            }
            else { return null; }
        }


        //////////////////////////////////////////////////////////////////////////////////EVENTS/////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Select element (Event).
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Mouse event information.</param>

        private void selectElement(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ew.PropertyGrid1.SelectedObject = ((Control)sender).Tag;
                this.setCurrentElement((IPreviewable)((Control)sender).Tag);
            }
        }

        /// <summary>
        /// Event to move a object of type Control.
        /// Also updates x/y coord in the Tag of the control.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Mouse event information.</param>
        private void controlMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IPreviewable prev = (IPreviewable)((Control)sender).Tag;
                PictureBox box = this.findBox(prev);
                this.setCurrentElement(prev);
                Control controlToMove = (Control)sender;
                controlToMove.BringToFront();

                if (((Control)sender).Tag is AbstractAugmentation)
                {

                    AbstractAugmentation aa;
                    aa = (AbstractAugmentation)((Control)sender).Tag;
                    this.setCoordinates(this.ew.CurrentElement, new Vector3D((int)((controlToMove.Location.X + e.Location.X)),
                        (int)((controlToMove.Location.Y + e.Location.Y)), 0));
                }
                controlToMove.Location = new Point(controlToMove.Location.X + e.Location.X - (box.Width / 2),
                       controlToMove.Location.Y + e.Location.Y - (box.Height / 2));
            }
        }

        /// <summary>
        /// Event handler. removes the current object.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void remove_by_click(object sender, EventArgs e)
        {
            IPreviewable temp = (IPreviewable)((ContextMenu)((MenuItem)sender).Parent).Tag;
            this.removePreviewable((IPreviewable)((ContextMenu)((MenuItem)sender).Parent).Tag);
            ew.PropertyGrid1.SelectedObject = null;
        }


        /// <summary>
        /// Event handler. Shows Source in PropertyGrid when you want this.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void show_source_by_click(object sender, EventArgs e)
        {
            ew.PropertyGrid1.SelectedObject = ((AbstractDynamic2DAugmentation)((ContextMenu)((MenuItem)sender).Parent).Tag).Source;
        }



        /// <summary>
        ///  Event handler. Removes the source of the augmentation and the contextmenuentries of this augmentation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void remove_source_by_click(object sender, EventArgs e)
        {
            AbstractDynamic2DAugmentation temp = (AbstractDynamic2DAugmentation)((ContextMenu)((MenuItem)sender).Parent).Tag;

            this.findBox(temp).ContextMenu.MenuItems.RemoveAt(4);
            this.findBox(temp).ContextMenu.MenuItems.RemoveAt(4);
            this.findBox(temp).ContextMenu.MenuItems.RemoveAt(4);
            if (((AbstractDynamic2DAugmentation)this.ew.CurrentElement).Source is FileSource)
            {
                this.findBox(temp).ContextMenu.MenuItems.RemoveAt(3);
            }

            this.removeSource(temp.Source, temp);
            ew.PropertyGrid1.SelectedObject = null;


        }

        /// <summary>
        /// EventHandler for copy function. copies the currentElement
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void copy_augmentation(object sender, EventArgs e)
        {
            if (typeof(AbstractAugmentation).IsAssignableFrom(this.ew.CurrentElement.GetType()))
            {
                this.copy = (AbstractAugmentation)this.ew.CurrentElement.Clone();
                this.panel.ContextMenu.MenuItems[0].Enabled = true;
                this.ew.setPasteButtonEnabled();
                //TODO enable contextmenu paste in MainContainer Conetextmenu
            }
        }
        /// <summary>
        /// EventHandler for paste function. paste the object at the current cursor position.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void paste_augmentation(object sender, EventArgs e)
        {
            if (this.trackable != null)
            {
                if (this.trackable.Augmentations.Count < 3)
                {
                    Point p = this.panel.PointToClient(Cursor.Position);
                    IPreviewable element = (IPreviewable)this.copy.Clone();
                    this.addPreviewable(element, new Vector3D(p.X, p.Y, 0));

                    if (element is AbstractDynamic2DAugmentation && ((AbstractDynamic2DAugmentation)element).Source != null)
                    {
                        this.setSourcePreview(element);
                    }
                }
            }
        }

        /// <summary>
        /// EventHandler for paste function. paste the object in the center of panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void paste_augmentation_center(object sender, EventArgs e)
        {
            if (this.trackable != null)
            {
                if (this.trackable.Augmentations.Count < 3)
                {
                    IPreviewable element = (IPreviewable)this.copy.Clone();
                    this.addPreviewable(element, new Vector3D(this.panel.Width / 2, this.panel.Height / 2, 0));

                    if (element is AbstractDynamic2DAugmentation && ((AbstractDynamic2DAugmentation)element).Source != null)
                    {
                        this.setSourcePreview(element);
                        ((AbstractDynamic2DAugmentation)element).Source = (AbstractSource)((AbstractDynamic2DAugmentation)copy).Source.Clone();
                    }
                }
            }
        }



        /**
         * <summary>    Raises the drag event when a source enters a augmentation. </summary>
         *
         * <remarks>    Robin, 19.01.2014. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information to send to registered event handlers. </param>
         */

        public void onAugmentationEnter(object sender, DragEventArgs e)
        {
            if (currentMetaCategory == MetaCategory.Source)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        /**
         * <summary>    Raises the drag event when a source is droped on an augmentation. </summary>
         *
         * <remarks>    Robin, 19.01.2014. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information to send to registered event handlers. </param>
         */

        public void onAugmentationDrop(object sender, DragEventArgs e)
        {
            if (currentMetaCategory == MetaCategory.Source)
            {
                ElementIcon icon = (ElementIcon)e.Data.GetData(typeof(ElementIcon));
                AbstractAugmentation augmentation = (AbstractAugmentation)((PictureBox)sender).Tag;
                AbstractSource source = (AbstractSource)icon.Element.Prototype.Clone();
                addSource(source, augmentation);
            }
        }

        /// <summary>
        /// EventHandler for Action before the ContextMenu is open.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void popupContextMenu(object sender, EventArgs e)
        {
            this.setCurrentElement((IPreviewable)((ContextMenu)sender).Tag);
            ew.PropertyGrid1.SelectedObject = (IPreviewable)((ContextMenu)sender).Tag;
        }

        /// <summary>
        /// EventHandler to open the Query in the Editor.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openQueryFile(object sender, EventArgs e)
        {
            try
            {
                string path = ((AbstractDynamic2DAugmentation)this.ew.CurrentElement).Source.Query;
                path = ew.project.ProjectPath == null ? path : Path.Combine(ew.project.ProjectPath, path);
                TextEditorForm tef = new TextEditorForm(path);
                if (tef.ShowDialog() == DialogResult.OK)
                {
                    reloadPreviewable((Chart)this.ew.CurrentElement);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// EventHandler to open the SourceFile in the Editor.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openSourceFile(object sender, EventArgs e)
        {
            try
            {
                string path = ((FileSource)((AbstractDynamic2DAugmentation)this.ew.CurrentElement).Source).Data;
                path = ew.project.ProjectPath == null ? path : Path.Combine(ew.project.ProjectPath, path);
                TextEditorForm tef = new TextEditorForm(path);
                if (tef.ShowDialog() == DialogResult.OK)
                {
                    reloadPreviewable((Chart)this.ew.CurrentElement);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// EventHandler to open the Options in the Editor.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openOptionsFile(object sender, EventArgs e)
        {
            try
            {
                string path = ((Chart)this.ew.CurrentElement).ResFilePath;
                path = ew.project.ProjectPath == null ? path : Path.Combine(ew.project.ProjectPath, path);
                TextEditorForm tef = new TextEditorForm(path);
                if(tef.ShowDialog() == DialogResult.OK)
                {
                    reloadPreviewable((Chart)this.ew.CurrentElement);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// EventHandle to open the AREL (customUserEvent) Script in the TextEditor
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openEventFile(object sender, EventArgs e)
        {
            try
            {
                string path;
                if (((AbstractAugmentation)ew.CurrentElement).EventFile == null)
                    ((AbstractAugmentation)ew.CurrentElement).EventFile = new ARdevKit.Model.Project.File.EventFile(Path.Combine("Events", ((AbstractAugmentation)ew.CurrentElement).ID + "_events.js"));
                else
                {
                    path = Path.Combine(ew.project.ProjectPath == null ? Environment.CurrentDirectory : ew.project.ProjectPath, ((AbstractAugmentation)ew.CurrentElement).EventFile.FilePath);
                    if (!System.IO.File.Exists(path))
                        ((AbstractAugmentation)ew.CurrentElement).EventFile = new ARdevKit.Model.Project.File.EventFile(Path.Combine("Events", ((AbstractAugmentation)ew.CurrentElement).ID + "_events.js"));
                }
                path = Path.Combine(ew.project.ProjectPath == null ? Environment.CurrentDirectory : ew.project.ProjectPath, ((AbstractAugmentation)ew.CurrentElement).EventFile.FilePath);
                TextEditorForm tef = new TextEditorForm(path);
                tef.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// EventHandler to delete the PictureBox of the currentElement from the Panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void delete_current_element(object sender, EventArgs e)
        {
            this.removePreviewable(this.ew.CurrentElement);
        }

        internal ScreenSize getMainContainerSize()
        {
            throw new NotImplementedException();
        }

        internal void updateElement(IPreviewable previewable)
        {
            throw new NotImplementedException();
        }
    }
}