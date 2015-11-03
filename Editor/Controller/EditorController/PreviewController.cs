using System;
using System.Drawing;
using System.Windows.Forms;
using ARdevKit.Model.Project;
using ARdevKit.View;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using ARdevKit.Model.Project.File;
using System.Threading;
using mshtml;
using System.Collections.Generic;

namespace ARdevKit.Controller.EditorController
{
    /// <summary>
    /// The class PreviewController manages all things which are in contact with the PreviewPanel. Here are all methods, who influence the PreviewPanel.
    /// </summary>
    public class PreviewController : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The Preview Port on which the WebsiteManager can be reached. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static readonly int PREVIEW_PORT = 1234;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>  The path where the temp data is stored. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public readonly string TEMP_PATH = Directory.GetCurrentDirectory() + "\\temp\\";
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The MetaCategory of the current element. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public MetaCategory currentMetaCategory { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The Trackable which hold the Augmentations and Sources. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public AbstractTrackable trackable { 
            get; 
            set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   The PreviewPanel which we need to add Previewables. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private WebBrowser htmlPreview;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   EditorWindow Instanz </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private EditorWindow ew;

        /// <summary>   The Index which Trackable out of Project we musst use </summary>
        private int index;

        /// <summary>
        /// gets the Index of the Trackable currently viewed,
        /// changes the Trackable which is viewn according to the trackables
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        /// <summary>
        /// a HashSet of IPreviewables, which associated files were changed and are reloaded, when the 
        /// Document containing them was completed loading
        /// </summary>
        private HashSet<IPreviewable>[] needToBeUpdated;

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

        /// <summary>
        /// Hosts the HTTPServer and manages changes to the scenes/Websites
        /// </summary>
        private WebSiteHTMLManager Websites;

        /// <summary>
        /// ContextMenu for the mainContainer
        /// </summary>
        private ContextMenu mainContainerContextMenu;

        /// <summary>
        /// ContextMenu for the augmentations
        /// </summary>
        private ContextMenu augmentationContextMenu;

        // <summary>
        /// ContextMenu for the chart without Source
        /// </summary>
        private ContextMenu chartContextMenu_noSource;
        // <summary>
        /// ContextMenu for the chart with FileSource and no Query
        /// </summary>
        private ContextMenu chartContextMenu_FileSource_noQuery;
        // <summary>
        /// ContextMenu for the chart with FileSource and Query
        /// </summary>
        private ContextMenu chartContextMenu_FileSource_Query;
        // <summary>
        /// ContextMenu for the chart with DbSource
        /// </summary>
        private ContextMenu chartContextMenu_DbSource;

        /// <summary>
        /// ContextMenu for the trackable
        /// </summary>
        private ContextMenu trackableContextMenu;

        /// <summary>
        /// indicates at which position the drag of an object has taken place
        /// </summary>
        private Point dragStartPosition;

        /// <summary>
        /// indicates if after the documentis loaded, the ew.currentElement should be marked
        /// </summary>
        private bool markCurrentElementInPreview;

        /// <summary>
        /// Thread for the Webserver to run in
        /// </summary>
        private Thread webServerThread;

        /// <summary>
        /// checks if files in the temp folder got changed and triggers the according events
        /// </summary>
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// a dictionary to look up which data belongs to which Previewable
        /// </summary>
        private Dictionary<string, IPreviewable> fileLookup;

        /// <summary>
        /// indicates if the PreviewController is disposed and should not be used anymore
        /// </summary>
        private bool disposed = false;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ew">
        /// EditorWindow which holds a Html_preview WebBrowserControl and 3 ToolStripMenuItems
        /// </param>
        /// <exception cref="System.ArgumentException">parameter ew was null.</exception>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public PreviewController(EditorWindow ew)
        {
            if (ew == null)
                throw new ArgumentException("parameter ew was null.");

            //get the WebBrowser which is ebbeded in the EditorWindow
            this.ew = ew;
            this.htmlPreview = this.ew.Html_preview;
            htmlPreview.DocumentCompleted += htmlPreview_DocumentFirstCompleted;

            //start with the first scene and no 
            this.currentMetaCategory = new MetaCategory();
            this.index = 0;
            this.trackable = null;
            this.ew.project.Trackables.Add(trackable);

            //create the temp directory if it does not exist
            if(!Directory.Exists(TEMP_PATH))
            Directory.CreateDirectory(TEMP_PATH);

            //start the Websitemanager, which hosts the previewpages
            this.Websites = new WebSiteHTMLManager(PREVIEW_PORT);
            Websites.changeMainContainerSize(EditorWindow.MINSCREENWIDTH, EditorWindow.MINSCREENHEIGHT);
            webServerThread = new Thread(new ThreadStart(Websites.listen));
            webServerThread.Name = "WebServerThread";
            webServerThread.IsBackground = true;

            webServerThread.Start();
            //navigate to the first site
            if (htmlPreview.Document != null)
                reloadCurrentWebsite();
            this.htmlPreview.Navigate("http://localhost:" + PREVIEW_PORT + "/" + index);

            //initialize mainContainerContextMenu
            mainContainerContextMenu = new ContextMenu();
            mainContainerContextMenu.MenuItems.Add("einfügen", paste_augmentation);
            mainContainerContextMenu.MenuItems[0].Enabled = false;

            //initialize augmentationContextMenu
            augmentationContextMenu = new ContextMenu();
            augmentationContextMenu.MenuItems.Add("kopieren", copy_augmentation);
            augmentationContextMenu.MenuItems.Add("löschen", delete_current_element);

            //initialize chartContextMenu(withoutSource)
            chartContextMenu_noSource = new ContextMenu();
            chartContextMenu_noSource.MenuItems.Add("kopieren", copy_augmentation);
            chartContextMenu_noSource.MenuItems.Add("löschen", delete_current_element);
            chartContextMenu_noSource.MenuItems.Add("options bearbeiten", openOptionsFile);
            //initialize chartContextMenu(withFileSource(without Query))
            chartContextMenu_FileSource_noQuery = new ContextMenu();
            chartContextMenu_FileSource_noQuery.MenuItems.Add("kopieren", copy_augmentation);
            chartContextMenu_FileSource_noQuery.MenuItems.Add("löschen", delete_current_element);
            chartContextMenu_FileSource_noQuery.MenuItems.Add("options bearbeiten", openOptionsFile);
            chartContextMenu_FileSource_noQuery.MenuItems.Add("Daten einsehen", openSourceFile);
            //initialize chartContextMenu(withFileSource(with Query))
            chartContextMenu_FileSource_Query = new ContextMenu();
            chartContextMenu_FileSource_Query.MenuItems.Add("kopieren", copy_augmentation);
            chartContextMenu_FileSource_Query.MenuItems.Add("löschen", delete_current_element);
            chartContextMenu_FileSource_Query.MenuItems.Add("options bearbeiten", openOptionsFile);
            chartContextMenu_FileSource_Query.MenuItems.Add("Daten einsehen", openSourceFile);
            chartContextMenu_FileSource_Query.MenuItems.Add("Query einsehen", openQueryFile);
            //initialize chartContextMenu(withDbSource)
            chartContextMenu_DbSource = new ContextMenu();
            chartContextMenu_DbSource.MenuItems.Add("kopieren", copy_augmentation);
            chartContextMenu_DbSource.MenuItems.Add("löschen", delete_current_element);
            chartContextMenu_DbSource.MenuItems.Add("options bearbeiten", openOptionsFile);
            chartContextMenu_DbSource.MenuItems.Add("Query einsehen", openQueryFile);

            //initialize trackableContextMenu
            trackableContextMenu = new ContextMenu();
            trackableContextMenu.MenuItems.Add("löschen", delete_current_element);
            trackableContextMenu.MenuItems[0].Enabled = true;

            //add Eventhandlers to the dropdownmenu in the editorwindow to copy, paste and delete the currentElement
            this.ew.Tsm_editor_menu_edit_paste.Click += new System.EventHandler(this.paste_augmentation_center);
            this.ew.Tsm_editor_menu_edit_copie.Click += new System.EventHandler(this.copy_augmentation);
            this.ew.Tsm_editor_menu_edit_delete.Click += new System.EventHandler(this.delete_current_element);

            markCurrentElementInPreview = false;

            //establish file management
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\temp");
            fileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory() + "\\temp");
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.Changed += fileWatcher_Changed;
            fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            fileWatcher.Filter = "";
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.SynchronizingObject = ew.Html_preview;
            fileWatcher.EnableRaisingEvents = true;
            fileLookup = new Dictionary<string,IPreviewable>();
            needToBeUpdated = new HashSet<IPreviewable>[10];
            for (int i = 0; i < needToBeUpdated.Length; i++)
            {
                needToBeUpdated[i] = new HashSet<IPreviewable>();               
            }
        }

        /// <summary>
        /// this Listener is triggered when the filWatcher recognizes changes.
        /// most important is the part to the changed file, which is looked up in the fileLookup
        /// and all associated Previewables are reloaded
        /// </summary>
        /// <param name="sender">the fileWatcher which detects a change</param>
        /// <param name="e">the Event describing the changes</param>
        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(fileLookup.ContainsKey(e.FullPath))
            {
                IPreviewable changedPrev = fileLookup[e.FullPath];
                if(changedPrev is AbstractAugmentation)
                {
                    if (trackable.Augmentations.Contains((AbstractAugmentation)changedPrev))
                    {
                        reloadPreviewable((AbstractAugmentation)fileLookup[e.FullPath]);
                    }
                    else
                    {
                        int i = ew.project.Trackables.FindIndex(track => track.Augmentations.Contains((AbstractAugmentation)changedPrev));
                        if(needToBeUpdated[i].Add(changedPrev))
                            MessageBox.Show("Es wurden Änderungen an einer Augmentation in einer anderen Szene vorgenommen,"
                                + "diese Änderungen werden erst angezeigt, wenn sie wieder Änderungen vornehmen, wenn die zu verändernde Szene die aktuell angezeigte ist.");
                    }
                   
                } else if(changedPrev is AbstractTrackable)
                {
                    if(trackable.Equals(changedPrev))
                    {
                        reloadPreviewable(changedPrev);
                    }
                    else
                    {
                        int i = ew.project.Trackables.FindIndex(track => track.Equals(changedPrev));
                        if (needToBeUpdated[i].Add(changedPrev))
                            MessageBox.Show("Es wurden Änderungen an einem Trackable in einer anderen Szene vorgenommen,"
                                + "diese Änderungen werden erst angezeigt, wenn sie wieder Änderungen vornehmen, wenn die zu verändernde Szene die aktuell angezeigte ist.");
                    }
                }                
            }
        }

