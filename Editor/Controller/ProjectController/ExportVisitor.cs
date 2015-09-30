using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

using ARdevKit.Model.Project;
using ARdevKit.Model.Project.File;

namespace ARdevKit.Controller.ProjectController
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An <see cref="ExportVisitor"/>  is an <see cref="AbstractProjectVisitor"/> which
    ///             exports the project to the path defined in <see cref="Project"/> so that it
    ///             is readable by the player. </summary>
    ///
    /// <remarks>   Imanuel, 15.01.2014. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class ExportVisitor : AbstractProjectVisitor
    {
        /// <summary>
        /// Gets or sets a value indicating whether [export is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [export is valid]; otherwise, <c>false</c>.
        /// </value>
        public bool ExportIsValid { get; set; }

        /// <summary>
        /// The exported <see cref="Project"/>.
        /// </summary>
        private Project project;

        /// <summary>   The <see cref="AbstractFile"/>s created by the export visitor. </summary>
        private List<AbstractFile> files = new List<AbstractFile>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the <see cref="AbstractFile"/>s created by the export visitor. </summary>
        ///
        /// <value> The files. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<AbstractFile> Files
        {
            get { return files; }
            set { files = value; }
        }

        /// <summary>   The <see cref="SceneFile"/>s created by the export visitor. </summary>
        private SceneFile[] sceneFiles = new SceneFile[10] ;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the <see cref="SceneFile"/>s created by the export visitor. </summary>
        ///
        /// <value> The files. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal SceneFile[] SceneFiles
        {
            get { return sceneFiles; }
            set { sceneFiles = value; }
        }



        /// <summary>   The arel glue file. </summary>
        private ARELGlueFile arelGlueFile;

        /// <summary>   The arel project file head block. </summary>
        private XMLBlock arelProjectFileHeadBlock;

        /// <summary>   The arel project file body block. </summary>
        private XMLBlock arelProjectFileBodyBlock;

        /// <summary>   The sensor block within the <see cref="trackingDataFile"/>. </summary>
        private XMLBlock trackingDataFileSensorBlock;
        /// <summary>   The sensor parameters block within the <see cref="trackingDataFile"/>. </summary>
        private XMLBlock trackingDataFileSensorParametersBlock;
        /// <summary>   The connections block within the <see cref="trackingDataFile"/>. </summary>
        private XMLBlock trackingDataFileConnectionsBlock;
        /// <summary>   The fuser block within the <see cref="trackingDataFile"/>. </summary>
        private XMLBlock trackingDataFileFuserBlock;

        /// <summary>   The counter for the COSes. </summary>
        private int cosCounter = 1;

        /// <summary>   The scene ready funktion block within the <see cref="arelGlueFile"/>. </summary>
        private JavaScriptBlock sceneReadyFunctionBlock;
        /// <summary>   if pattern is found block within the <see cref="arelGlueFile"/>. </summary>
        private JavaScriptBlock ifPatternIsFoundBlock;
        /// <summary>   if pattern is lost block within the <see cref="arelGlueFile"/>. </summary>
        private JavaScriptBlock ifPatternIsLostBlock;

        /// <summary>   The chart file parse block. </summary>
        private JavaScriptBlock chartFileCreateBlock;
        /// <summary>   The chart file parse block. </summary>
        private JavaScriptBlock chartFileQueryBlock;


        /// <summary>   Number of videoes. </summary>
        private int videoCount;
        /// <summary>   Number of images added to the <see cref="arelGlueFile"/>. </summary>
        private int imageCount;
        /// <summary>   Number of bar charts. </summary>
        private int chartCount;
        /// <summary>   Number of htmlImages. </summary>
        private int htmlImageCount;
        /// <summary>   Number of htmlVideos. </summary>
        private int htmlVideoCount;
        /// <summary>   Number of htmlGenerics. </summary>
        private int htmlGenericCount;
        /// <summary>   Identifier for the coordinate system. </summary>
        private int coordinateSystemID;
        /// <summary>   True if jquery was already imported.    </summary>
        private bool importedJQuery;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExportVisitor()
        {
            ExportIsValid = true;
            videoCount = 1;
            imageCount = 1;
            chartCount = 1;
            coordinateSystemID = 0;
        }
        public override void Visit(EventFile eventFile)
        {
            // Copy to projectPath
            if (!Directory.Exists(Path.Combine(project.ProjectPath, "Events")))
                Directory.CreateDirectory(Path.Combine(project.ProjectPath, "Events"));
            eventFile.FilePath = Path.Combine(project.ProjectPath, eventFile.FilePath);
            Files.Add(eventFile);

            if (!importedJQuery)
            {
                ExportIsValid = Helper.Copy("res\\jquery\\jquery-2.0.3.js", Path.Combine(project.ProjectPath, "Assets")) && ExportIsValid;
                arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/jquery-2.0.3.js\"")));
                importedJQuery = true;
            }
        }

        /// <summary>
        /// Visits the given <see cref="VideoAugmentation"/>
        /// </summary>
        /// <param name="video">The video</param>
        public override void Visit(VideoAugmentation video)
        {
            // Copy to projectPath
            string newPath = "Assets";
            if (video.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(video.ResFilePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, video.ResFilePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            video.ResFilePath = Path.Combine(newPath, Path.GetFileName(video.ResFilePath));

            // arelGlue.js
            string videoID = video.ID = video.ID == null ? "video" + videoCount : video.ID;

            arelGlueFile.AddBlock(new JavaScriptLine("var " + videoID));

            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            sceneReadyFunctionBlock.AddBlock(loadContentBlock);

            string videoPath = Path.GetFileNameWithoutExtension(video.ResFilePath) + Path.GetExtension(video.ResFilePath);
            loadContentBlock.AddLine(new JavaScriptLine(videoID + " = arel.Object.Model3D.createFromMovie(\"" + videoID + "\",\"Assets/" + videoPath + "\")"));
            loadContentBlock.AddLine(new JavaScriptLine(videoID + ".setVisibility(" + video.IsVisible.ToString().ToLower() + ")"));
            loadContentBlock.AddLine(new JavaScriptLine(videoID + ".setCoordinateSystemID(" + coordinateSystemID + ")"));
            string augmentationScalingX = video.Scaling.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationScalingY = video.Scaling.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationScalingZ = video.Scaling.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine(videoID + ".setScale(new arel.Vector3D(" + augmentationScalingX + "," + augmentationScalingY + "," + augmentationScalingZ + "))"));
            string augmentationTranslationX = video.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationTranslationY = video.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationTranslationZ = video.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine(videoID + ".setTranslation(new arel.Vector3D(" + augmentationTranslationX + "," + augmentationTranslationY + "," + augmentationTranslationZ + "))"));
            string augmentationRotationX = video.Rotation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationY = video.Rotation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationZ = video.Rotation.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine("var " + videoID + "Rotation = new arel.Rotation()"));
            loadContentBlock.AddLine(new JavaScriptLine(videoID + "Rotation.setFromEulerAngleDegrees(new arel.Vector3D(" + augmentationRotationX + "," + augmentationRotationY + "," + augmentationRotationZ + "))"));
            loadContentBlock.AddLine(new JavaScriptLine(videoID + ".setRotation(" + videoID + "Rotation)"));

            if (video.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + videoID + "_events.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            loadContentBlock.AddLine(new JavaScriptLine("arel.Scene.addObject(" + videoID + ")"));

            ifPatternIsFoundBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + videoID + "\").setVisibility(true)"));
            ifPatternIsFoundBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + videoID + "\").startMovieTexture()"));

            ifPatternIsLostBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + videoID + "\").setVisibility(false)"));
            ifPatternIsLostBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + videoID + "\").pauseMovieTexture()"));

            videoCount++;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="ImageAugmentation"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="image">    The image. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(ImageAugmentation image)
        {
            // Copy to projectPath
            string newPath = "Assets";
            if (image.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(image.ResFilePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, image.ResFilePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            image.ResFilePath = Path.Combine(newPath, Path.GetFileName(image.ResFilePath));

            // arelGlue.js
            string imageID = image.ID = image.ID == null ? "image" + imageCount : image.ID;

            arelGlueFile.AddBlock(new JavaScriptLine("var " + imageID));

            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            sceneReadyFunctionBlock.AddBlock(loadContentBlock);

            loadContentBlock.AddLine(new JavaScriptLine(imageID + " = arel.Object.Model3D.createFromImage(\"" + imageID + "\",\"Assets/" + Path.GetFileName(image.ResFilePath) + "\")"));
            loadContentBlock.AddLine(new JavaScriptLine(imageID + ".setVisibility(" + image.IsVisible.ToString().ToLower() + ")"));
            loadContentBlock.AddLine(new JavaScriptLine(imageID + ".setCoordinateSystemID(" + coordinateSystemID + ")"));
            string augmentationScalingX = image.Scaling.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationScalingY = image.Scaling.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationScalingZ = image.Scaling.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine(imageID + ".setScale(new arel.Vector3D(" + augmentationScalingX + "," + augmentationScalingY + "," + augmentationScalingZ + "))"));
            string augmentationTranslationX = image.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationTranslationY = image.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationTranslationZ = image.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine(imageID + ".setTranslation(new arel.Vector3D(" + augmentationTranslationX + "," + augmentationTranslationY + "," + augmentationTranslationZ + "))"));
            string augmentationRotationX = image.Rotation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationY = image.Rotation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationZ = image.Rotation.Z.ToString("F1", CultureInfo.InvariantCulture);
            loadContentBlock.AddLine(new JavaScriptLine("var " + imageID + "Rotation = new arel.Rotation()"));
            loadContentBlock.AddLine(new JavaScriptLine(imageID + "Rotation.setFromEulerAngleDegrees(new arel.Vector3D(" + augmentationRotationX + "," + augmentationRotationY + "," + augmentationRotationZ + "))"));
            loadContentBlock.AddLine(new JavaScriptLine(imageID + ".setRotation(" + imageID + "Rotation)"));

            if (image.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + imageID + "_events.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            loadContentBlock.AddLine(new JavaScriptLine("arel.Scene.addObject(" + imageID + ")"));

            ifPatternIsFoundBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + imageID + "\").setVisibility(true)"));
            ifPatternIsLostBlock.AddLine(new JavaScriptLine("arel.Scene.getObject(\"" + imageID + "\").setVisibility(false)"));

            imageCount++;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="Chart"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="chart">    The bar graph. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(Chart chart)
        {

            string chartID = chart.ID = chart.ID == null ? "chart" + chartCount : chart.ID;
            string chartPluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(chartID);
            SceneFile sceneFile = sceneFiles[coordinateSystemID-1];

            // arel[projectName].html
            if (chartCount == 1)
            {
                if (!importedJQuery)
                {
                    ExportIsValid = Helper.Copy("res\\jquery\\jquery-2.0.3.js", Path.Combine(project.ProjectPath, "Assets")) && ExportIsValid;
                    arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/jquery-2.0.3.js\"")));
                    importedJQuery = true;
                }

                ExportIsValid = Helper.Copy("res\\highcharts\\highcharts.js", Path.Combine(project.ProjectPath, "Assets")) && ExportIsValid;
                arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/highcharts.js\"")));
            }

            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/" + chartID + "/chart.js\"")));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js
            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            sceneFile.SceneReadyBlock.AddBlock(loadContentBlock);

            sceneFile.DefineBlock.AddLine(new JavaScriptInLine(chartID + " : " + chartPluginID, true));

            if (chart.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + chartID + "_Event.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            //loadContentBlock.AddLine(new JavaScriptLine(chartID + ".hide()"));

            // onTracking
            JavaScriptBlock chartIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!this." + chartID + ".created || this." + chartID + ".forceRecalculation)", new BlockMarker("{", "}"));
            sceneFile.CreateBlock.AddBlock(chartIfPatternIsFoundCreateBlock);
            chartIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine("this." + chartID + ".create(this.id)"));
            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.forceRecalculation |= this." + chartID + ".forceRecalculation"));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create chart.js
            ChartFile chartFile = new ChartFile(project.ProjectPath, chartID);
            files.Add(chartFile);

            JavaScriptBlock chartFileVariablesBlock = new JavaScriptBlock();

            JavaScriptBlock chartFileDefineBlock = new JavaScriptBlock(chartPluginID + " = ", new BlockMarker("{", "};"));
            chartFile.AddBlock(chartFileDefineBlock);

            // Ready
            chartFileDefineBlock.AddLine(new JavaScriptInLine("created : false", true));
            // Recalculate
            chartFileDefineBlock.AddLine(new JavaScriptInLine("forceRecalculation : " + chart.ForceRecalculation.ToString().ToLower(), true));
            // Visibility
            chartFileDefineBlock.AddLine(new JavaScriptInLine("visible : false", true));
            // ID
            chartFileDefineBlock.AddLine(new JavaScriptInLine("id : \"" + chartID + "\"", true));
            // CoordinateSystemID
            chartFileDefineBlock.AddLine(new JavaScriptInLine("coordinateSystemID : " + coordinateSystemID, true));
            // Options
            chartFileDefineBlock.AddLine(new JavaScriptInLine("options : {}", true));
            //// Translation
            //string translationX = chart.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            //string translationY = chart.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            //string translationZ = chart.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            //chartFileDefineBlock.AddBlock(new JavaScriptInLine("translation : new arel.Vector3D(" + translationX + "," + translationY + "," + translationZ + ")", true));
            // Chartelement
            chartFileDefineBlock.AddBlock(new JavaScriptInLine("element : document.createElement(\"div\")", true));

            // Create
            // DivElement
            chartFileCreateBlock = new JavaScriptBlock("create : function(sceneID)", new BlockMarker("{", "},"));
            chartFileDefineBlock.AddBlock(chartFileCreateBlock);
            chartFileCreateBlock.AddLine(new JavaScriptLine("this.created = true"));
            chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"id\", this.id)"));
            switch (chart.Positioning.PositioningMode)
            {
                case (HtmlPositioning.PositioningModes.STATIC):
                    chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"static\""));
                    break;
                case (HtmlPositioning.PositioningModes.ABSOLUTE):
                    chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"absolute\""));
                    break;
                case (HtmlPositioning.PositioningModes.RELATIVE):
                    chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"relative\""));
                    break;
            }
            if (chart.Positioning.Top > 0)
                chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.top = \"" + chart.Positioning.Top + "px\""));
            if (chart.Positioning.Left > 0)
                chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.left = \"" + chart.Positioning.Left + "px\""));

            chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.width = \"" + chart.Width + "px\""));
            chartFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.height = \"" + chart.Height + "px\""));
            chartFileCreateBlock.AddLine(new JavaScriptLine("document.getElementById(sceneID).appendChild(this.element)"));

            // Copy options.js
            string chartFilesDirectory = Path.Combine("Assets", chartID);
            if (chart.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(chart.ResFilePath, Path.Combine(project.ProjectPath, chartFilesDirectory), "options.js") && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, chart.ResFilePath), Path.Combine(project.ProjectPath, chartFilesDirectory), "options.js") && ExportIsValid;
            }
            chart.ResFilePath = Path.Combine(chartFilesDirectory, "options.js");

            // setOptions
            JavaScriptBlock chartFileDefineLoadOptionsBlock = new JavaScriptBlock("$.getScript(\"Assets/" + chartID + "/options.js\", function()", new BlockMarker("{", "})"));
            chartFileCreateBlock.AddBlock(chartFileDefineLoadOptionsBlock);
            chartFileCreateBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load options for " + chartID + "\")})", false));
            chartFileCreateBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded options for " + chartID + " successfully\")})"));

            chartFileDefineLoadOptionsBlock.AddLine(new JavaScriptLine(chartPluginID + ".options = init()"));
            chartFileDefineLoadOptionsBlock.AddLine(new JavaScriptLine("$('#' + " + chartPluginID + ".id).highcharts(" + chartPluginID + ".options)"));

            //// Show            
            //JavaScriptBlock chartShowBlock = new JavaScriptBlock("show : function()", new BlockMarker("{", "},"));
            //chartFileDefineBlock.AddBlock(chartShowBlock);
            //chartShowBlock.AddLine(new JavaScriptLine("$('#' + this.id).show()"));
            //chartShowBlock.AddLine(new JavaScriptLine("this.visible = true"));

            //// Hide
            //JavaScriptBlock chartHideBlock = new JavaScriptBlock("hide : function()", new BlockMarker("{", "},"));
            //chartFileDefineBlock.AddBlock(chartHideBlock);
            //chartHideBlock.AddLine(new JavaScriptLine("$('#' + this.id).hide()"));
            //chartHideBlock.AddLine(new JavaScriptLine("this.visible = false"));

            //// Get coordinateSystemID
            //JavaScriptBlock chartGetCoordinateSystemIDBlock = new JavaScriptBlock("getCoordinateSystemID : function()", new BlockMarker("{", "}"));
            //chartFileDefineBlock.AddBlock(chartGetCoordinateSystemIDBlock);
            //chartGetCoordinateSystemIDBlock.AddLine(new JavaScriptLine("return this.coordinateSystemID"));

            chartCount++;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="DbSource"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="source">   Source for the. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(DbSource source)
        {
            string chartID = source.Augmentation.ID;
            string chartPluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(chartID);
            string chartFilesDirectory = Path.Combine("Assets", chartID);

            if (source.Query != null && source.Query != "")
            {
                if (source.Query.Contains(':'))
                {
                    ExportIsValid = Helper.Copy(source.Query, Path.Combine(project.ProjectPath, chartFilesDirectory), "query.js") && ExportIsValid;
                }
                else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
                {
                    ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, source.Query), Path.Combine(project.ProjectPath, chartFilesDirectory), "query.js") && ExportIsValid;
                }
                source.Query = Path.Combine(chartFilesDirectory, "query.js");

                chartFileQueryBlock = new JavaScriptBlock("$.getScript(\"Assets/" + chartID + "/" + Path.GetFileName(source.Query) + "\", function()", new BlockMarker("{", "})"));
                chartFileCreateBlock.AddBlock(chartFileQueryBlock);
                chartFileCreateBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load query\")})", false));
                chartFileCreateBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded query successfully\")})"));
                chartFileQueryBlock.AddLine(new JavaScriptLine("var dataPath = \"" + source.Url + "\""));
                chartFileQueryBlock.AddLine(new JavaScriptLine("query(dataPath, " + chartPluginID + ")"));
            }
            else
                chartFileCreateBlock.AddLine(new JavaScriptLine("alert('No query defined')"));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="FileSource"/>. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="source">   Source for the <see cref="AbstractDynamic2DAugmentation"/>. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(FileSource source)
        {
            string chartID = source.Augmentation.ID;
            string chartPluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(chartID);
            string chartFilesDirectory = Path.Combine("Assets", chartID);

            if (source.Data != null && source.Data != "")
            {
                if (source.Query.Contains(':'))
                {
                    ExportIsValid = Helper.Copy(source.Data, Path.Combine(project.ProjectPath, chartFilesDirectory), "data" + Path.GetExtension(source.Data)) && ExportIsValid;
                }
                else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
                {
                    ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, source.Data), Path.Combine(project.ProjectPath, chartFilesDirectory), "data" + Path.GetExtension(source.Data)) && ExportIsValid;
                }
                source.Data = Path.Combine(chartFilesDirectory, "data" + Path.GetExtension(source.Data));

                if (source.Query != null && source.Query != "")
                {
                    if (source.Query.Contains(':'))
                    {
                        ExportIsValid = Helper.Copy(source.Query, Path.Combine(project.ProjectPath, chartFilesDirectory), "query.js");
                    }
                    else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
                    {
                        ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, source.Query), Path.Combine(project.ProjectPath, chartFilesDirectory), "query.js");
                    }
                    source.Query = Path.Combine(chartFilesDirectory, "query.js");

                    chartFileQueryBlock = new JavaScriptBlock("$.getScript(\"Assets/" + chartID + "/" + Path.GetFileName(source.Query) + "\", function()", new BlockMarker("{", "})"));
                    chartFileCreateBlock.AddBlock(chartFileQueryBlock);
                    chartFileCreateBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load query\")})", false));
                    chartFileCreateBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded query successfully\")})"));
                    chartFileQueryBlock.AddLine(new JavaScriptLine("var dataPath = \"Assets/" + chartID + "/data" + Path.GetExtension(source.Data) + "\""));
                    chartFileQueryBlock.AddLine(new JavaScriptLine("query(dataPath, " + chartPluginID + ")"));
                }
                else
                    chartFileCreateBlock.AddLine(new JavaScriptLine("alert('No query defined')"));
            }
            else
                chartFileCreateBlock.AddLine(new JavaScriptLine("alert('No source file defined')"));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerlessFuser"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="markerlessFuser">  The markerless fuser. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(MarkerlessFuser markerlessFuser)
        {
            // Fuser
            trackingDataFileFuserBlock.Update(new XMLTag("Fuser", "type=\"" + markerlessFuser.FuserType + "\""));

            // Parameters
            XMLBlock fuserParametersBlock = new XMLBlock(new XMLTag("Parameters"));
            trackingDataFileFuserBlock.AddBlock(fuserParametersBlock);

            string value = markerlessFuser.KeepPoseForNumberOfFrames.ToString();
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("KeepPoseForNumberOfFrames"), value));

            value = markerlessFuser.GravityAssistance.ToString();
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("GravityAssistance"), value));

            value = markerlessFuser.AlphaTranslation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("AlphaTranslation"), value));

            value = markerlessFuser.GammaTranslation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("GammaTranslation"), value));

            value = markerlessFuser.AlphaRotation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("AlphaRotation"), value));

            value = markerlessFuser.GammaRotation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("GammaRotation"), value));

            value = markerlessFuser.ContinueLostTrackingWithOrientationSensor.ToString().ToLower();
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("ContinueLostTrackingWithOrientationSensor"), value));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerlessSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="markerlessSensor"> The markerless sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(MarkerlessSensor markerlessSensor)
        {
            trackingDataFileSensorParametersBlock.AddLine(new XMLLine(new XMLTag("FeatureDescriptorAlignment"), markerlessSensor.FeatureDescriptorAlignment.ToString()));
            trackingDataFileSensorParametersBlock.AddLine(new XMLLine(new XMLTag("MaxObjectsToDetectPerFrame"), markerlessSensor.MaxObjectsToDetectPerFrame.ToString()));
            trackingDataFileSensorParametersBlock.AddLine(new XMLLine(new XMLTag("MaxObjectsToTrackInParallel"), markerlessSensor.MaxObjectsToTrackInParallel.ToString()));
            trackingDataFileSensorParametersBlock.AddLine(new XMLLine(new XMLTag("SimilarityThreshold"), markerlessSensor.SimilarityThreshold.ToString("F1", CultureInfo.InvariantCulture)));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerFuser"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="markerFuser">  The marker fuser. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(MarkerFuser markerFuser)
        {
            // Fuser
            trackingDataFileFuserBlock.Update(new XMLTag("Fuser", "type=\"" + markerFuser.FuserType + "\""));

            // Parameters
            XMLBlock fuserParametersBlock = new XMLBlock(new XMLTag("Parameters"));
            trackingDataFileFuserBlock.AddBlock(fuserParametersBlock);

            string value = markerFuser.AlphaRotation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("AlphaRotation"), value));

            value = markerFuser.AlphaTranslation.ToString("F1", CultureInfo.InvariantCulture);
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("AlphaTranslation"), value));

            value = markerFuser.KeepPoseForNumberOfFrames.ToString();
            fuserParametersBlock.AddLine(new XMLLine(new XMLTag("KeepPoseForNumberOfFrames"), value));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="PictureMarkerSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="pictureMarkerSensor">  The picture marker sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(PictureMarkerSensor pictureMarkerSensor)
        {
            // MarkerParameters
            XMLBlock markerTrackingParametersBlock = new XMLBlock(new XMLTag("MarkerTrackingParameters"));
            trackingDataFileSensorParametersBlock.AddBlock(markerTrackingParametersBlock);

            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("TrackingQuality"), pictureMarkerSensor.TrackingQuality.ToString()));
            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("ThresholdOffset"), pictureMarkerSensor.ThresholdOffset.ToString()));
            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("NumberOfSearchIterations"), pictureMarkerSensor.NumberOfSearchIterations.ToString()));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="Image"/>. </summary>
        ///
        /// <remarks>   Imanuel, 26.01.2014. </remarks>
        ///
        /// <param name="image">    The image. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(ImageTrackable image)
        {
            // Copy the file
            string newPath = "Assets";
            if (image.ImagePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(image.ImagePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, image.ImagePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            image.ImagePath = Path.Combine(newPath, Path.GetFileName(image.ImagePath));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // trackingData.xml

            XMLBlock sensorCOSBlock = new XMLBlock(new XMLTag("SensorCOS"));
            trackingDataFileSensorBlock.AddBlock(sensorCOSBlock);

            image.SensorCosID = IDFactory.CreateNewSensorCosID(image);
            sensorCOSBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), image.SensorCosID));

            XMLBlock parameterBlock = new XMLBlock(new XMLTag("Parameters"));
            sensorCOSBlock.AddBlock(parameterBlock);
            parameterBlock.AddLine(new XMLLine(new XMLTag("referenceImage", "WidthMM=\"" + image.WidthMM + "\" HeightMM=\"" + image.HeightMM + "\""), Path.GetFileName(image.ImagePath)));
            string value = image.SimilarityThreshold.ToString("F1", CultureInfo.InvariantCulture);
            parameterBlock.AddLine(new XMLLine(new XMLTag("SimilarityThreshold"), value));

            // Connections 

            // COS
            XMLBlock cosBlock = new XMLBlock(new XMLTag("COS"));
            trackingDataFileConnectionsBlock.AddBlock(cosBlock);

            // Name
            cosBlock.AddLine(new XMLLine(new XMLTag("Name"), project.Sensor.Name + "COS" + cosCounter++));

            // Fuser
            trackingDataFileFuserBlock = new XMLBlock(new XMLTag("Fuser"));
            cosBlock.AddBlock(trackingDataFileFuserBlock);

            // SensorSource
            XMLBlock sensorSourceBlock = new XMLBlock(new XMLTag("SensorSource"));
            cosBlock.AddBlock(sensorSourceBlock);

            // SensorID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorID"), project.Sensor.SensorIDString));
            // SensorCosID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), image.SensorCosID));

            // Hand-Eye-Calibration
            XMLBlock handEyeCalibrationBlock = new XMLBlock(new XMLTag("HandEyeCalibration"));
            sensorSourceBlock.AddBlock(handEyeCalibrationBlock);

            // Translation
            XMLBlock hecTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            handEyeCalibrationBlock.AddBlock(hecTranslationOffset);

            // TODO get vectors
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));

            // Rotation
            XMLBlock hecRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            handEyeCalibrationBlock.AddBlock(hecRotationOffset);

            // TODO get vectors
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("W"), "1"));

            // COSOffset
            XMLBlock COSOffsetBlock = new XMLBlock(new XMLTag("COSOffset"));
            sensorSourceBlock.AddBlock(COSOffsetBlock);

            // Translation
            XMLBlock COSOffsetTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetTranslationOffset);

            string augmentationPositionX = image.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationPositionY = image.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationPositionZ = image.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), augmentationPositionX));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), augmentationPositionY));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), augmentationPositionZ));

            // Rotation
            XMLBlock COSOffsetRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetRotationOffset);

            string augmentationRotationX = image.Rotation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationY = image.Rotation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationZ = image.Rotation.Z.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationW = image.Rotation.W.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("X"), augmentationRotationX));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), augmentationRotationY));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), augmentationRotationZ));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("W"), augmentationRotationW));

            coordinateSystemID++;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js

            // Set anchor
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.Flush();
            string anchorPath = Path.Combine(project.ProjectPath, "Assets", "anchor.png");
            if (!File.Exists(anchorPath))
                bmp.Save(anchorPath, System.Drawing.Imaging.ImageFormat.Png);

            // Add global variable for the anchor
            arelGlueFile.AddBlock(new JavaScriptLine("var COS" + coordinateSystemID + "Anchor"));

            // Add the anchor to the scene
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor = arel.Object.Model3D.createFromImage(\"COS" + coordinateSystemID + "Anchor" + "\",\"Assets/anchor.png" + "\")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setVisibility(false)"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setCoordinateSystemID(" + coordinateSystemID + ")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("arel.Scene.addObject(COS" + coordinateSystemID + "Anchor)"));

            // Create scene.js
            createSceneFileToCurrentCoordSysID();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="PictureMarker"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="pictureMarker">    The picture marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(PictureMarker pictureMarker)
        {
            // Copy the file
            string newPath = "Assets";
            if (pictureMarker.PicturePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(pictureMarker.PicturePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, pictureMarker.PicturePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            pictureMarker.PicturePath = Path.Combine(newPath, Path.GetFileName(pictureMarker.PicturePath));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // trackingData.xml

            XMLBlock sensorCOSBlock = new XMLBlock(new XMLTag("SensorCOS"));
            trackingDataFileSensorBlock.AddBlock(sensorCOSBlock);

            pictureMarker.SensorCosID = IDFactory.CreateNewSensorCosID(pictureMarker);
            sensorCOSBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), pictureMarker.SensorCosID));

            XMLBlock parameterBlock = new XMLBlock(new XMLTag("Parameters"));
            sensorCOSBlock.AddBlock(parameterBlock);

            XMLBlock markerParametersBlock = new XMLBlock(new XMLTag("MarkerParameters"));
            parameterBlock.AddBlock(markerParametersBlock);
            string value = pictureMarker.SimilarityThreshold.ToString("F1", CultureInfo.InvariantCulture);
            markerParametersBlock.AddLine(new XMLLine(new XMLTag("referenceImage", "WidthMM=\"" + pictureMarker.WidthMM + "\" HeightMM=\""+ pictureMarker.HeightMM + "\" qualityThreshold=\"" + value + "\""), Path.GetFileName(pictureMarker.PicturePath)));
            value = pictureMarker.SimilarityThreshold.ToString("F1", CultureInfo.InvariantCulture);
            parameterBlock.AddLine(new XMLLine(new XMLTag("SimilarityThreshold"), value));

            // Connections 

            // COS
            XMLBlock cosBlock = new XMLBlock(new XMLTag("COS"));
            trackingDataFileConnectionsBlock.AddBlock(cosBlock);

            // Name
            cosBlock.AddLine(new XMLLine(new XMLTag("Name"), project.Sensor.Name + "COS" + cosCounter++));

            // Fuser
            trackingDataFileFuserBlock = new XMLBlock(new XMLTag("Fuser"));
            cosBlock.AddBlock(trackingDataFileFuserBlock);

            // SensorSource
            XMLBlock sensorSourceBlock = new XMLBlock(new XMLTag("SensorSource", "trigger=\"1\""));
            cosBlock.AddBlock(sensorSourceBlock);

            // SensorID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorID"), project.Sensor.SensorIDString));
            // SensorCosID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), pictureMarker.SensorCosID));

            // Hand-Eye-Calibration
            XMLBlock handEyeCalibrationBlock = new XMLBlock(new XMLTag("HandEyeCalibration"));
            sensorSourceBlock.AddBlock(handEyeCalibrationBlock);

            // Translation
            XMLBlock hecTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            handEyeCalibrationBlock.AddBlock(hecTranslationOffset);

            // TODO get vectors
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));

            // Rotation
            XMLBlock hecRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            handEyeCalibrationBlock.AddBlock(hecRotationOffset);

            // TODO get vectors
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("W"), "1"));

            // COSOffset
            XMLBlock COSOffsetBlock = new XMLBlock(new XMLTag("COSOffset"));
            sensorSourceBlock.AddBlock(COSOffsetBlock);

            // Translation
            XMLBlock COSOffsetTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetTranslationOffset);

            string augmentationPositionX = pictureMarker.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationPositionY = pictureMarker.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationPositionZ = pictureMarker.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), augmentationPositionX));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), augmentationPositionY));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), augmentationPositionZ));

            // Rotation
            XMLBlock COSOffsetRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetRotationOffset);

            string augmentationRotationX = pictureMarker.Rotation.X.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationY = pictureMarker.Rotation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationZ = pictureMarker.Rotation.Z.ToString("F1", CultureInfo.InvariantCulture);
            string augmentationRotationW = pictureMarker.Rotation.W.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("X"), augmentationRotationX));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), augmentationRotationY));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), augmentationRotationZ));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("W"), augmentationRotationW));

            coordinateSystemID++;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js

            // Set anchor
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.Flush();
            string anchorPath = Path.Combine(project.ProjectPath, "Assets", "anchor.png");
            if (!File.Exists(anchorPath))
                bmp.Save(anchorPath, System.Drawing.Imaging.ImageFormat.Png);

            // Add global variable for the anchor
            arelGlueFile.AddBlock(new JavaScriptLine("var COS" + coordinateSystemID + "Anchor"));

            // Add the anchor to the scene
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor = arel.Object.Model3D.createFromImage(\"COS" + coordinateSystemID + "Anchor" + "\",\"Assets/anchor.png" + "\")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setVisibility(false)"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setCoordinateSystemID(" + coordinateSystemID + ")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("arel.Scene.addObject(COS" + coordinateSystemID + "Anchor)"));

            // Create scene.js
            createSceneFileToCurrentCoordSysID();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="idMarkerSensor">   The identifier marker sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(MarkerSensor idMarkerSensor)
        {
            // MarkerParameters
            XMLBlock markerTrackingParametersBlock = new XMLBlock(new XMLTag("MarkerTrackingParameters"));
            trackingDataFileSensorParametersBlock.AddBlock(markerTrackingParametersBlock);

            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("TrackingQuality"), idMarkerSensor.TrackingQuality.ToString()));
            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("ThresholdOffset"), idMarkerSensor.ThresholdOffset.ToString()));
            markerTrackingParametersBlock.AddLine(new XMLLine(new XMLTag("NumberOfSearchIterations"), idMarkerSensor.NumberOfSearchIterations.ToString()));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="IDMarker"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="idMarker"> The identifier marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(IDMarker idMarker)
        {
            // SensorCOS
            XMLBlock sensorCOSBlock = new XMLBlock(new XMLTag("SensorCOS"));
            trackingDataFileSensorBlock.AddBlock(sensorCOSBlock);

            //idMarker.SensorCosID = IDFactory.CreateNewSensorCosID(idMarker);
            sensorCOSBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), idMarker.SensorCosID));

            // Parameters
            XMLBlock parameterBlock = new XMLBlock(new XMLTag("Parameters"));
            sensorCOSBlock.AddBlock(parameterBlock);

            // MarkerParameters
            XMLBlock markerParametersBlock = new XMLBlock(new XMLTag("MarkerParameters"));
            parameterBlock.AddBlock(markerParametersBlock);

            markerParametersBlock.AddLine(new XMLLine(new XMLTag("Size"), idMarker.Size.ToString()));
            markerParametersBlock.AddLine(new XMLLine(new XMLTag("MatrixID"), idMarker.MatrixID.ToString()));

            // Connections 

            // COS
            XMLBlock cosBlock = new XMLBlock(new XMLTag("COS"));
            trackingDataFileConnectionsBlock.AddBlock(cosBlock);

            // Name
            cosBlock.AddLine(new XMLLine(new XMLTag("Name"), project.Sensor.Name + "COS" + cosCounter++));

            // Fuser
            trackingDataFileFuserBlock = new XMLBlock(new XMLTag("Fuser"));
            cosBlock.AddBlock(trackingDataFileFuserBlock);

            // SensorSource
            XMLBlock sensorSourceBlock = new XMLBlock(new XMLTag("SensorSource", "trigger=\"1\""));
            cosBlock.AddBlock(sensorSourceBlock);

            // SensorID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorID"), project.Sensor.SensorIDString));
            // SensorCosID
            sensorSourceBlock.AddLine(new XMLLine(new XMLTag("SensorCosID"), idMarker.SensorCosID));

            // Hand-Eye-Calibration
            XMLBlock handEyeCalibrationBlock = new XMLBlock(new XMLTag("HandEyeCalibration"));
            sensorSourceBlock.AddBlock(handEyeCalibrationBlock);

            // Translation
            XMLBlock hecTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            handEyeCalibrationBlock.AddBlock(hecTranslationOffset);

            // TODO get vectors
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));

            // Rotation
            XMLBlock hecRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            handEyeCalibrationBlock.AddBlock(hecRotationOffset);

            // TODO get vectors
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("X"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), "0"));
            hecRotationOffset.AddLine(new XMLLine(new XMLTag("W"), "1"));

            // COSOffset
            XMLBlock COSOffsetBlock = new XMLBlock(new XMLTag("COSOffset"));
            sensorSourceBlock.AddBlock(COSOffsetBlock);

            // Translation
            XMLBlock COSOffsetTranslationOffset = new XMLBlock(new XMLTag("TranslationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetTranslationOffset);

            string trackablePositionX = idMarker.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            string trackablePositionY = idMarker.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string trackablePositionZ = idMarker.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("X"), trackablePositionX));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Y"), trackablePositionY));
            COSOffsetTranslationOffset.AddLine(new XMLLine(new XMLTag("Z"), trackablePositionZ));

            // Rotation
            XMLBlock COSOffsetRotationOffset = new XMLBlock(new XMLTag("RotationOffset"));
            COSOffsetBlock.AddBlock(COSOffsetRotationOffset);

            string trackableRotationX = idMarker.Rotation.X.ToString("F1", CultureInfo.InvariantCulture);
            string trackableRotationY = idMarker.Rotation.Y.ToString("F1", CultureInfo.InvariantCulture);
            string trackableRotationZ = idMarker.Rotation.Z.ToString("F1", CultureInfo.InvariantCulture);
            string trackableRotationW = idMarker.Rotation.W.ToString("F1", CultureInfo.InvariantCulture);
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("X"), trackableRotationX));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Y"), trackableRotationY));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("Z"), trackableRotationZ));
            COSOffsetRotationOffset.AddLine(new XMLLine(new XMLTag("W"), trackableRotationW));

            coordinateSystemID++;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js

            // Set anchor
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.Flush();
            string anchorPath = Path.Combine(project.ProjectPath, "Assets", "anchor.png");
            if (!File.Exists(anchorPath))
                bmp.Save(anchorPath, System.Drawing.Imaging.ImageFormat.Png);

            // Add global variable for the anchor
            arelGlueFile.AddBlock(new JavaScriptLine("var COS" + coordinateSystemID + "Anchor"));

            // Add the anchor to the scene
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor = arel.Object.Model3D.createFromImage(\"COS" + coordinateSystemID + "Anchor" + "\",\"Assets/anchor.png" + "\")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setVisibility(false)"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("COS" + coordinateSystemID + "Anchor.setCoordinateSystemID(" + coordinateSystemID + ")"));
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("arel.Scene.addObject(COS" + coordinateSystemID + "Anchor)"));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create scene.js
            createSceneFileToCurrentCoordSysID();
        }

        private void createSceneFileToCurrentCoordSysID()
        {

            //the scene div which contains all augmentations
            string sceneID = "scene" + coordinateSystemID;
            string scenePluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sceneID);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // sceneX.js File
            SceneFile sceneFile = new SceneFile(project.ProjectPath, sceneID);
            if (coordinateSystemID < 10 && coordinateSystemID > 0 && sceneFiles[coordinateSystemID - 1] == null)
            {
                sceneFiles[coordinateSystemID - 1] = sceneFile;
                files.Add(sceneFile);
            }
            else
            {
                throw new NotSupportedException("There are only 10 scenes and one trackable per scene supported.");
            }

            JavaScriptBlock sceneFileVariablesBlock = new JavaScriptBlock();

            sceneFile.DefineBlock = new JavaScriptBlock(scenePluginID + " = ", new BlockMarker("{", "};"));
            sceneFile.AddBlock(sceneFile.DefineBlock);

            // Ready
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine("created : false", true));
            // Visibility
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine("visible : false", true));
            // Recalculate
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine("forceRecalculation : false", true));
            // ID
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine("id : \"" + sceneID + "\"", true));
            // CoordinateSystemID
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine("coordinateSystemID : " + coordinateSystemID, true));

            // scene div
            sceneFile.DefineBlock.AddBlock(new JavaScriptInLine("element : document.createElement(\"div\")", true));


            // Create
            // element
            sceneFile.CreateBlock = new JavaScriptBlock("create : function()", new BlockMarker("{", "},"));
            sceneFile.DefineBlock.AddBlock(sceneFile.CreateBlock);
            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.created = true"));
            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"id\", this.id)"));
            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"absolute\""));

            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.element.style.width = \"" + project.Screensize.Width + "px\""));
            sceneFile.CreateBlock.AddLine(new JavaScriptLine("this.element.style.height = \"" + project.Screensize.Height + "px\""));

            sceneFile.CreateBlock.AddLine(new JavaScriptLine("document.documentElement.appendChild(this.element)"));

            // Show            
            JavaScriptBlock sceneShowBlock = new JavaScriptBlock("show : function()", new BlockMarker("{", "},"));
            sceneFile.DefineBlock.AddBlock(sceneShowBlock);
            sceneShowBlock.AddLine(new JavaScriptLine("$('#' + this.id).show()"));
            sceneShowBlock.AddLine(new JavaScriptLine("this.visible = true"));

            // Hide
            JavaScriptBlock sceneHideBlock = new JavaScriptBlock("hide : function()", new BlockMarker("{", "},"));
            sceneFile.DefineBlock.AddBlock(sceneHideBlock);
            sceneHideBlock.AddLine(new JavaScriptLine("$('#' + this.id).hide()"));
            sceneHideBlock.AddLine(new JavaScriptLine("this.visible = false"));

            // Get coordinateSystemID
            JavaScriptBlock sceneGetCoordinateSystemIDBlock = new JavaScriptBlock("getCoordinateSystemID : function()", new BlockMarker("{", "},"));
            sceneFile.DefineBlock.AddBlock(sceneGetCoordinateSystemIDBlock);
            sceneGetCoordinateSystemIDBlock.AddLine(new JavaScriptLine("return this.coordinateSystemID"));

            // Get AugmentationsReady
            sceneFile.SceneReadyBlock = new JavaScriptBlock("getAugmentationsReady : function()", new BlockMarker("{", "}"));
            sceneFile.DefineBlock.AddBlock(sceneFile.SceneReadyBlock);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js
            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            sceneReadyFunctionBlock.AddBlock(loadContentBlock);

            arelGlueFile.AddBlock(new JavaScriptLine("var " + sceneID));

            loadContentBlock.AddLine(new JavaScriptLine(sceneID + " = " + scenePluginID));
            loadContentBlock.AddLine(new JavaScriptLine(sceneID + ".getAugmentationsReady()"));
            loadContentBlock.AddLine(new JavaScriptLine(sceneID + ".hide()"));

            // onTracking
            JavaScriptBlock sceneIfPatternIsFoundShowBlock = new JavaScriptBlock("if (param[0].getCoordinateSystemID() == " + sceneID + ".getCoordinateSystemID())", new BlockMarker("{", "}"));
            ifPatternIsFoundBlock.AddBlock(sceneIfPatternIsFoundShowBlock);
            JavaScriptBlock sceneIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!" + sceneID + ".created || " + sceneID + ".forceRecalculation)", new BlockMarker("{", "}"));
            sceneIfPatternIsFoundShowBlock.AddBlock(sceneIfPatternIsFoundCreateBlock);
            sceneIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine(sceneID + ".create()"));

            sceneIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine(sceneID + ".show()"));

            //moving is handled by the scene
            sceneIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine("arel.Scene.getScreenCoordinatesFrom3DPosition(COS" + coordinateSystemID + "Anchor.getTranslation(), " + sceneID + ".getCoordinateSystemID(), function(coord){move(COS" + coordinateSystemID + "Anchor, " + sceneID + ", coord);})"));

            // onTracking lost
            ifPatternIsLostBlock.AddLine(new JavaScriptLine(sceneID + ".hide()"));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arel[projectName].html to BODY TAG, because it must be loaded after the augmentations.js that belong to this scene
            arelProjectFileBodyBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/" + sceneID + ".js\"")));

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="Project"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="p">    The project. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Visit(Project p)
        {
            project = p;

            //Clean up before export
            if (project.OldProjectPath != project.ProjectPath)
            {
                try
                {
                    if (Directory.Exists(project.ProjectPath))
                        Directory.Delete(project.ProjectPath, true);
                    Directory.CreateDirectory(project.ProjectPath);
                }
                catch (Exception e)
                {
                    ExportIsValid = false;
                }
            }

            // Copy arel file
            ExportIsValid = Helper.Copy(Path.Combine("res", "arel", "arel.js"), project.ProjectPath) && ExportIsValid;

            // Create [projectName].html
            ARELProjectFile arelProjectFile = new ARELProjectFile("<!DOCTYPE html>", Path.Combine(project.ProjectPath, "arel" + (p.Name != "" ? p.Name : "Test") + ".html"));
            files.Add(arelProjectFile);

            // head
            arelProjectFileHeadBlock = new XMLBlock(new XMLTag("head"));
            arelProjectFile.AddBlock(arelProjectFileHeadBlock);

            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("title"), p.Name != "" ? p.Name : "Test"));

            arelProjectFileHeadBlock.AddLine(new XMLLine(new NonTerminatingXMLTag("meta", "charset=\"UTF-8\"")));
            arelProjectFileHeadBlock.AddLine(new XMLLine(new NonTerminatingXMLTag("meta", "name=\"viewport\" content=\"width=device-width, initial-scale=1\"")));

            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("title"), p.Name != null ? p.Name : "Test"));

            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "type=\"text/javascript\" src=\"arel.js\"")));
            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "type=\"text/javascript\" src=\"Assets/arelGlue.js\"")));

            // body
            arelProjectFileBodyBlock = new XMLBlock(new XMLTag("body"));
            arelProjectFile.AddBlock(arelProjectFileBodyBlock);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Prepare TrackinData.xml
            string trackingDataFileName = "TrackingData_" + p.Sensor.Name;
            trackingDataFileName += p.Sensor.SensorSubType != AbstractSensor.SensorSubTypes.None ? p.Sensor.SensorSubType.ToString() : "";
            trackingDataFileName += ".xml";
            TrackingDataFile trackingDataFile;
            trackingDataFile = new TrackingDataFile("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", project.ProjectPath, trackingDataFileName);
            files.Add(trackingDataFile);

            // TrackingData
            XMLBlock trackingDataBlock = new XMLBlock(new XMLTag("TrackingData"));
            trackingDataFile.AddBlock(trackingDataBlock);

            // Sensors
            XMLBlock sensorsBlock = new XMLBlock(new XMLTag("Sensors"));
            trackingDataBlock.AddBlock(sensorsBlock);

            // Sensors
            string sensorExtension = "Type=\"" + p.Sensor.SensorType + "\"";
            sensorExtension += p.Sensor.SensorSubType != AbstractSensor.SensorSubTypes.None ? " Subtype=\"" + p.Sensor.SensorSubType + "\"" : "";
            trackingDataFileSensorBlock = new XMLBlock(new XMLTag("Sensor", sensorExtension));
            sensorsBlock.AddBlock(trackingDataFileSensorBlock);

            // SensorID
            trackingDataFileSensorBlock.AddLine(new XMLLine(new XMLTag("SensorID"), p.Sensor.SensorIDString));

            // Parameters
            trackingDataFileSensorParametersBlock = new XMLBlock(new XMLTag("Parameters"));
            trackingDataFileSensorBlock.AddBlock(trackingDataFileSensorParametersBlock);

            // Connections
            trackingDataFileConnectionsBlock = new XMLBlock(new XMLTag("Connections"));
            trackingDataBlock.AddBlock(trackingDataFileConnectionsBlock);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create arelConfig.xml
            ARELConfigFile arelConfigFile = new ARELConfigFile("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", project.ProjectPath);
            files.Add(arelConfigFile);

            // Results
            XMLBlock resultsBlock = new XMLBlock(new XMLTag("results"));
            arelConfigFile.AddBlock(resultsBlock);

            // Trackingdata
            string trackingdataExtension = "channel=\"0\" poiprefix=\"extpoi-124906-\" url=\"Assets/" + trackingDataFileName + "\" /";
            resultsBlock.AddLine(new XMLLine(new NonTerminatingXMLTag("trackingdata", trackingdataExtension)));
            resultsBlock.AddLine(new XMLLine(new XMLTag("apilevel"), "7"));
            resultsBlock.AddLine(new XMLLine(new XMLTag("arel"), Path.GetFileName(arelProjectFile.FilePath)));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create arelGlue.js
            arelGlueFile = new ARELGlueFile(project.ProjectPath);
            files.Add(arelGlueFile);

            JavaScriptBlock sceneReadyBlock = new JavaScriptBlock("arel.sceneReady", new BlockMarker("(", ");"));
            arelGlueFile.AddBlock(sceneReadyBlock);
            sceneReadyFunctionBlock = new JavaScriptBlock("function()", new BlockMarker("{", "}"));
            sceneReadyBlock.AddBlock(sceneReadyFunctionBlock);

            // Console log ready
            sceneReadyFunctionBlock.AddLine(new JavaScriptLine("console.log(\"Scene ready\")"));

            // Set a listener to tracking to get information about when the image is tracked
            JavaScriptBlock eventListenerBlock = new JavaScriptBlock("arel.Events.setListener", new BlockMarker("(", ");"));
            sceneReadyFunctionBlock.AddBlock(eventListenerBlock);
            JavaScriptBlock eventListenreFunktionBlock = new JavaScriptBlock("arel.Scene, function(type, param)", new BlockMarker("{", "}"));
            eventListenerBlock.AddBlock(eventListenreFunktionBlock);

            eventListenreFunktionBlock.AddLine(new JavaScriptLine("trackingHandler(type, param)"));

            JavaScriptBlock trackingHandlerBlock = new JavaScriptBlock("function trackingHandler(type, param)", new BlockMarker("{", "};"));
            arelGlueFile.AddBlock(trackingHandlerBlock);

            // Tracking information availiable
            JavaScriptBlock ifTrackingInformationAvailiableBlock = new JavaScriptBlock("if(param[0] !== undefined)", new BlockMarker("{", "}"));
            trackingHandlerBlock.AddBlock(ifTrackingInformationAvailiableBlock);

            // Patternn found
            ifPatternIsFoundBlock = new JavaScriptBlock("if(type && type == arel.Events.Scene.ONTRACKING && param[0].getState() == arel.Tracking.STATE_TRACKING)", new BlockMarker("{", "}"));
            ifTrackingInformationAvailiableBlock.AddBlock(ifPatternIsFoundBlock);
            ifPatternIsFoundBlock.AddLine(new JavaScriptLine("console.log(\"Tracked coordinateSystemID: \" + param[0].getCoordinateSystemID())"));

            // Pattern lost
            ifPatternIsLostBlock = new JavaScriptBlock("else if(type && type == arel.Events.Scene.ONTRACKING && param[0].getState() == arel.Tracking.STATE_NOTTRACKING)", new BlockMarker("{", "}"));
            ifTrackingInformationAvailiableBlock.AddBlock(ifPatternIsLostBlock);
            ifPatternIsLostBlock.AddLine(new JavaScriptLine("console.log(\"Tracking lost\")"));

            // Move
            JavaScriptBlock arelGlueMoveBlock = new JavaScriptBlock("function move(anchor, object, coord)", new BlockMarker("{", "};"));
            arelGlueFile.AddBlock(arelGlueMoveBlock);
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("var oldLeft = object.element.style.left"));
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("var oldTop = object.element.style.top"));
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("var newLeft = (coord.getX() - parseInt(object.element.style.width) / 2)"));
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("var newTop = (coord.getY() - parseInt(object.element.style.height) / 2)"));
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("object.element.style.left = newLeft + 'px'"));
            arelGlueMoveBlock.AddBlock(new JavaScriptLine("object.element.style.top = newTop + 'px'"));

            JavaScriptBlock arelGlueMoveLogBlock = new JavaScriptBlock("if (object.element.style.left != oldLeft || object.element.style.top != oldTop)", new BlockMarker("{", "}"));
            arelGlueMoveBlock.AddBlock(arelGlueMoveLogBlock);
            arelGlueMoveLogBlock.AddBlock(new JavaScriptLine("console.log(\"Moved \" + object.id + \" from (\" + oldLeft + \", \" + oldTop + \") to (\" + object.element.style.left + \", \" + object.element.style.top + \")\")"));

            JavaScriptBlock arelGlueMoveTimeoutBlock = new JavaScriptBlock("if (object.visible)", new BlockMarker("{", "}"));
            arelGlueMoveBlock.AddBlock(arelGlueMoveTimeoutBlock);

            arelGlueMoveTimeoutBlock.AddLine(new JavaScriptLine("setTimeout(function() { arel.Scene.getScreenCoordinatesFrom3DPosition(anchor.getTranslation(), object.getCoordinateSystemID(), function(coord){move(anchor, object, coord);}); }, 100)"));
        }

        public override void Visit(HtmlImage htmlImage)
        {
            string htmlImageID = htmlImage.ID = htmlImage.ID == null ? "htmlImage" + htmlImageCount : htmlImage.ID;
            string htmlImagePluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(htmlImageID);

            //get corresponding sceneX.js
            SceneFile sceneFile = SceneFiles[coordinateSystemID - 1];

            // arel[projectName].html
            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/" + htmlImageID + "/htmlImage.js\"")));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js
            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            //write in the sceneFile instead of ArelGlue
            //sceneReadyFunctionBlock.AddBlock(loadContentBlock);
            sceneFile.SceneReadyBlock.AddBlock(loadContentBlock);

            //write in the sceneFile instead of ArelGlue
            //arelGlueFile.AddBlock(new JavaScriptLine("var " + htmlImageID));
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine(htmlImageID + " : " + htmlImagePluginID, true));

            //loadContentBlock.AddLine(new JavaScriptLine(htmlImageID + " = " + htmlImagePluginID));

            if (htmlImage.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + htmlImageID + "_Event.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            //scene handles hiding and showing
            //loadContentBlock.AddLine(new JavaScriptLine(htmlImageID + ".hide()"));

            // onTracking
            JavaScriptBlock htmlImageIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!this." + htmlImageID + ".created || this." + htmlImageID + ".forceRecalculation)", new BlockMarker("{", "}"));
            sceneFile.CreateBlock.AddBlock(htmlImageIfPatternIsFoundCreateBlock);
            htmlImageIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine("this." + htmlImageID + ".create(this.id)"));
            //JavaScriptBlock htmlImageIfPatternIsFoundShowBlock = new JavaScriptBlock("if (param[0].getCoordinateSystemID() == " + htmlImageID + ".getCoordinateSystemID())", new BlockMarker("{", "}"));
            //ifPatternIsFoundBlock.AddBlock(htmlImageIfPatternIsFoundShowBlock);
            //JavaScriptBlock htmlImageIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!" + htmlImageID + ".created || " + htmlImageID + ".forceRecalculation)", new BlockMarker("{", "}"));
            //htmlImageIfPatternIsFoundShowBlock.AddBlock(htmlImageIfPatternIsFoundCreateBlock);
            //htmlImageIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine(htmlImageID + ".create(\"scene" + coordinateSystemID + "\")"));

            //htmlImageIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine(htmlImageID + ".show()"));

            
            //moving is handled by the scene
            //if (htmlImage.Positioning.PositioningMode == HtmlPositioning.PositioningModes.RELATIVE)
            //    htmlImageIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine("arel.Scene.getScreenCoordinatesFrom3DPosition(COS" + coordinateSystemID + "Anchor.getTranslation(), " + htmlImageID + ".getCoordinateSystemID(), function(coord){move(COS" + coordinateSystemID + "Anchor, " + htmlImageID + ", coord);})"));

            // onTracking lost
            //ifPatternIsLostBlock.AddLine(new JavaScriptLine(htmlImageID + ".hide()"));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create htmlImage.js
            HtmlImageFile htmlImageFile = new HtmlImageFile(project.ProjectPath, htmlImageID);
            files.Add(htmlImageFile);

            JavaScriptBlock htmlImageFileVariablesBlock = new JavaScriptBlock();

            JavaScriptBlock htmlImageFileDefineBlock = new JavaScriptBlock(htmlImagePluginID + " = ", new BlockMarker("{", "};"));
            htmlImageFile.AddBlock(htmlImageFileDefineBlock);

            // Ready
            htmlImageFileDefineBlock.AddLine(new JavaScriptInLine("created : false", true));
            //// Visibility
            //htmlImageFileDefineBlock.AddLine(new JavaScriptInLine("visible : false", true));
            // ID
            htmlImageFileDefineBlock.AddLine(new JavaScriptInLine("id : \"" + htmlImageID + "\"", true));
            //// CoordinateSystemID
            //htmlImageFileDefineBlock.AddLine(new JavaScriptInLine("coordinateSystemID : " + coordinateSystemID, true));
            //// Translation in this Case Ignore translations and us positioning
            //string translationX = htmlImage.Translation.X.ToString("F1", CultureInfo.InvariantCulture);
            //string translationY = htmlImage.Translation.Y.ToString("F1", CultureInfo.InvariantCulture);
            //string translationZ = htmlImage.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            
            //commented because translation is no longer neeeded 
            //htmlImageFileDefineBlock.AddBlock(new JavaScriptInLine("translation : new arel.Vector3D(" + translationX + "," + translationY + "," + translationZ + ")", true));
            // htmlImage
            htmlImageFileDefineBlock.AddBlock(new JavaScriptInLine("element : document.createElement(\"img\")", true));

            // Copy image to projectPath
            string newPath = "Assets";
            if (htmlImage.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(htmlImage.ResFilePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, htmlImage.ResFilePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            htmlImage.ResFilePath = Path.Combine(newPath, Path.GetFileName(htmlImage.ResFilePath));

            // Create
            // element
            JavaScriptBlock htmlImageFileCreateBlock = new JavaScriptBlock("create : function(sceneID)", new BlockMarker("{", "},"));
            htmlImageFileDefineBlock.AddBlock(htmlImageFileCreateBlock);
            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.created = true"));
            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"id\", this.id)"));
            switch (htmlImage.Positioning.PositioningMode)
            {
                case (HtmlPositioning.PositioningModes.STATIC):
                    htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"static\""));
                    break;
                case (HtmlPositioning.PositioningModes.ABSOLUTE):
                case (HtmlPositioning.PositioningModes.RELATIVE):
                    htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"absolute\""));
                    break;
            }

            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.top = \"" + htmlImage.Positioning.Top + "px\""));
            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.left = \"" + htmlImage.Positioning.Left + "px\""));

            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.width = \"" + htmlImage.Width + "px\""));
            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.height = \"" + htmlImage.Height + "px\""));

            //set ImageBackground
            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("this.element.src =\"" + htmlImage.ResFilePath.Replace('\\', '/') + "\""));
            //TODO

            htmlImageFileCreateBlock.AddLine(new JavaScriptLine("document.getElementById(sceneID).appendChild(this.element)"));

            //// Show            
            //JavaScriptBlock htmlImageShowBlock = new JavaScriptBlock("show : function()", new BlockMarker("{", "},"));
            //htmlImageFileDefineBlock.AddBlock(htmlImageShowBlock);
            //htmlImageShowBlock.AddLine(new JavaScriptLine("$('#' + this.id).show()"));
            //htmlImageShowBlock.AddLine(new JavaScriptLine("this.visible = true"));

            //// Hide
            //JavaScriptBlock htmlImageHideBlock = new JavaScriptBlock("hide : function()", new BlockMarker("{", "},"));
            //htmlImageFileDefineBlock.AddBlock(htmlImageHideBlock);
            //htmlImageHideBlock.AddLine(new JavaScriptLine("$('#' + this.id).hide()"));
            //htmlImageHideBlock.AddLine(new JavaScriptLine("this.visible = false"));

            //// Get coordinateSystemID
            //JavaScriptBlock htmlImageGetCoordinateSystemIDBlock = new JavaScriptBlock("getCoordinateSystemID : function()", new BlockMarker("{", "}"));
            //htmlImageFileDefineBlock.AddBlock(htmlImageGetCoordinateSystemIDBlock);
            //htmlImageGetCoordinateSystemIDBlock.AddLine(new JavaScriptLine("return this.coordinateSystemID"));

            htmlImageCount++;
        }

        public override void Visit(HtmlVideo htmlVideo)
        {
            string htmlVideoID = htmlVideo.ID = htmlVideo.ID == null ? "htmlVideo" + htmlVideoCount : htmlVideo.ID;
            string htmlVideoPluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(htmlVideoID);

            //get corresponding sceneX.js
            SceneFile sceneFile = SceneFiles[coordinateSystemID - 1];

            // arel[projectName].html
            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/" + htmlVideoID + "/htmlVideo.js\"")));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js
            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            //sceneReadyFunctionBlock.AddBlock(loadContentBlock);
            sceneFile.SceneReadyBlock.AddBlock(loadContentBlock);

            //arelGlueFile.AddBlock(new JavaScriptLine("var " + htmlVideoID));
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine(htmlVideoID + " : " + htmlVideoPluginID, true));

            //loadContentBlock.AddLine(new JavaScriptLine(htmlVideoID + " = " + htmlVideoPluginID));

            if (htmlVideo.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + htmlVideoID + "_Event.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            //loadContentBlock.AddLine(new JavaScriptLine(htmlVideoID + ".hide()"));

            // onTracking
            JavaScriptBlock htmlImageIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!this." + htmlVideoID + ".created || this." + htmlVideoID + ".forceRecalculation)", new BlockMarker("{", "}"));
            sceneFile.CreateBlock.AddBlock(htmlImageIfPatternIsFoundCreateBlock);
            htmlImageIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine("this." + htmlVideoID + ".create(this.id)"));

            //JavaScriptBlock htmlVideoIfPatternIsFoundShowBlock = new JavaScriptBlock("if (param[0].getCoordinateSystemID() == " + htmlVideoID + ".getCoordinateSystemID())", new BlockMarker("{", "}"));
            //ifPatternIsFoundBlock.AddBlock(htmlVideoIfPatternIsFoundShowBlock);
            //JavaScriptBlock htmlVideoIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!" + htmlVideoID + ".created || " + htmlVideoID + ".forceRecalculation)", new BlockMarker("{", "}"));
            //htmlVideoIfPatternIsFoundShowBlock.AddBlock(htmlVideoIfPatternIsFoundCreateBlock);
            //htmlVideoIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine(htmlVideoID + ".create()"));

            //htmlVideoIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine(htmlVideoID + ".show()"));
            //if (htmlVideo.Positioning.PositioningMode == HtmlPositioning.PositioningModes.RELATIVE)
            //    htmlVideoIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine("arel.Scene.getScreenCoordinatesFrom3DPosition(COS" + coordinateSystemID + "Anchor.getTranslation(), " + htmlVideoID + ".getCoordinateSystemID(), function(coord){move(COS" + coordinateSystemID + "Anchor, " + htmlVideoID + ", coord);})"));

            //// onTracking lost
            //ifPatternIsLostBlock.AddLine(new JavaScriptLine(htmlVideoID + ".hide()"));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create htmlVideo.js
            HtmlVideoFile htmlVideoFile = new HtmlVideoFile(project.ProjectPath, htmlVideoID);
            files.Add(htmlVideoFile);

            JavaScriptBlock htmlVideoFileVariablesBlock = new JavaScriptBlock();

            JavaScriptBlock htmlVideoFileDefineBlock = new JavaScriptBlock(htmlVideoPluginID + " = ", new BlockMarker("{", "};"));
            htmlVideoFile.AddBlock(htmlVideoFileDefineBlock);

            // Ready
            htmlVideoFileDefineBlock.AddLine(new JavaScriptInLine("created : false", true));
            //// Visibility
            //htmlVideoFileDefineBlock.AddLine(new JavaScriptInLine("visible : false", true));
            // ID
            htmlVideoFileDefineBlock.AddLine(new JavaScriptInLine("id : \"" + htmlVideoID + "\"", true));
            //// CoordinateSystemID
            //htmlVideoFileDefineBlock.AddLine(new JavaScriptInLine("coordinateSystemID : " + coordinateSystemID, true));
            //// Translation in this Case Ignore translations and us positioning
            //string translationX = htmlVideo.Positioning.Left.ToString("F1", CultureInfo.InvariantCulture);
            //string translationY = htmlVideo.Positioning.Top.ToString("F1", CultureInfo.InvariantCulture);
            //string translationZ = htmlVideo.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            //htmlVideoFileDefineBlock.AddBlock(new JavaScriptInLine("translation : new arel.Vector3D(" + translationX + "," + translationY + "," + translationZ + ")", true));
            // htmlVideo
            htmlVideoFileDefineBlock.AddBlock(new JavaScriptInLine("element : document.createElement(\"video\")", true));

            // Create
            // element
            JavaScriptBlock htmlVideoFileCreateBlock = new JavaScriptBlock("create : function(sceneID)", new BlockMarker("{", "},"));
            htmlVideoFileDefineBlock.AddBlock(htmlVideoFileCreateBlock);
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.created = true"));
            //htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"id\", this.id)"));
            //switch (htmlVideo.Positioning.PositioningMode)
            //{
            //    case (HtmlPositioning.PositioningModes.STATIC):
            //        htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"static\""));
            //        break;
            //    case (HtmlPositioning.PositioningModes.ABSOLUTE):
            //    case (HtmlPositioning.PositioningModes.RELATIVE):
            //        htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"absolute\""));
            //        break;
            //}

            // Copy video to projectPath
            string newPath = "Assets";
            if (htmlVideo.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(htmlVideo.ResFilePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, htmlVideo.ResFilePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            htmlVideo.ResFilePath = Path.Combine(newPath, Path.GetFileName(htmlVideo.ResFilePath));


            //htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.width = \"" + htmlVideo.Width + "px\""));
            //htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.height = \"" + htmlVideo.Height + "px\""));

            //set ImageBackground
            //htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.src =\"" + htmlVideo.ResFilePath.Replace('\\', '/') + "\""));
            //TODO

            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position =\"absolute\""));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.top = \"" + htmlVideo.Positioning.Top + "px\""));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.left = \"" + htmlVideo.Positioning.Left + "px\""));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"width\", \"" + htmlVideo.Width + "px\")"));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"height\", \"" + htmlVideo.Height + "px\")"));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"controls\",true)"));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("this.element.innerHTML = \"<source src=\\\""+ htmlVideo.ResFilePath.Replace('\\', '/')+"\\\" type=\\\"video/mp4\\\">\""));
            htmlVideoFileCreateBlock.AddLine(new JavaScriptLine("document.getElementById(sceneID).appendChild(this.element)"));

            //// Show            
            //JavaScriptBlock htmlVideoShowBlock = new JavaScriptBlock("show : function()", new BlockMarker("{", "},"));
            //htmlVideoFileDefineBlock.AddBlock(htmlVideoShowBlock);
            //htmlVideoShowBlock.AddLine(new JavaScriptLine("$('#' + this.id).show()"));
            //htmlVideoShowBlock.AddLine(new JavaScriptLine("this.visible = true"));

            //// Hide
            //JavaScriptBlock htmlVideoHideBlock = new JavaScriptBlock("hide : function()", new BlockMarker("{", "},"));
            //htmlVideoFileDefineBlock.AddBlock(htmlVideoHideBlock);
            //htmlVideoHideBlock.AddLine(new JavaScriptLine("$('#' + this.id).hide()"));
            //htmlVideoHideBlock.AddLine(new JavaScriptLine("this.visible = false"));

            //// Get coordinateSystemID
            //JavaScriptBlock htmlVideoGetCoordinateSystemIDBlock = new JavaScriptBlock("getCoordinateSystemID : function()", new BlockMarker("{", "}"));
            //htmlVideoFileDefineBlock.AddBlock(htmlVideoGetCoordinateSystemIDBlock);
            //htmlVideoGetCoordinateSystemIDBlock.AddLine(new JavaScriptLine("return this.coordinateSystemID"));

            htmlVideoCount++;
        }

        public override void Visit(GenericHtml htmlGeneric)
        {
            string htmlGenericID = htmlGeneric.ID = htmlGeneric.ID == null ? "htmlGeneric" + htmlGenericCount : htmlGeneric.ID;
            string htmlGenericPluginID = "arel.Plugin." + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(htmlGenericID);

            //get corresponding sceneX.js
            SceneFile sceneFile = SceneFiles[coordinateSystemID - 1];

            // arel[projectName].html
            arelProjectFileHeadBlock.AddLine(new XMLLine(new XMLTag("script", "src=\"Assets/" + htmlGenericID + "/htmlGeneric.js\"")));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // arelGlue.js
            JavaScriptBlock loadContentBlock = new JavaScriptBlock();
            //sceneReadyFunctionBlock.AddBlock(loadContentBlock);
            sceneFile.SceneReadyBlock.AddBlock(loadContentBlock);

            //arelGlueFile.AddBlock(new JavaScriptLine("var " + htmlGenericID));
            sceneFile.DefineBlock.AddLine(new JavaScriptInLine(htmlGenericID + " : " + htmlGenericPluginID, true));
            
            //loadContentBlock.AddLine(new JavaScriptLine(htmlGenericID + " = " + htmlGenericPluginID));

            if (htmlGeneric.Events != null)
            {
                JavaScriptBlock loadEventsBlock = new JavaScriptBlock("$.getScript(\"Events/" + htmlGenericID + "_Event.js\", function()", new BlockMarker("{", "})"));
                loadContentBlock.AddBlock(loadEventsBlock);
                loadContentBlock.AddBlock(new JavaScriptInLine(".fail(function() { console.log(\"Failed to load events\")})", false));
                loadContentBlock.AddBlock(new JavaScriptLine(".done(function() { console.log(\"Loaded events successfully\")})"));
            }

            //loadContentBlock.AddLine(new JavaScriptLine(htmlGenericID + ".hide()"));

            // onTracking
            JavaScriptBlock htmlImageIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!this." + htmlGenericID + ".created || this." + htmlGenericID + ".forceRecalculation)", new BlockMarker("{", "}"));
            sceneFile.CreateBlock.AddBlock(htmlImageIfPatternIsFoundCreateBlock);
            htmlImageIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine("this." + htmlGenericID + ".create(this.id)"));
            
            //JavaScriptBlock htmlGenericIfPatternIsFoundShowBlock = new JavaScriptBlock("if (param[0].getCoordinateSystemID() == " + htmlGenericID + ".getCoordinateSystemID())", new BlockMarker("{", "}"));
            //ifPatternIsFoundBlock.AddBlock(htmlGenericIfPatternIsFoundShowBlock);
            //JavaScriptBlock htmlGenericIfPatternIsFoundCreateBlock = new JavaScriptBlock("if (!" + htmlGenericID + ".created || " + htmlGenericID + ".forceRecalculation)", new BlockMarker("{", "}"));
            //htmlGenericIfPatternIsFoundShowBlock.AddBlock(htmlGenericIfPatternIsFoundCreateBlock);
            //htmlGenericIfPatternIsFoundCreateBlock.AddLine(new JavaScriptLine(htmlGenericID + ".create()"));

            //htmlGenericIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine(htmlGenericID + ".show()"));
            //if (htmlGeneric.Positioning.PositioningMode == HtmlPositioning.PositioningModes.RELATIVE)
            //    htmlGenericIfPatternIsFoundShowBlock.AddLine(new JavaScriptLine("arel.Scene.getScreenCoordinatesFrom3DPosition(COS" + coordinateSystemID + "Anchor.getTranslation(), " + htmlGenericID + ".getCoordinateSystemID(), function(coord){move(COS" + coordinateSystemID + "Anchor, " + htmlGenericID + ", coord);})"));

            //// onTracking lost
            //ifPatternIsLostBlock.AddLine(new JavaScriptLine(htmlGenericID + ".hide()"));

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create htmlGeneric.js
            HtmlGenericFile htmlGenericFile = new HtmlGenericFile(project.ProjectPath, htmlGenericID);
            files.Add(htmlGenericFile);

            JavaScriptBlock htmlGenericFileVariablesBlock = new JavaScriptBlock();

            JavaScriptBlock htmlGenericFileDefineBlock = new JavaScriptBlock(htmlGenericPluginID + " = ", new BlockMarker("{", "};"));
            htmlGenericFile.AddBlock(htmlGenericFileDefineBlock);

            // Ready
            htmlGenericFileDefineBlock.AddLine(new JavaScriptInLine("created : false", true));
            //// Visibility
            //htmlGenericFileDefineBlock.AddLine(new JavaScriptInLine("visible : false", true));
            // ID
            htmlGenericFileDefineBlock.AddLine(new JavaScriptInLine("id : \"" + htmlGenericID + "\"", true));
            //// CoordinateSystemID
            //htmlGenericFileDefineBlock.AddLine(new JavaScriptInLine("coordinateSystemID : " + coordinateSystemID, true));
            //// Translation in this Case Ignore translations and us positioning
            //string translationX = htmlGeneric.Positioning.Left.ToString("F1", CultureInfo.InvariantCulture);
            //string translationY = htmlGeneric.Positioning.Top.ToString("F1", CultureInfo.InvariantCulture);
            //string translationZ = htmlGeneric.Translation.Z.ToString("F1", CultureInfo.InvariantCulture);
            //htmlGenericFileDefineBlock.AddBlock(new JavaScriptInLine("translation : new arel.Vector3D(" + translationX + "," + translationY + "," + translationZ + ")", true));
            // htmlGeneric
            htmlGenericFileDefineBlock.AddBlock(new JavaScriptInLine("element : document.createElement(\""+htmlGeneric.Tag+"\")", true));

            // Create
            // element
            JavaScriptBlock htmlGenericFileCreateBlock = new JavaScriptBlock("create : function(sceneID)", new BlockMarker("{", "},"));
            htmlGenericFileDefineBlock.AddBlock(htmlGenericFileCreateBlock);
            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.created = true"));
            //htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.setAttribute(\"id\", this.id)"));
            //switch (htmlGeneric.Positioning.PositioningMode)
            //{
            //    case (HtmlPositioning.PositioningModes.STATIC):
            //        htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"static\""));
            //        break;
            //    case (HtmlPositioning.PositioningModes.ABSOLUTE):
            //    case (HtmlPositioning.PositioningModes.RELATIVE):
            //        htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position = \"absolute\""));
            //        break;
            //}

            // Copy html to projectPath
            string newPath = "Assets";
            if (htmlGeneric.ResFilePath.Contains(':'))
            {
                ExportIsValid = Helper.Copy(htmlGeneric.ResFilePath, Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            else if (project.OldProjectPath != null && !project.OldProjectPath.Equals(project.ProjectPath))
            {
                ExportIsValid = Helper.Copy(Path.Combine(project.OldProjectPath, htmlGeneric.ResFilePath), Path.Combine(project.ProjectPath, newPath)) && ExportIsValid;
            }
            htmlGeneric.ResFilePath = Path.Combine(newPath, Path.GetFileName(htmlGeneric.ResFilePath));


            //htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.width = \"" + htmlGeneric.Width + "px\""));
            //htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.height = \"" + htmlGeneric.Height + "px\""));

            //set ImageBackground
            //htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.src =\"" + htmlGeneric.ResFilePath.Replace('\\', '/') + "\""));
            //TODO

            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("document.getElementById(sceneID).appendChild(this.element)"));
            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.outerHTML = \"" + File.ReadAllText(Path.Combine(project.ProjectPath, htmlGeneric.ResFilePath)).Replace("\"", "\\\"") + "\""));
            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.position =\"absolute\""));
            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.top = \"" + htmlGeneric.Positioning.Top + "px\""));
            htmlGenericFileCreateBlock.AddLine(new JavaScriptLine("this.element.style.left = \"" + htmlGeneric.Positioning.Left + "px\""));
            //// Show            
            //JavaScriptBlock htmlGenericShowBlock = new JavaScriptBlock("show : function()", new BlockMarker("{", "},"));
            //htmlGenericFileDefineBlock.AddBlock(htmlGenericShowBlock);
            //htmlGenericShowBlock.AddLine(new JavaScriptLine("$('#' + this.id).show()"));
            //htmlGenericShowBlock.AddLine(new JavaScriptLine("this.visible = true"));

            //// Hide
            //JavaScriptBlock htmlGenericHideBlock = new JavaScriptBlock("hide : function()", new BlockMarker("{", "},"));
            //htmlGenericFileDefineBlock.AddBlock(htmlGenericHideBlock);
            //htmlGenericHideBlock.AddLine(new JavaScriptLine("$('#' + this.id).hide()"));
            //htmlGenericHideBlock.AddLine(new JavaScriptLine("this.visible = false"));

            //// Get coordinateSystemID
            //JavaScriptBlock htmlGenericGetCoordinateSystemIDBlock = new JavaScriptBlock("getCoordinateSystemID : function()", new BlockMarker("{", "}"));
            //htmlGenericFileDefineBlock.AddBlock(htmlGenericGetCoordinateSystemIDBlock);
            //htmlGenericGetCoordinateSystemIDBlock.AddLine(new JavaScriptLine("return this.coordinateSystemID"));

            htmlGenericCount++;
        }
    }
}