        /// <summary>
        /// EventListener that is triggered only for the first time a Document is sucessfully loaded by the Webbrowser.
        /// It attaches the Listener that regularly reacts to the DocumentedCompletedEvent, and the MouseEventHandler for the MouseDown/Up Events
        /// </summary>
        /// <param name="sender">The WebBrowser which finished loading the Document</param>
        /// <param name="e">the WebBrowserDocumentCompletedEventArgs</param>
        private void htmlPreview_DocumentFirstCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            htmlPreview.DocumentCompleted -= htmlPreview_DocumentFirstCompleted;
            htmlPreview.DocumentCompleted += htmlPreview_DocumentCompleted;
            htmlPreview.Document.MouseDown += handleDocumentMouseDown;
            htmlPreview.Document.MouseUp += writeBackChangesfromDOM;
        }

        /// <summary>
        /// Is triggered every time the current Document has been fully loaded. It reattaches the Listener of to the DocuemntMouseDownEvents. It marks the
        /// ew.CurrentElement with a border, if it should still be marked. It attaches MouseHandler to the charts, which cannot be handled with the MouseHandler for the whole Document.
        /// It reloads the Previewables which are on the List of still to be updated.
        /// </summary>
        /// <param name="sender">The Webbrowser which finished loading the Document</param>
        /// <param name="e">a WebBrowserDocumentCompletedEventArgs</param>
        private void htmlPreview_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //to determine of this previewController should be currently used
            if (!this.ew.PreviewController.Equals(this) || disposed)
                throw new Exception("This PreviewController should not be atached to the htmlPreview!");

            //during fast reloading of the page the mouseEventhandler was not attached to the new Document so it is now
            htmlPreview.Document.MouseDown -= handleDocumentMouseDown;
            htmlPreview.Document.MouseUp -= writeBackChangesfromDOM;
            htmlPreview.Document.MouseDown += handleDocumentMouseDown;
            htmlPreview.Document.MouseUp += writeBackChangesfromDOM;

            //if the element was not added to the scene yet, or was not availible when it should have been selected,
            //markCurrentElementInPreview is set true, so when the new Document finishes, the right Element gets a border
            if (markCurrentElementInPreview)
            {
                if (ew.CurrentElement != null)
                {
                    IHTMLElement selectedElement;
                    if(ew.CurrentElement is AbstractSource)
                    {
                        selectedElement = (IHTMLElement)findElement(((AbstractSource)ew.CurrentElement).Augmentation).DomElement;
                    }
                    else
                    {
                        selectedElement = (IHTMLElement)findElement(this.ew.CurrentElement).DomElement;
                    }
                    selectedElement.style.border = "solid 2.5px #F39814";
                    selectedElement.style.zIndex = "10";
                }
            }
            
            //reload every Previewable that was changed, when the current index scene was not viewed and therefore added to needToBeUpdated
            foreach (IPreviewable item in needToBeUpdated[index])
            {
                reloadPreviewable(item);
            }
            needToBeUpdated[index].Clear();

            //attach Eventhandlers to all charts attached to the current trackable directly
            if(trackable != null)
            foreach (Chart chart in trackable.Augmentations.FindAll(x => x is Chart))
            {
                HtmlElement htmlChart = htmlPreview.Document.GetElementById(chart.ID);
                htmlChart.MouseDown -= chartMouseDown;
                htmlChart.MouseMove -= chartMouseMove;
                htmlChart.MouseDown += chartMouseDown;
                htmlChart.MouseMove += chartMouseMove;
            }
        }
        
        /// <summary>
        /// The senders DomElement(a IHTMLElement) is repositioned by adding the delta of the mouse positions during the whole
        /// dragging process to te current position.
        /// </summary>
        /// <param name="sender">a HtmlElement, which is currently added to a Document and has therefore a DomElement</param>
        /// <param name="e">HtmlElementEventArgs, mainly used for the OffsetMousePosition</param>
        private void chartMouseMove(object sender, HtmlElementEventArgs e)
        {
            //the sender must be a HtmlElement
            HtmlElement draggedElement = (HtmlElement)sender;
          
            //drag the draggedElement only if, the left MouseButton is clicked while moving
            if (e.MouseButtonsPressed == MouseButtons.Left)
            {
                IHTMLElement draggedDomElement = (IHTMLElement)draggedElement.DomElement;
                //get the left and top position, default values are 0px
                string oldMarginLeft = draggedDomElement.style.marginLeft;
                string oldMarginTop = draggedDomElement.style.marginTop;
                if (oldMarginLeft == null)
                    oldMarginLeft = "0px";
                if (oldMarginTop == null)
                    oldMarginTop = "0px";
                //calculate the new position by incrementing the current position by the difference between the current and the starting mouse position.
                //the starting position should be set in the MouseDown Event
                int newMarginLeft = Int16.Parse((oldMarginLeft).Replace("px", "")) + e.OffsetMousePosition.X - dragStartPosition.X;
                int newMarginTop = Int16.Parse((oldMarginTop).Replace("px", "")) + e.OffsetMousePosition.Y - dragStartPosition.Y;
                draggedDomElement.style.marginLeft = newMarginLeft + "px";
                draggedDomElement.style.marginTop = newMarginTop + "px";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chartMouseDown(object sender, HtmlElementEventArgs e)
        {
            //the augmentation in the current Scene which shares the same ID with the sender is selected
            AbstractAugmentation aug = trackable.Augmentations.Find(augmentation => { return augmentation.ID == ((HtmlElement)sender).Id; });
            //the augmentation can be clicked on, which means 
            ew.Cmb_editor_properties_objectSelection.SelectedItem = aug;
            if(e.MouseButtonsPressed == MouseButtons.Right)
            {
                Chart chart = (Chart)aug;
                if(chart.Source == null)
                {
                    chartContextMenu_noSource.Show(ew.BackgroundPanel, e.ClientMousePosition);
                } else if(chart.Source is FileSource)
                {
                    if(((FileSource)chart.Source).Query == null)
                    {
                        chartContextMenu_FileSource_noQuery.Show(ew.BackgroundPanel, e.ClientMousePosition);
                    }
                    else
                    {
                        chartContextMenu_FileSource_Query.Show(ew.BackgroundPanel, e.ClientMousePosition);
                    }
                } else {
                    chartContextMenu_DbSource.Show(ew.BackgroundPanel, e.ClientMousePosition);
                }
            }
            if(e.MouseButtonsPressed == MouseButtons.Left)
            {
                dragStartPosition = e.MousePosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void handleDocumentMouseDown(object sender, HtmlElementEventArgs e)
        {
            HtmlElement clickedElement = htmlPreview.Document.GetElementFromPoint(e.MousePosition);
            switch (clickedElement.GetAttribute("title"))
            {
                case "containment-wrapper":
                    if (e.MouseButtonsPressed == MouseButtons.Right)
                    {
                        mainContainerContextMenu.Show(ew.BackgroundPanel, e.MousePosition);
                    }
                    break;

                case "trackable":
                    //used this, because it sets the right name and triggers setCurrentElement
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = trackable;
                    //setCurrentElement(trackable);
                    if (e.MouseButtonsPressed == MouseButtons.Right)
                    {
                        trackableContextMenu.Show(ew.BackgroundPanel, e.MousePosition);
                    }
                    if (e.MouseButtonsPressed == MouseButtons.Left)
                    {
                        dragStartPosition = e.MousePosition;
                    } 
                    break;

                case "augmentation":
                    AbstractAugmentation aug = trackable.Augmentations.Find(augmentation => { return augmentation.ID == clickedElement.Id; });
                    //used this, because it sets the right name and triggers setCurrentElement
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = aug;
                    if (e.MouseButtonsPressed == MouseButtons.Right)
                    {
                        if (aug is Abstract2DAugmentation)
                        {
                            if (aug is Chart)
                            {
                                return;
                            } 
                            else 
                            {
                                augmentationContextMenu.Show(ew.BackgroundPanel, e.MousePosition);
                            }
                        }
                    }
                    if (e.MouseButtonsPressed == MouseButtons.Left)
                    {
                        htmlPreview.Document.MouseMove += dragHandler;
                        dragStartPosition = e.MousePosition;
                    } 
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dragHandler(object sender, HtmlElementEventArgs e)
        {
            HtmlElement draggedElement = htmlPreview.Document.GetElementFromPoint(e.MousePosition);
            if (e.MouseButtonsPressed == MouseButtons.Left && draggedElement.GetAttribute("title") == "augmentation")
            {
                IHTMLElement draggedDomElement = ((IHTMLElement)draggedElement.DomElement);
                Debug.Print(String.Format("MousePosX = {0}; StartPositionX = {1}; currentMarginLeft = {2}\n", e.MousePosition.X, dragStartPosition.X, (float)draggedDomElement.offsetLeft));
                Debug.Print(String.Format("MousePosY = {0}; StartPositionY = {1}; currentMarginTop = {2}\n", e.MousePosition.Y, dragStartPosition.Y, (float)draggedDomElement.offsetTop));
                int newMarginLeft = (e.MousePosition.X - dragStartPosition.X) + draggedDomElement.offsetLeft;
                int newMarginTop = (e.MousePosition.Y - dragStartPosition.Y) + draggedDomElement.offsetTop;
                //if (newMarginLeft >= 0 && newMarginLeft + draggedElement.ClientRectangle.Width < getMainContainerSize().Width)
                draggedDomElement.style.marginLeft = newMarginLeft + "px";

                //if (newMarginTop >= 0 && newMarginTop + draggedElement.ClientRectangle.Height < getMainContainerSize().Height)
                draggedDomElement.style.marginTop = newMarginTop + "px";

                dragStartPosition = e.MousePosition;
            }       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="index"></param>
        public void createNewScenewithTrackable(AbstractTrackable track, int index)
        {
            this.htmlPreview.Navigate("http:localhost:" + PREVIEW_PORT + "/" + index);
            this.index = index;
            this.trackable = null;
            ew.project.Trackables.Add(trackable);
            addPreviewable(track, new Vector3D(0, 0, 0));
            List<AbstractAugmentation> augs = new List<AbstractAugmentation>(track.Augmentations);
            track.Augmentations.Clear();
            foreach (AbstractAugmentation aug in augs)
            {
                addPreviewable(aug, aug.Translation);
            }
        }

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

            //adds a trackable to an empty scene
            if (currentElement is AbstractTrackable && trackable == null)
            {
                this.ew.Tsm_editor_menu_edit_delete.Enabled = true;
                Vector3D center = new Vector3D(0, 0, 0);
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
                            if (!this.addElement(currentElement, center))
                                return;
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
                            updateElementCombobox(trackable);
                           
                            ew.Cmb_editor_properties_objectSelection.SelectedItem = currentElement;                           
                            break;
                        }
                    }
                }
            }
            else if (currentElement is AbstractAugmentation && trackable != null && trackable.Augmentations.Count < 3)
            {
                bool isInitOk = currentElement.initElement(ew);
                if (isInitOk)
                {
                    //check if the currentElement can be added to the scene
                    if (!this.addElement(currentElement, v))
                        return;
                    //set references 
                    trackable.Augmentations.Add((AbstractAugmentation)currentElement);
                    ((AbstractAugmentation)currentElement).Trackable = this.trackable;
                    updateElementCombobox(trackable);
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = currentElement;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalPath"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private string addToFileSystemWithoutNotification(string originalPath, IPreviewable element)
        {
            if (!Path.IsPathRooted(originalPath))
                originalPath = ew.project.ProjectPath + "\\" + originalPath;

            fileWatcher.EnableRaisingEvents = false;
            string newName = Path.GetFileName(originalPath);
            while(fileLookup.ContainsKey(TEMP_PATH + newName))
            {
                NewNameInput NameDialog = new NewNameInput();                    
                if (NameDialog.ShowDialog(this.ew) == DialogResult.OK)
                {
                    // Read the contents of testDialog's TextBox.
                    newName = NameDialog.TextBox1.Text;
                }
                else
                {
                    fileWatcher.EnableRaisingEvents = true;
                    return null;
                }
                NameDialog.Dispose();                  
            }
            if(Helper.Copy(originalPath, TEMP_PATH, newName))
            {
                fileLookup.Add(TEMP_PATH + newName, element);
                fileWatcher.EnableRaisingEvents = true;
                return TEMP_PATH + newName;
            }
            else
            {
                fileWatcher.EnableRaisingEvents = true;
                return null;
            }
        }


        /// <summary>
        ///     adds elements to website and navigates again to new Website
        ///     adds the according preview to 
        /// </summary>
        /// <param name="currentElement"></param>
        /// <param name="vector"> a <see cref="Vector3D">Vector</see>, if the given <see cref="IPreviewable"/>Previewable</see> is
        /// a trackable, the vector contains Metaio Coordinates, because it is a native metaio Element.
        /// For the Html Augmentations it contains HTMLCoordinates</param>
        public bool addElement(IPreviewable currentElement, Vector3D vector)    
        {
            if (currentElement is Abstract2DTrackable)
            {
                Abstract2DTrackable trackable = ((Abstract2DTrackable)currentElement);
                HtmlElement htmlTrackable = htmlPreview.Document.CreateElement("img");
                int height, width;

                htmlTrackable.Id = trackable.SensorCosID;
                htmlTrackable.SetAttribute("title", "trackable");
                if (currentElement is IDMarker)
                {
                    height = width = trackable.Size;
                }
                else
                {
                    string newPath = null;
                    if(currentElement is PictureMarker)
                    {
                        //adding to tempFile
                        newPath = addToFileSystemWithoutNotification(((PictureMarker)currentElement).PicturePath, currentElement);
                        if(newPath == null)
                        {
                            MessageBox.Show("Es konnte keine Kopie der genutzten Bilddatei in " + TEMP_PATH + " angelegt werden");
                            return false;
                        }
                        ((PictureMarker)currentElement).PicturePath = newPath;
                    }
                    if(currentElement is ImageTrackable)
                    {
                        //adding to tempFile
                        newPath = addToFileSystemWithoutNotification(((ImageTrackable)currentElement).ImagePath, currentElement);
                        if (newPath == null)
                        {
                            MessageBox.Show("Es konnte keine Kopie der genutzten Bilddatei in "+TEMP_PATH+" angelegt werden");
                            return false;
                        }
                        ((ImageTrackable)currentElement).ImagePath = newPath;
                    }
                    height = trackable.HeightMM;
                    width = trackable.WidthMM;
                }               
                //tempPreview in WebsiteManager
                Bitmap preview;
                try
                {
                    preview = trackable.getPreview(ew.project.ProjectPath);
                }
                catch(System.IO.IOException e)
                {
                    MessageBox.Show(e.Message + " Versuchen Sie den Fehler zu beheben. Mit dem Drücken von ok, wird ein neuer Versuch gestartet.", "Fehler");
                    RemoveByValue<string, IPreviewable>(fileLookup, currentElement);
                    return addElement(currentElement, vector);
                }
                preview.Tag = trackable.SensorCosID;
                Websites.previews.Add(preview);
                htmlTrackable.SetAttribute("src", "http://localhost:" + PREVIEW_PORT + "/" + trackable.SensorCosID);
                htmlTrackable.Style = String.Format(@"width: {0}px; height: {1}px; z-index:{2}; margin-left: {3}px; margin-top: {4}px; position: absolute",
                width, height,
                vector.Z,
                vector.X + (ew.project.Screensize.Width - width)/2, vector.Y + (ew.project.Screensize.Height - height)/2);
                Websites.addElementAt(htmlTrackable, index);
                this.trackable = trackable;
            }
            else if (currentElement is Chart)
            {
                Chart chart = ((Chart)currentElement);
                this.setAugOnHtmlVector(chart, vector);
                HtmlElement htmlChart = htmlPreview.Document.CreateElement("div");
                htmlChart.SetAttribute("id", chart.ID);
                htmlChart.SetAttribute("class","augmentation");
                htmlChart.SetAttribute("title", "augmentation");
                htmlChart.Style = String.Format(@"width: {0}px; height: {1}px; margin-left: {2}px; margin-top: {3}px; position: absolute",
                chart.Width, chart.Height, chart.Positioning.Left, chart.Positioning.Top);
                if (chart.ResFilePath != null)
                {
                    string chartTempFolderPath = TEMP_PATH + chart.ID;
                    fileWatcher.EnableRaisingEvents = false;
                    if (!Directory.Exists(chartTempFolderPath))
                        Directory.CreateDirectory(chartTempFolderPath);
                    if(Path.IsPathRooted(chart.ResFilePath))
                        Helper.Copy(chart.ResFilePath, chartTempFolderPath, "options.js");
                    else
                        Helper.Copy(ew.project.ProjectPath + "\\" +chart.ResFilePath, chartTempFolderPath, "options.js");  
                    chart.ResFilePath = chartTempFolderPath + "\\options.js";
                    fileLookup.Add(chart.ResFilePath, currentElement);
                    Websites.chartFiles.Add("options"+chart.ID, File.ReadAllText(chart.ResFilePath));
                    fileWatcher.EnableRaisingEvents = true;
                    HtmlElement script = htmlPreview.Document.CreateElement("script");
                    ((IHTMLScriptElement)script.DomElement).type = "text/javascript";
                    ((IHTMLScriptElement)script.DomElement).text = "if (!window.console) console = {log: function() {}};\n "+chart.ID+"_object = { id : \"" + chart.ID + "\", options : {} };\n" +
                        "$.getScript(\"http://localhost:" + PREVIEW_PORT + "/options" + chart.ID + "\", function(){" + chart.ID + "_object.options = init(); $(\"#" + chart.ID + "\").highcharts(init());});\n";
                    if (chart.Source != null)
                    {
                        if (chart.Source.Query != null)
                        {
                            //copying the query
                            fileWatcher.EnableRaisingEvents = false;
                            if (Path.IsPathRooted(chart.Source.Query))
                                Helper.Copy(chart.Source.Query, chartTempFolderPath, "query.js");
                            else
                                Helper.Copy(Path.Combine(ew.project.ProjectPath, chart.Source.Query), chartTempFolderPath, "query.js");
                            chart.Source.Query = chartTempFolderPath + "\\query.js";
                            fileLookup.Add(chart.Source.Query, currentElement);
                            Websites.chartFiles.Add("query" + chart.ID, File.ReadAllText(chart.Source.Query));
                            fileWatcher.EnableRaisingEvents = true;

                            if (chart.Source is FileSource)
                            {
                                //copying the Data File and save on Server
                                fileWatcher.EnableRaisingEvents = false;
                                if (Path.IsPathRooted(((FileSource)chart.Source).Data))
                                    Helper.Copy(((FileSource)chart.Source).Data, chartTempFolderPath);
                                else
                                    Helper.Copy(Path.Combine(ew.project.ProjectPath, ((FileSource)chart.Source).Data), chartTempFolderPath);
                                ((FileSource)chart.Source).Data = Path.Combine(chartTempFolderPath, Path.GetFileName(((FileSource)chart.Source).Data));
                                fileLookup.Add(((FileSource)chart.Source).Data, currentElement);
                                string dataID = "data" + chart.ID + Path.GetExtension(((FileSource)chart.Source).Data);
                                Websites.chartFiles.Add(dataID, File.ReadAllText(((FileSource)chart.Source).Data));
                                fileWatcher.EnableRaisingEvents = true;
                                
                                //adding the loading to the Chart in the previewwith path to the data
                                ((IHTMLScriptElement)script.DomElement).text = ((IHTMLScriptElement)script.DomElement).text.Replace("$(\"#" + chart.ID + "\").highcharts(init());", "");
                                ((IHTMLScriptElement)script.DomElement).text += "$.getScript(\"http://localhost:" + PREVIEW_PORT + "/query" + chart.ID + "\", function(){query(\"http://localhost:" + PREVIEW_PORT + "/" + dataID + "\", " + chart.ID + "_object);});\n";
                            }
                            else
                            {
                                //adding the loading to the Chart in the preview with url to the data
                                ((IHTMLScriptElement)script.DomElement).text += "$.getScript(\"http://localhost:" + PREVIEW_PORT + "/query" + chart.ID + "\", function(){query(\"" + ((DbSource)chart.Source).Url + "\", " + chart.ID + "_object);});\n";   
                            }
                        }
                    }
                    htmlChart.AppendChild(script);
                }
                Websites.addElementAt(htmlChart, index);
            }
            else if (currentElement is HtmlImage)
            {
                HtmlImage image = ((HtmlImage)currentElement);
                
                HtmlElement htmlImage = htmlPreview.Document.CreateElement("img");
                //adding to tempFolder
                string newPath = addToFileSystemWithoutNotification(image.ResFilePath, image);
                if(newPath == null)
                {
                    MessageBox.Show("Es konnte keine Kopie der genutzten Bilddatei in " + TEMP_PATH + " angelegt werden");
                    return false;
                }
                image.ResFilePath = newPath;
                Bitmap preview = image.getPreview(ew.project.ProjectPath);
                htmlImage.Id = image.ID;
                htmlImage.SetAttribute("title", "augmentation");
                preview.Tag = image.ID;
                image.Width = preview.Width;
                image.Height = preview.Height;
                this.setAugOnHtmlVector(image, vector);
                htmlImage.SetAttribute("src", "http://localhost:" + PREVIEW_PORT + "/" + image.ID);
                htmlImage.Style = String.Format(@"width: {0}px; height: {1}px; z-index:{2}; margin-left: {3}px; margin-top: {4}px; position: absolute",
                image.Width, image.Height,
                vector.Z,
                image.Positioning.Left , image.Positioning.Top);
                Websites.addElementAt(htmlImage, index);
                Websites.previews.Add(preview);                
            } 
            else if (currentElement is HtmlVideo)
            {
                HtmlVideo video = ((HtmlVideo)currentElement);
                this.setAugOnHtmlVector(video, vector);
                HtmlElement htmlVideoPrev = htmlPreview.Document.CreateElement("img");
                //retrieving thumbnail for preview
                AForge.Video.FFMPEG.VideoFileReader reader = new AForge.Video.FFMPEG.VideoFileReader();
                reader.Open(video.ResFilePath);
                for (int i = 0; i < reader.FrameCount / 20; i++)
                { reader.ReadVideoFrame(); }
                Bitmap preview = reader.ReadVideoFrame();
                preview.Tag = video.ID;
                Websites.previews.Add(preview);

                //adding to tempFolder
                string newPath = addToFileSystemWithoutNotification(video.ResFilePath, video);
                if(newPath == null)
                {
                    MessageBox.Show("Es konnte keine Kopie der genutzten Videodatei in " + TEMP_PATH + " angelegt werden");
                    return false;
                }
                video.ResFilePath = newPath;
               
                htmlVideoPrev.Id = video.ID;
                htmlVideoPrev.SetAttribute("title", "augmentation");
                htmlVideoPrev.SetAttribute("src", "http://localhost:" + PREVIEW_PORT + "/" + video.ID);
                htmlVideoPrev.Style = String.Format(@"width: {0}px; height: {1}px; z-index:{2}; margin-left: {3}px; margin-top: {4}px; position: absolute",
                video.Width, video.Height,
                vector.Z,
                video.Positioning.Left, video.Positioning.Top);
                Websites.addElementAt(htmlVideoPrev, index);

            }
            else if (currentElement is GenericHtml)
            { 
                GenericHtml element = ((GenericHtml)currentElement);
                if (element.ResFilePath != null)
                {
                    string elementText;
                    if (Path.IsPathRooted(element.ResFilePath))
                        elementText = File.ReadAllText(element.ResFilePath);
                    else
                        elementText = File.ReadAllText(Path.Combine(ew.project.ProjectPath, element.ResFilePath));
                    int bracketsCount = 0;
                    foreach (char Char in elementText.ToCharArray())
	                {
		                if(Char.Equals('<'))++bracketsCount;
                        if(Char.Equals('>'))--bracketsCount;
	                }
                    if(bracketsCount == 0)
                    {
                        this.setAugOnHtmlVector(element, vector);
                        //get the ID and set as active element
                        System.Text.RegularExpressions.Regex idRex = new System.Text.RegularExpressions.Regex(@"\s*<\s*(?<tag>\S+)\s[^>]*id\s*=\s*""?(?<id>[^""\s]+)(""|\s)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        string elementId = idRex.Match(elementText).Groups["id"].Value;

                        if (changeAugIDFromTo(element, elementId)){                           
                            if (elementText.Contains("title"))
                            {
                                System.Text.RegularExpressions.Regex titleRex = new System.Text.RegularExpressions.Regex(@"title\s*=\s*""?[^""]*""?");
                                elementText = titleRex.Replace(elementText, @"title=""augmentation"" ", 1);
                            }
                            else
                            {
                                System.Text.RegularExpressions.Regex titleRex = new System.Text.RegularExpressions.Regex(@"(?<id>id\s*=\s*""?[^""\s]*""?)");
                                elementText = titleRex.Replace(elementText, @"${id} title=""augmentation"" ");
                            }
                            //adding to fileSystem and websitepage
                            string newPath =  addToFileSystemWithoutNotification(element.ResFilePath, element);
                            if (newPath == null)
                            {
                                MessageBox.Show("Es konnte keine Kopie der genutzten HTMLDatei in " + TEMP_PATH + " angelegt werden");
                                return false;
                            }
                            element.ResFilePath = newPath;
                            element.Tag = idRex.Match(elementText).Groups["tag"].Value;
                            Websites.insertRawTextElement(elementText, index);
                        }
                        else
                        {
                            MessageBox.Show("Es gibt die von ihnen vergebene id bereits");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Es gibt gibt eine ungleiche Anzahl öffnender und schließender Tags");
                    }
                } 
                else
                {
                    MessageBox.Show("sie müssen eine dazugehörige HTML Datei mit einem Element darin bestimmen");
                }
            } 
            else 
            {
                throw new NotSupportedException("Other then Abstract2DAugmentation/Abstract2DTrackable not yet supported");
            }
            reloadCurrentWebsite();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        private bool changeAugIDFromTo(AbstractAugmentation element, string newId)
        {
            foreach (AbstractTrackable t in ew.project.Trackables)
            {
                if(t != null)
                foreach (AbstractAugmentation a in t.Augmentations)
                {
                    if (a.ID == newId && a != element) return false;
                }
            }
            //if(!htmlPreview.IsBusy)
            //{
            //    foreach (HtmlElement htmlElement in htmlPreview.Document.All)
            //    {
            //        if (htmlElement.Id == newId) return false;
            //    }
            //}
            element.ID = newId;
            return true;
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

            if (currentElement is AbstractDynamic2DAugmentation)
            {
                if (this.trackable != null && trackable.existAugmentation((AbstractAugmentation)currentElement)
                    && ((AbstractDynamic2DAugmentation)currentElement).Source == null)
                {
                    if (source is FileSource)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\highcharts";
                        openFileDialog.Title = "Daten auswählen";
                        openFileDialog.Filter = "Data Files (*.json , *.xml)|*.json;*.xml";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            //set reference to the augmentations in Source
                            source.initElement(ew);
                            source.Augmentation = ((AbstractDynamic2DAugmentation)currentElement);

                            //string newDataPath = Path.Combine(Environment.CurrentDirectory, "tmp", source.Augmentation.ID);
                            //Helper.Copy(openFileDialog.FileName, newDataPath, "data" + Path.GetExtension(openFileDialog.FileName));
                            //((FileSource)source).Data = Path.Combine(newDataPath, "data" + Path.GetExtension(openFileDialog.FileName));

                            ((FileSource)source).Data = openFileDialog.FileName;
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
                                    //string newQueryPath = Path.Combine(Environment.CurrentDirectory, "tmp", source.Augmentation.ID);
                                    //Helper.Copy(openFileDialog.FileName, newQueryPath, "query.js");
                                    //((FileSource)source).Query = Path.Combine(newQueryPath, "query.js");
                                    ((FileSource)source).Query = openFileDialog.FileName;

                                    //Helper.Copy(Path.Combine(Environment.CurrentDirectory, "res","templates","chart(source).html"),newQueryPath,"chart.html");
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
                            string newQueryPath = Path.Combine(TEMP_PATH + source.Augmentation.ID);
                            fileWatcher.EnableRaisingEvents = false;
                            Helper.Copy(openFileDialog.FileName, newQueryPath, "query.js");
                            ((DbSource)source).Query = Path.Combine(newQueryPath, "query.js");
                            Websites.chartFiles.Add("query" + source.Augmentation.ID, File.ReadAllText(source.Query));
                            fileLookup.Add(((DbSource)source).Query, source.Augmentation);

                            //add references in Augmentation, Picturebox + project.sources List.
                            ((AbstractDynamic2DAugmentation)currentElement).Source = source;

                            this.reloadPreviewable(currentElement);
                            fileWatcher.EnableRaisingEvents = true;
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

            if (currentElement is Chart)
            {
                Chart chart = ((Chart)currentElement);
                chart.Source = null;
                this.ew.project.Sources.Remove(source);
                //TODO look into changes that may affect chart updates
                List<string> FilesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, source);
                foreach (string filepath in FilesToDelete)
                {
                    if (System.IO.File.Exists(filepath))
                        System.IO.File.Delete(filepath);
                }
                if (source is FileSource)
                {
                    Websites.chartFiles.Remove("data" + chart.ID + Path.GetExtension(((FileSource)source).Data));
                    if (source.Query != null)
                    {
                        Websites.chartFiles.Remove("query" + chart.ID);
                    }
                }
                else
                {
                    Websites.chartFiles.Remove("query" + chart.ID);
                }
                //this.findElement(currentElement).Image = this.getSizedBitmap(currentElement);
                //this.findElement(currentElement).Refresh();
            }
            updateElementCombobox(trackable);
        }

        /// <summary>
        /// Removes the Previewable and the object, what is linked to the Previewable.  
        /// </summary>
        /// <param name="currentElement">The current element.</param>
        public void removePreviewable(IPreviewable currentElement)
        {
            if (currentElement == null)
                throw new ArgumentException("parameter currentElement was null.");

            if (currentElement is AbstractTrackable && trackable != null)
            {
                List<string> FilesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, currentElement);
                foreach (string filepath in FilesToDelete)
                {
                    if(System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                }
                foreach(AbstractAugmentation aug in trackable.Augmentations)
                {
                    //release all Chart JS and JSON Files from Websitemanager
                    if (aug is Chart)
                    {
                        Chart chart = (Chart)aug;
                        Websites.chartFiles.Remove("options" + chart.ID);
                        if (chart.Source != null)
                        {
                            removeSource(chart.Source, chart);
                        }
                    }
                    //delete all pictures saved in memory from Websitemanager
                    if (aug is HtmlImage || aug is HtmlVideo)
                    {
                        Websites.previews.RemoveAll(pic => pic.Tag == ((AbstractHtmlElement)aug).ID);
                    }
                    //delete all Files associated with the deleted HtmlAugmentations
                    FilesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, aug);
                    foreach (string filepath in FilesToDelete)
                    {
                        if (System.IO.File.Exists(filepath))
                            System.IO.File.Delete(filepath);
                    }
                }
                Websites.resetPage(index);
                reloadCurrentWebsite();
                this.trackable = null;
                this.ew.project.Trackables[index] = null;
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
                //release all Chart JS and JSON Files from Websitemanager
                if(currentElement is Chart)
                {
                    Chart chart = (Chart)currentElement;
                    Websites.chartFiles.Remove("options"+chart.ID);
                    if(chart.Source != null)
                    {
                        removeSource(chart.Source, chart);                      
                    }
                }
                //delete all pictures saved in memory from Websitemanager
                if (currentElement is HtmlImage || currentElement is HtmlVideo)
                {
                    Websites.previews.RemoveAll(pic => pic.Tag == ((AbstractHtmlElement)currentElement).ID);
                }
                Websites.removeElementAt(findElement(currentElement), index);

                //delete Files and release from Filelookup
                List<string> FilesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, currentElement);
                foreach (string filepath in FilesToDelete)
                {
                    if (System.IO.File.Exists(filepath))
                        System.IO.File.Delete(filepath);
                }
                if(currentElement is Chart)
                {
                    string dirPath = TEMP_PATH + ((Chart)currentElement).ID;
                    if (System.IO.Directory.Exists(dirPath))
                        System.IO.Directory.Delete(dirPath, true);
                }
                reloadCurrentWebsite();
                this.ew.project.RemoveAugmentation((AbstractAugmentation)currentElement);
            }
            else if (currentElement is AbstractSource && trackable != null)
            {
                AbstractSource source = (AbstractSource)currentElement;
                removeSource(source, source.Augmentation);
            }
            updateElementCombobox(trackable);
            if(ew.CurrentElement == currentElement)
            {
                setCurrentElement(trackable);
                ew.PropertyGrid1.SelectedObject = trackable;
                ew.Cmb_editor_properties_objectSelection.SelectedItem = trackable;
            }
        }

        /// <summary>
        /// Removes all Elements from the PreviewPanel and clears all references and delete the trackable from the list.
        /// Then it reloads the CurrentWebsite and updates the combobox
        /// </summary>
        public void deleteCurrentScene()
        {
            Websites.deletePage(index);
            this.trackable = null;
            reloadCurrentWebsite();
            updateElementCombobox(trackable);
        }


        /// <summary>
        /// Sets current Page, chosen by index to the standard HTMLPreviewPage.
        /// Adds the current trackable to the Trackables of the project.
        /// </summary>
        public void cleanCurrentPageAndAddCurrentTrackable()
        {
            Websites.resetPage(index);
            this.ew.project.Trackables.Add(trackable);
            updateElementCombobox(trackable);
        }


        /// <summary>
        /// A static method which takes a Generic Dictionary and searches it for all Keys associated with the TValue.
        /// Then it deletes all the found Pairs from the Dictionary and return a List of the found Keys
        /// </summary>
        /// <typeparam name="TKey">A generic Type used as a Key in the dicitonary</typeparam>
        /// <typeparam name="TValue">A generic Type used as a Value in the dicitonary</typeparam>
        /// <param name="dictionary">A Dictionary which maps TKey to TValue</param>
        /// <param name="someValue">The TValue which should be found and every pair which includes it should be deleted from dictionary</param>
        /// <returns></returns>
        private static List<TKey> RemoveByValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue someValue)
        {
            List<TKey> itemsToRemove = new List<TKey>();

            foreach (var pair in dictionary)
            {
                if (pair.Value.Equals(someValue))
                    itemsToRemove.Add(pair.Key);
            }

            foreach (TKey item in itemsToRemove)
            {
                dictionary.Remove(item);
            }
            return itemsToRemove;
        }


        /// <summary>
        /// Renames a single previewable.
        /// </summary>
        /// <param name="ID">The new ID it should be named to</param>
        /// <param name="prev">The Previewable</param>
        public void renamePreviewable(IPreviewable prev, string ID)
        {
            //TODO check if Sources can be renamed too, and handle those cases
            //remove the HtmlElement from the Webpage and delete linked files in the filecache of the webserver
            List<string> filesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, prev);
            if (prev is Chart)
            {
                Chart chart = (Chart)prev;
                Websites.chartFiles.Remove("options" + chart.ID);
                if (chart.Source != null)
                {
                    AbstractSource source = chart.Source;
                    if (source is FileSource)
                    {
                        Websites.chartFiles.Remove("data" + chart.ID + Path.GetExtension(((FileSource)source).Data));
                    }
                    Websites.chartFiles.Remove("query" + chart.ID);
                }
            }
            if (prev is HtmlImage || prev is HtmlVideo)
            {
                Websites.previews.RemoveAll(pic => pic.Tag == ((AbstractHtmlElement)prev).ID);
            }
            if (prev is Abstract2DTrackable)
            {
                Websites.previews.RemoveAll(pic => pic.Tag == ((Abstract2DTrackable)prev).SensorCosID);
            }
            Websites.removeElementAt(findElement(prev), index);

            //rename the previewable
            if(prev is AbstractAugmentation)
            {
                ((AbstractAugmentation)prev).ID = ID;
            }
            else if (prev is Abstract2DTrackable)
            {
                ((Abstract2DTrackable)prev).SensorCosID = ID;
            }

            //add the Elements again to the Webpage, along with their corresponding files
            if (prev is Chart)
            {
                Chart chart = (Chart)prev;
                this.addElement(chart, new Vector3D(chart.Positioning.Left, chart.Positioning.Top, chart.Translation.Z));
                if (chart.Source != null)
                {
                    //addSource
                    AbstractSource source = chart.Source;
                }
            }
            else if (prev is AbstractHtmlElement)
            {
                AbstractHtmlElement elem = (AbstractHtmlElement)prev;
                this.addElement(prev, new Vector3D(elem.Positioning.Left, elem.Positioning.Top, elem.Translation.Z));
            }
            else if (prev is AbstractTrackable)
            {
                this.addElement(prev, new Vector3D(0, 0, 0));
            }

            if (typeof(AbstractDynamic2DAugmentation).IsAssignableFrom(prev.GetType()) && ((AbstractDynamic2DAugmentation)prev).Source != null)
            {
                reloadCurrentWebsite();
            }
            //if the same Files are referenced by the new elements they must not be deleted, thats why is is tested here
            fileWatcher.EnableRaisingEvents = false;
            foreach (string path in filesToDelete)
            {
                try
                {
                    if (path.Contains(TEMP_PATH) && !fileLookup.ContainsKey(path))
                        File.Delete(path);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.StackTrace + ":\n" + ex.Message);
                }
            }
            fileWatcher.EnableRaisingEvents = true;
        }



        /// <summary>
        /// Reloads a single previewable. deletes all not needed files associated with it 
        /// call this method if something changed, that need associated files to be renewed
        /// </summary>
        /// <param name="prev">The previewable.</param>
        public void reloadPreviewable(IPreviewable prev)
        {
            if (findElement(prev) == null)
                return;
            //make a List of all the associated Files which must be deleted if the previewable is altered
            List<string> filesToDelete = RemoveByValue<string, IPreviewable>(fileLookup, prev);
            if (prev is Chart)
            {
                Chart chart = (Chart)prev;
                Websites.chartFiles.Remove("options" + chart.ID);
                if (chart.Source != null)
                {
                    AbstractSource source = chart.Source;
                    if (source is FileSource)
                    {
                        Websites.chartFiles.Remove("data" + chart.ID + Path.GetExtension(((FileSource)source).Data));
                    }
                    Websites.chartFiles.Remove("query" + chart.ID);
                }
            }
            if (prev is HtmlImage || prev is HtmlVideo)
            {
                Websites.previews.RemoveAll(pic => pic.Tag == ((AbstractHtmlElement)prev).ID);
            }
            if (prev is Abstract2DTrackable)
            {
                Websites.previews.RemoveAll(pic => pic.Tag == ((Abstract2DTrackable)prev).SensorCosID);
            }
            Websites.removeElementAt(findElement(prev), index);
            if (prev is Chart)
            {
                Chart chart = (Chart)prev;
                this.addElement(chart, new Vector3D(chart.Positioning.Left, chart.Positioning.Top, chart.Translation.Z));
                if (chart.Source != null)
                {
                    ///addSource
                    AbstractSource source = chart.Source;
                }
            } else if(prev is AbstractHtmlElement)
            {
                AbstractHtmlElement elem = (AbstractHtmlElement)prev;
                this.addElement(prev, new Vector3D(elem.Positioning.Left, elem.Positioning.Top, elem.Translation.Z));
            }
            else if (prev is AbstractTrackable)
            {
                this.addElement(prev, new Vector3D(0, 0, 0));
            }

            if (typeof(AbstractDynamic2DAugmentation).IsAssignableFrom(prev.GetType()) && ((AbstractDynamic2DAugmentation)prev).Source != null)
            {
                reloadCurrentWebsite();
            }
            //if the same Files are referenced by the new elements they must not be deleted, thats why is is tested here
            fileWatcher.EnableRaisingEvents = false;
            foreach (string path in filesToDelete)
            {
                try
                {
                    if (path.Contains(TEMP_PATH) && !fileLookup.ContainsKey(path))
                        File.Delete(path);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.StackTrace + ":\n" + ex.Message);
                }
            }
            fileWatcher.EnableRaisingEvents = true;
        }


        /// <summary>
        /// Searchs in the HtmlPreview for the HtmlElement associated with the given IPreviewable.
        /// </summary>
        /// <param name="prev">The given IPreviewable, must not be null</param>
        /// <returns>the HtmlElement associated with prev, or null if it is not in the DOM or the DOM cant be evaluated</returns>
        /// <exception cref="System.ArgumentException">parameter prev was null.</exception>
        public HtmlElement findElement(IPreviewable prev)
        {
            if (prev == null)
                throw new ArgumentException("parameter prev was null.");
            HtmlElement element = null;
            //get the prevs unique identifier which differs with type, and searches for an HtmlElement with this ID
            if (prev is Abstract2DTrackable)
            {
                element = htmlPreview.Document.GetElementById(((Abstract2DTrackable)prev).SensorCosID);
            }
            else if (prev is AbstractAugmentation)
            {
                element = htmlPreview.Document.GetElementById(((AbstractAugmentation)prev).ID);
            }
            else if (prev is AbstractSource)
            {
                element = htmlPreview.Document.GetElementById(((AbstractSource)prev).Augmentation.ID);
            }
            return element;
        }

        /// <summary>
        /// Sets the currentElement in EditorWindow and marks the HtmlElement in the PreviewPanel by adding a border to it.
        /// </summary>
        /// <param name="currentElement">The current element, which should be marked and described by the PropertyPanel</param>
        public void setCurrentElement(IPreviewable currentElement)
        {
            //if there is a currentElement we musst set the box of the actual currentElement to normal
            //the box of the new currentElement will be set to the new Borderstyle.
            if (currentElement != null)
            {
                if (htmlPreview.IsBusy)
                {
                    markCurrentElementInPreview = true;
                }
                else
                {
                    if (this.ew.CurrentElement != currentElement)
                    {
                        //try to reset the borderstyle of the currentElement
                        if (this.ew.CurrentElement != null)
                        {
                            IHTMLElement previouslySelectedElement = this.ew.CurrentElement is AbstractSource ?
                                (IHTMLElement)findElement(((AbstractSource)ew.CurrentElement).Augmentation).DomElement :
                                (IHTMLElement)findElement(this.ew.CurrentElement).DomElement;
                            previouslySelectedElement.style.border = null;
                            if (currentElement is AbstractAugmentation)
                            {
                                previouslySelectedElement.style.zIndex = ((AbstractAugmentation)currentElement).Translation.Z;
                            }
                            else
                            {
                                previouslySelectedElement.style.zIndex = 0;
                            }
                        }

                        HtmlElement unselectedElement = null;
                        //set the elements to be shown in the propertygrid and in the EditorWindow to be the actual ones
                        ew.CurrentElement = currentElement;
                        ew.PropertyGrid1.SelectedObject = currentElement;
                        updateElementCombobox(trackable);
                        //next line is commented because this triggers another listener, which calls this setCurrentElement method
                        //ew.Cmb_editor_properties_objectSelection.SelectedItem = currentElement;

                        //if it is a source the corresponding chart has to be marked, but the source has to be shown in the property grid
                        if (currentElement is AbstractSource)
                        {
                            currentElement = ((AbstractSource)currentElement).Augmentation;
                        }

                        unselectedElement = findElement(currentElement);
                        //this means it could not be found, so its setting true for it to be marked in DocumentCompleted
                        if (unselectedElement == null)
                        {
                            markCurrentElementInPreview = true;
                        }
                        //this means it could be found and the dynamic properties to show the mark are set
                        else
                        {
                            IHTMLElement unselectedDomElement = (IHTMLElement)unselectedElement.DomElement;
                            unselectedDomElement.style.border = "solid 2.5px #F39814";
                            unselectedDomElement.style.zIndex = 10;
                        }
                        //Websites.removeElementAt(previouslySelectedElement, index);
                        //previouslySelectedElement.SetAttribute("class", "");
                        //Websites.addElementAt(previouslySelectedElement, index);
                        //}
                        //IHTMLElement unselectedElement = (IHTMLElement)findElement(currentElement).DomElement;
                        ////Websites.removeElementAt(unselectedElement, index);
                        //unselectedElement.style.border = "solid 2.5px #F39814";
                        //Websites.addElementAt(unselectedElement, index);
                        //if (currentElement is AbstractSource)
                        //{
                        //    this.ew.CurrentElement = ((AbstractSource)currentElement).Augmentation;
                        //}
                        //else
                        //{
                        //    this.ew.CurrentElement = currentElement;
                        //}

                        //if (this.ew.CurrentElement is AbstractAugmentation)
                        //{
                        //    this.ew.Tsm_editor_menu_edit_copie.Enabled = true;
                        //}
                        //else if (this.ew.CurrentElement is AbstractTrackable)
                        //{
                        //    this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
                        //}           
                        //}
                    }
                    //if there is no currentElement we'll mark the box and set the currentElement in EditorWindow.
                    //else
                    //{
                    //    if (this.ew.CurrentElement != null)
                    //    {
                    //        IHTMLElement previouslySelectedElement = (IHTMLElement)findElement(this.ew.CurrentElement).DomElement;
                    //        previouslySelectedElement.style.border = null;
                    //        this.ew.CurrentElement = null;
                    //    }
                    //    //Websites.deleteSelection(index);
                    //    this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
                    //}
                    if (this.ew.CurrentElement != null)
                    {
                        this.ew.Tsm_editor_menu_edit_copie.Enabled = this.ew.CurrentElement is AbstractAugmentation;
                        this.ew.Tsm_editor_menu_edit_delete.Enabled = true;
                    }
                }
            }
            //we want to set the currentElement to null, indicating no element is currently selected
            else
            {
                ew.CurrentElement = null;
                this.ew.Tsm_editor_menu_edit_delete.Enabled = false;
                this.ew.Tsm_editor_menu_edit_copie.Enabled = false;
            }
        }

        /// <summary>
        /// Recalculates the vector in dependency to panel.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>the new Vector in dependency to the panel</returns>
        private Vector3D recalculateVector(Vector3D v)
        {
            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (getMainContainerSize().Width / 2 + v.X * 1.6 * scale);
            result.Y = (getMainContainerSize().Height / 2 - v.Y * 1.6 * scale);
            return result;
        }

        private Vector3D recalculateChartVector(Vector3D v)
        {
            Vector3D result = new Vector3D(0, 0, 0);
            result.X = (getMainContainerSize().Width / 2 + v.X);
            result.Y = (getMainContainerSize().Height / 2 - v.Y);
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
        
        public void rescalePreviewPanel(uint width, uint height)
        {
            //int width = (int)this.getMainContainerSize().Width;
            //int height = (int)this.getMainContainerSize().Height;

            for (int i = 0; i < this.ew.project.Trackables.Count; i++)
			{
                AbstractTrackable trackable = this.ew.project.Trackables[i];
                if (trackable != null)
                {
                    HtmlElement trackElement = findElement(trackable);
                    if (trackElement != null)
                        Websites.changePositionOf(trackElement, i,
                        (height / 2 - trackElement.OffsetRectangle.Height / 2).ToString() + "px",
                        (width / 2 - trackElement.OffsetRectangle.Width / 2).ToString() + "px");
                    //foreach (AbstractAugmentation aug in trackable.Augmentations)
                    //{
                    //    if (aug is Chart)
                    //    {
                    //        ((Chart)aug).Positioning.Left = (int)((aug.Translation.X + getMainContainerSize().Width / 2));
                    //        ((Chart)aug).Positioning.Top = (int)((aug.Translation.Y + getMainContainerSize().Height / 2));
                    //    }
                    //}
                }
			}
        }

        /// <summary>
        /// Takes a Vector with HtmlCoordinates and changes the Translation Vector of the augmentation acordingly.
        /// </summary>
        /// <param name="aug">The <see cref="AbstractAugmentation">augmentation</see> which Coordinates should be altered according to newV.</param>
        /// <param name="newV">The new <see cref="Vector3D">vector</see> which contains HtmlCoordinates on which the aug shall be.</param>
        /// <exception cref="System.ArgumentException">
        /// parameter aug was null.
        /// or
        /// parameter newV was null.
        /// </exception>
        public void setAugOnHtmlVector(AbstractAugmentation aug, Vector3D newV)
        {
            if (aug == null)
                throw new ArgumentException("parameter aug was null.");
            if (newV == null)
                throw new ArgumentException("parameter newV was null.");


            if (aug is Chart)
            {
                Chart chart = (Chart)aug;
                chart.Positioning.Left = (int)newV.X;
                chart.Positioning.Top = (int)newV.Y;

                Vector3D result = new Vector3D(0, 0, chart.Translation.Z);
                result.X = (int)(newV.X - getMainContainerSize().Width / 2 + chart.Width / 2);
                result.Y = (int)(newV.Y - getMainContainerSize().Height / 2 + chart.Height / 2);
                chart.Translation = result;
            }
            else if (aug is AbstractHtmlElement)
            {

                AbstractHtmlElement elem = (AbstractHtmlElement)aug;
                elem.Positioning.Left = (int)newV.X;
                elem.Positioning.Top = (int)newV.Y;

                Vector3D result = new Vector3D(0, 0, elem.Translation.Z);
                result.X = (int)(newV.X - getMainContainerSize().Width / 2 + elem.Width / 2);
                result.Y = (int)(newV.Y - getMainContainerSize().Height / 2 + elem.Height / 2);
                elem.Translation = result;
            }
        }

        /// <summary>
        /// Takes a Vector with MetaioCoordinates and changes the HtmlPositioning of the augmentation acordingly.
        /// </summary>
        /// <param name="aug">The <see cref="AbstractAugmentation">augmentation</see> which Coordinates should be altered according to newV.</param>
        /// <param name="newV">The new <see cref="Vector3D">vector</see> which contains MetaioCoordinates on which the aug shall be.</param>
        /// <exception cref="System.ArgumentException">
        /// parameter aug was null.
        /// or
        /// parameter newV was null.
        /// </exception>
        public void setAugOnNativeVector(AbstractAugmentation aug, Vector3D newV)
        {
            if (aug == null)
                throw new ArgumentException("parameter aug was null.");
            if (newV == null)
                throw new ArgumentException("parameter newV was null.");

            if (aug is Chart)
            {
                Chart chart = (Chart)aug;
                chart.Translation.X = newV.X;
                chart.Translation.Y = newV.Y;

                HtmlPositioning result = new HtmlPositioning(chart.Positioning.PositioningMode);
                result.Left = (int)(newV.X + getMainContainerSize().Width / 2 - chart.Width / 2);
                result.Top = (int)(newV.Y + getMainContainerSize().Height / 2 - chart.Height / 2);
                chart.Positioning = result;
            }
            else if (aug is AbstractHtmlElement)
            {
                AbstractHtmlElement elem = (AbstractHtmlElement)aug;
                elem.Translation.X = newV.X;
                elem.Translation.Y = newV.Y;

                HtmlPositioning result = new HtmlPositioning(elem.Positioning.PositioningMode);
                result.Left = (int)(newV.X + getMainContainerSize().Width / 2 - elem.Width / 2);
                result.Top = (int)(newV.Y + getMainContainerSize().Height / 2 - elem.Height / 2);
                elem.Positioning = result;
            }
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
            HtmlElement element = this.findElement(prev);
            Bitmap imgOriginal = this.getSizedBitmap(prev);

            Bitmap tempBitmap = new Bitmap((int)(imgOriginal.Width), (int)(imgOriginal.Height));
            tempBitmap.SetResolution(imgOriginal.HorizontalResolution, imgOriginal.HorizontalResolution);
            System.Drawing.Graphics Graph = Graphics.FromImage(tempBitmap);
            Matrix X = new Matrix();
            X.RotateAt(grad, new Point((imgOriginal.Width / 2), (imgOriginal.Height / 2)));
            Graph.Transform = X;
            Graph.DrawImageUnscaled(imgOriginal, new Point(0, 0));

            X.Dispose();

            if(prev is Abstract2DAugmentation)
            {
                 tempBitmap.Save(((Abstract2DAugmentation)prev).ResFilePath);
            }
            htmlPreview.Navigate(htmlPreview.Url);
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
        /// Writes the back changesfrom DOM.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HtmlElementEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void writeBackChangesfromDOM(object sender, HtmlElementEventArgs e)
        {
            if (ew.CurrentElement != null)
            {
                while (htmlPreview.IsBusy)
                {
                    Application.DoEvents();
                }
                HtmlElement changedElement = findElement(ew.CurrentElement);
                if (changedElement.GetAttribute("title") == "augmentation")
                {
                    string newMarginLeft =  ((IHTMLElement)changedElement.DomElement).style.marginLeft;
                    string newMarginTop = ((IHTMLElement)changedElement.DomElement).style.marginTop;
                    float newLeft = ((IHTMLElement)changedElement.DomElement).offsetLeft;
                    float newTop = ((IHTMLElement)changedElement.DomElement).offsetTop;
                    Websites.changePositionOf(changedElement, index, newMarginTop ,newMarginLeft);
                    reloadCurrentWebsite();
                    setAugOnHtmlVector((AbstractAugmentation)ew.CurrentElement, 
                        new Vector3D(newLeft, newTop, ((AbstractAugmentation)ew.CurrentElement).Translation.Z));
                    ew.PropertyGrid1.SelectedObject = ew.CurrentElement;
                }
            }
            htmlPreview.Document.MouseMove -= dragHandler;
        }

        /// <summary>
        /// reloads the current Website by clearing the IEs Cache
        /// </summary>
        public void reloadCurrentWebsite()
        {
            markCurrentElementInPreview = true;
            htmlPreview.Refresh(WebBrowserRefreshOption.Completely);
            WebBrowserHelper.ClearCache();
            htmlPreview.Navigate(htmlPreview.Url);
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



        ///// <summary>
        /////  Event handler. Removes the source of the augmentation and the contextmenuentries of this augmentation.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //private void remove_source_by_click(object sender, EventArgs e)
        //{
        //    AbstractDynamic2DAugmentation temp = (AbstractDynamic2DAugmentation)((ContextMenu)((MenuItem)sender).Parent).Tag;

        //    this.findElement(temp).ContextMenu.MenuItems.RemoveAt(4);
        //    this.findElement(temp).ContextMenu.MenuItems.RemoveAt(4);
        //    this.findElement(temp).ContextMenu.MenuItems.RemoveAt(4);
        //    if (((AbstractDynamic2DAugmentation)this.ew.CurrentElement).Source is FileSource)
        //    {
        //        this.findElement(temp).ContextMenu.MenuItems.RemoveAt(3);
        //    }

        //    this.removeSource(temp.Source, temp);
        //    ew.PropertyGrid1.SelectedObject = null;
        //}

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
                mainContainerContextMenu.MenuItems[0].Enabled = true;
                this.ew.setPasteButtonEnabled();
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
                    IPreviewable element = (IPreviewable)this.copy.Clone();
                    Point htmlPos = getHtmlCoordinatesFromScreenPoint(Cursor.Position);
                    this.addPreviewable(element, new Vector3D(htmlPos.X, htmlPos.Y,((AbstractAugmentation)element).Translation.Z));
                    
                    if (element is AbstractDynamic2DAugmentation && ((AbstractDynamic2DAugmentation)element).Source != null)
                    {
                        //TODO add Source to copied chart
                    }
                }
            }
        }

        public Point getHtmlCoordinatesFromScreenPoint(Point point)
        {
            HtmlElement container = htmlPreview.Document.GetElementById("containment-wrapper");
            Rectangle offset = container.OffsetRectangle;
            point.X -= (htmlPreview.Document.Window.Position.X + offset.X);
            point.Y -= (htmlPreview.Document.Window.Position.Y + offset.Y);
            return point;
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
                    this.addPreviewable(element, new Vector3D(this.getMainContainerSize().Width / 2, this.getMainContainerSize().Height / 2, 0));

                    if (element is AbstractDynamic2DAugmentation && ((AbstractDynamic2DAugmentation)element).Source != null)
                    {
                        //TODO add Source to copied Element
                        ((AbstractDynamic2DAugmentation)element).Source = (AbstractSource)((AbstractDynamic2DAugmentation)copy).Source.Clone();
                    }
                }
            }
        }

        /**
         * <summary>    Raises the drag event when a source is dropped on an augmentation. </summary>
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
                HtmlElement element;
                if (htmlPreview.Document == null)
                    return;
                element = htmlPreview.Document.GetElementFromPoint(htmlPreview.PointToClient(new Point(e.X, e.Y)));
                if (element == null)
                    return;
                while (element.Parent != null && element.Parent.Id != "containment-wrapper")
                {
                    element = element.Parent;
                }
                AbstractAugmentation augmentation = trackable.Augmentations.Find(x => x.ID == element.Id);
                if (!(augmentation is Chart))
                    return;
                AbstractSource source = (AbstractSource)icon.Element.Prototype.Clone();
                addSource(source, augmentation);
            }
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
                path = ew.project.ProjectPath == null && Path.IsPathRooted(path) ? path : Path.Combine(ew.project.ProjectPath, path);
                TextEditorForm tef = new TextEditorForm(path);
                if (tef.ShowDialog() == DialogResult.OK)
                {
                    //reloadPreviewable((Chart)this.ew.CurrentElement);
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
                    //reloadPreviewable((Chart)this.ew.CurrentElement);
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

       
        public ScreenSize getMainContainerSize()
        {
            return this.ew.project.Screensize;
            //ScreenSize result = new ScreenSize();
            //result.Height = Convert.ToUInt32(htmlPreview.Document.GetElementById("containment-wrapper").ClientRectangle.Height);
            //result.Width = Convert.ToUInt32(htmlPreview.Document.GetElementById("containment-wrapper").ClientRectangle.Width);
            //return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public void setMainContainerSize(ScreenSize size)
        {
            Websites.changeMainContainerSize(size.Width, size.Height);
            rescalePreviewPanel(size.Width, size.Height);
            reloadCurrentWebsite();
        }

        /// <summary>
        /// Navigates the browser to the new address and changes the index and current trackable
        /// </summary>
        /// <param name="index"></param>
        public void changeSceneTo(int index)
        {
            htmlPreview.Navigate("http://localhost:" + PREVIEW_PORT + "/" + index);
            this.index = index;
            this.trackable = ew.project.Trackables[index];
            ew.Cmb_editor_properties_objectSelection.Items.Clear();
            updateElementCombobox(trackable);
            //in order to prevent setCurrentElement from trying to delete the mark on the 
            ew.CurrentElement = null;
            //compare the index before and after the change of the selected item, if the only element is null then there is no change in index and 
            //setCurrentElement must be triggered here, because the selectedIndex EventListener is not triggered, which would normally trigger setCurrentElement
            int index_pre = ew.Cmb_editor_properties_objectSelection.SelectedIndex;
            ew.Cmb_editor_properties_objectSelection.SelectedItem = trackable;
            if (index_pre == ew.Cmb_editor_properties_objectSelection.SelectedIndex)
                setCurrentElement(trackable);
        }
        
        private void shutDownWebserver()
        {
            Websites.Listener.Server.Close();
            if (!webServerThread.Join(1000)) // try to wait for it...
                webServerThread.Abort();
            Websites = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            //this listener keeps calling disposed Previewcontroller, so it must be released
            htmlPreview.DocumentCompleted -= htmlPreview_DocumentCompleted;
            htmlPreview.DocumentCompleted -= htmlPreview_DocumentFirstCompleted;
            htmlPreview.Document.MouseDown -= handleDocumentMouseDown;
            htmlPreview.Document.MouseUp -= writeBackChangesfromDOM;
            htmlPreview = null;
            this.ew.Tsm_editor_menu_edit_paste.Click -= new System.EventHandler(this.paste_augmentation_center);
            this.ew.Tsm_editor_menu_edit_copie.Click -= new System.EventHandler(this.copy_augmentation);
            this.ew.Tsm_editor_menu_edit_delete.Click -= new System.EventHandler(this.delete_current_element);
            fileWatcher.Changed -= fileWatcher_Changed;

            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            fileWatcher = null;

            if (Directory.Exists(TEMP_PATH))
                Directory.Delete(TEMP_PATH, true);

            if(Websites != null)
            shutDownWebserver();
            this.ew = null;
            this.webServerThread = null;
            this.disposed = true;
        }
    }
}