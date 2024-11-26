using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Newtonsoft.Json;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using VTC.Common;
using VTC.Kernel;
using VTC.Classifier;

namespace TrajectoryAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<Movement> trajectoriesList { get; private set; }
        public ObservableCollection<Movement> prototypeTrajectoriesList { get; private set; }
        public ObservableCollection<MatchWithExplanation> selectedPrototypeTrajectoriesList { get; private set; }
        public ObservableCollection<Movement> selectedTrajectoriesList { get; private set; }
        public ObservableCollection<Movement> selectableTrajectoriesList { get; private set; }

        private List<string> approaches;
        private List<string> exits;

        private string selectedApproach = "Any";
        private string selectedExit = "Any";

        private Bitmap backgroundImage;

        private YoloIntegerNameMapping _yoloNameMapping = new YoloIntegerNameMapping();

        //private List<int> _lineNumbers = new List<int>();

        public MainWindow()
        {
            InitializeComponent();
            trajectoriesList = new ObservableCollection<Movement>();
            selectedTrajectoriesList = new ObservableCollection<Movement>();
            selectableTrajectoriesList = new ObservableCollection<Movement>();
            prototypeTrajectoriesList = new ObservableCollection<Movement>();
            selectedPrototypeTrajectoriesList = new ObservableCollection<MatchWithExplanation>();
            trajectoryListView.ItemsSource = selectableTrajectoriesList;
            approaches = new List<string>();
            exits = new List<string>();
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                string[] droppedFilePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                foreach (var path in droppedFilePaths)
                {   
                    FileInfo fi = new FileInfo(path);
                    TrajectoryFilenameBox.Text = fi.Name;

                    ProcessFileType(path,fi);

                    if(Directory.Exists(path))
                    { 
                        var children = Directory.GetFiles(path);
                        foreach(var file in children)
                        {
                            ProcessFileType(file, new FileInfo(file));
                        }
                    }

                }
            }
        }

        private void ProcessFileType(string filepath, FileInfo fi)
        { 
            if (fi.Extension == ".json")
            {
                var lines = File.ReadAllLines(filepath);

                if (fi.Name.Contains("Movements"))
                {
                    Console.WriteLine("Processing " + filepath + " as movements.");

                    foreach (var line in lines)
                    {
                        var m = ParseAsMovement(line);
                        trajectoriesList.Add(m);
                        if (!approaches.Contains(m.Approach))
                        {
                            approaches.Add(m.Approach);
                        }

                        if (!exits.Contains(m.Exit))
                        {
                            exits.Add(m.Exit);
                        }
                    }
                }
                else if (fi.Name.Contains("Synthetic"))
                { 
                    Console.WriteLine("Processing " + filepath + " as synthetic movements.");
                    foreach (var line in lines)
                    {
                        var m = ParseAsMovement(line);
                        prototypeTrajectoriesList.Add(m);
                    }
                }

                PopulateApproachAndExitsBoxes();
            }
            else if (fi.Extension == ".png")
            {
                Console.WriteLine("Processing " + filepath + " as background image.");
                backgroundImage = (Bitmap) System.Drawing.Image.FromFile(filepath);
            }
        }

        private string ExtractTurnSubstring(string movementString)
        {
            var t = ParseAsMovement(movementString);
            return t.ToString();
        }

        private Movement ParseAsMovement(string movementJson)
        {
            return JsonConvert.DeserializeObject<Movement>(movementJson);
        }

        private void RenderBackground(DrawingContext drawingContext)
        {
            if (backgroundImage != null)
            {
                BitmapSource backgroundBmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    backgroundImage.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                drawingContext.DrawImage(backgroundBmpSource,
                    new Rect(0, 0, backgroundImage.Width, backgroundImage.Height));
            }
            else
            {
                drawingContext.DrawRectangle(Brushes.DarkGray, new System.Windows.Media.Pen(Brushes.DarkGray, 1), new Rect(new System.Windows.Size(640,480)) );
            }
        }

        private void RenderTrajectory(Movement m, int width, int height, double dpi, byte[] pixelData, DrawingContext drawingContext, System.Windows.Media.Brush lineColor, double lineThickness)
        {
            Regex rgx = new Regex(@"[^\d\.]");

            if(filterSyntheticCheckbox.IsChecked.Value)
            { 
                if(approachComboBox.SelectedItem != null)
                {
                    selectedApproach = approachComboBox.SelectedItem.ToString();
                    if(m.Approach != selectedApproach && selectedApproach != "Any")
                    {
                        return;
                    }        
                }

                if(exitComboxBox.SelectedItem != null)
                { 
                    selectedExit = exitComboxBox.SelectedItem.ToString();
                    if(m.Exit != selectedExit && selectedExit != "Any")
                    {
                        return;
                    }
                }
            }

            if(extrapolateCheckbox.IsChecked.Value)
            {
                var extrap = TrajectoryExtrapolator.ExtrapolatedTrajectory(m.StateEstimates, width, height);
                for (int i = 0; i < extrap.Count; i++)
                {
                    if (i + 1 < extrap.Count)
                    {
                        var thisPoint = extrap[i];
                        var nextPoint = extrap[i + 1];

                        if(thisPoint.X > width | thisPoint.X < 0)
                        { 
                            continue;
                        }

                        if(thisPoint.Y > height | thisPoint.Y < 0)
                        { 
                            continue;
                        }

                        if(nextPoint.X > width | nextPoint.X < 0)
                        { 
                            continue;
                        }

                        if(nextPoint.Y > height | nextPoint.Y < 0)
                        { 
                            continue;
                        }

                        drawingContext.DrawLine(new System.Windows.Media.Pen(Brushes.Yellow, lineThickness),
                            new Point(thisPoint.X, thisPoint.Y), new Point(nextPoint.X, nextPoint.Y));
                    }
                }           
            }

            for (int i = 0; i < m.StateEstimates.Count; i++)
            {
                if (i + 1 < m.StateEstimates.Count)
                {
                    var thisPoint = m.StateEstimates[i];
                    var nextPoint = m.StateEstimates[i + 1];

                    if(thisPoint.X > width | thisPoint.X < 0)
                    { 
                        continue;
                    }

                    if(thisPoint.Y > height | thisPoint.Y < 0)
                    { 
                        continue;
                    }

                    if(nextPoint.X > width | nextPoint.X < 0)
                    { 
                        continue;
                    }

                    if(nextPoint.Y > height | nextPoint.Y < 0)
                    { 
                        continue;
                    }

                    drawingContext.DrawLine(new System.Windows.Media.Pen(lineColor, lineThickness),
                        new Point(thisPoint.X, thisPoint.Y), new Point(nextPoint.X, nextPoint.Y));
                }
            }

            var firstMeasurement = m.StateEstimates.First();
            var lastMeasurement = m.StateEstimates.Last();

            if(showEndpointsCheckbox.IsChecked.Value)
            { 
                drawingContext.DrawEllipse(Brushes.Yellow, new  System.Windows.Media.Pen(Brushes.Yellow,1), new Point(firstMeasurement.X, firstMeasurement.Y), 3, 3);
                drawingContext.DrawEllipse(Brushes.Purple, new  System.Windows.Media.Pen(Brushes.Purple,1), new Point(lastMeasurement.X, lastMeasurement.Y), 3, 3);
            }
        }

        private void RenderJoins(List<StateEstimate> trajectory1, List<StateEstimate> trajectory2, DrawingContext drawingContext, System.Windows.Media.Brush lineColor, double lineThickness)
        {
            //For each point on t1, find the nearest corresponding point on t2 while ensuring we only move forwards on t2.
            var indexT2 = 0;
            for(int i=0;i<trajectory1.Count;i++)
            {
                //Draw a line connecting these points.
                var trajectory1StateEstimate = trajectory1[i];
                var trajectory2NearestStateEstimate = TrajectorySimilarity.NearestPointOnTrajectory(trajectory1StateEstimate, trajectory2.GetRange(indexT2,trajectory2.Count()-indexT2));
                indexT2 = trajectory2.IndexOf(trajectory2NearestStateEstimate);
                drawingContext.DrawLine(new System.Windows.Media.Pen(lineColor, lineThickness),
                        new Point(trajectory1StateEstimate.X, trajectory1StateEstimate.Y), new Point(trajectory2NearestStateEstimate.X, trajectory2NearestStateEstimate.Y));
            }
            
        }

        private List<Movement> SyntheticPrototypes()
        {
            return prototypeTrajectoriesList.ToList();
        }

        private void trajectoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double dpi = 96;
            int width = 640;
            int height = 480;
            byte[] pixelData = new byte[width * height];
            Regex rgx = new Regex(@"[^\d\.]");

            trajectoryMatchListView.Items.Clear();
            selectedTrajectoriesList.Clear();

            foreach (var item in e.AddedItems)
            {   
                var movement = (Movement) item;
                selectedTrajectoriesList.Add(movement);
            }

            DrawSelectedTrajectories();

            foreach (var item in e.AddedItems)
            {   
                var movement = (Movement) item;
                movementNameBox.Content = movement.TurnType.ToString();

                //Compare against all synthetic trajectories
                var synthetics = SyntheticPrototypes();
                
                var mostLikelyClassType =
                        YoloIntegerNameMapping.GetObjectNameFromClassInteger(movement.StateEstimates.Last().MostFrequentClassId(),
                            _yoloNameMapping.IntegerToObjectName);
                var uppercaseClassType = CommonFunctions.FirstCharToUpper(mostLikelyClassType);

                var tracked_object = new VTC.Common.TrackedObject();
                tracked_object.ObjectType = mostLikelyClassType;
                tracked_object.StateHistory = movement.StateEstimates;
                //Populate trajectory-comparison list box with comparison statistics for each synthetic trajectory 
                List<MatchWithExplanation> matchExplanations = new List<MatchWithExplanation>();
                foreach(var synth in synthetics)
                {
                    var matchCost = TrajectorySimilarity.PathIntegralCost(tracked_object.StateHistory, synth.StateEstimates);
                    var description = synth.ToString() + " match-cost: " + Math.Round(matchCost,1) + "," + TrajectorySimilarity.CostExplanation(tracked_object.StateHistory, synth.StateEstimates);
                    var matchExplanation = new MatchWithExplanation();
                    matchExplanation.matchCost = matchCost;
                    matchExplanation.explanation = description;
                    matchExplanation.movement = synth;
                    matchExplanations.Add(matchExplanation);
                }

                var sortedMatches = matchExplanations.OrderBy(me => me.matchCost);
                foreach(var me in sortedMatches)
                {
                    System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem();
                    lvi.Text = me.explanation;
                    lvi.Tag = me;
                    trajectoryMatchListView.Items.Add(lvi);
                }

                //var recalculateMatchCost = TrajectorySimilarity.PathIntegralCost(tracked_object.StateHistory, sortedMatches.First().movement.StateEstimates); 
                //var recalculatedExplanation = sortedMatches.First().movement + " match-cost: " + Math.Round(recalculateMatchCost, 1) + "," + TrajectorySimilarity.CostExplanation(tracked_object.StateHistory, sortedMatches.First().movement.StateEstimates);
                //System.Windows.Forms.MessageBox.Show(recalculatedExplanation);

                var netMovement = tracked_object.NetMovement();
                netMovementBox.Content = netMovement;

                var pathLength = tracked_object.PathLengthIntegral();
                pathLengthBox.Content = pathLength;

                numSamplesBox.Content = movement.StateEstimates.Count();
                missedDetectionsBox.Content = movement.StateEstimates.Last().TotalMissedDetections;
                var lastStateEstimate = movement.StateEstimates.Last();
                finalPositionCovarianceBox.Content = Math.Sqrt(Math.Pow(lastStateEstimate.CovX,2) + Math.Pow(lastStateEstimate.CovX,2));
                maximumPositionCovarianceBox.Content =
                    movement.StateEstimates.Select(se => Math.Sqrt(Math.Pow(se.CovX,2) + Math.Pow(se.CovY,2))).Max();
                averagePositionCovarianceBox.Content =
                    movement.StateEstimates.Select(se => Math.Sqrt(Math.Pow(se.CovX, 2) + Math.Pow(se.CovY, 2))).Average();

                var smoothness = movement.StateEstimates.Smoothness();
                smoothnessBox.Content = smoothness;

                var movementToLengthRatio = netMovement / pathLength;
                movementToLengthRatioBox.Content = movementToLengthRatio;

                var smoothMovement = smoothness * movementToLengthRatio;
                smoothMovementBox.Content = smoothMovement;
                
                missRatioBox.Content = Math.Round( movement.MissRatio(),2);
            }
        }

        private void exitComboxBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedExit = exitComboxBox.SelectedItem.ToString();
            UpdateSelectedTrajectories();
            DrawSelectedTrajectories();
        }

        private void approachComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedApproach = approachComboBox.SelectedItem.ToString();
            UpdateSelectedTrajectories();
            DrawSelectedTrajectories();
        }

        private void PopulateApproachAndExitsBoxes()
        {
            approachComboBox.Items.Clear();
            exitComboxBox.Items.Clear();

            approaches.Sort();
            exits.Sort();

            foreach(var a in approaches)
            {approachComboBox.Items.Add(a);}
            approachComboBox.Items.Add("Any");

            foreach(var e in exits)
            {exitComboxBox.Items.Add(e);}
            exitComboxBox.Items.Add("Any");
        }

        private void UpdateSelectedTrajectories()
        {
            selectedTrajectoriesList.Clear();
            selectableTrajectoriesList.Clear();
            foreach (var t in trajectoriesList)
            {
                if (t.Approach != selectedApproach && selectedApproach != "Any")
                {
                    continue;
                }
                
                if (t.Exit != selectedExit && selectedExit != "Any")
                {
                    continue;
                }

                selectedTrajectoriesList.Add(t);
                selectableTrajectoriesList.Add(t);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
           DrawSelectedTrajectories();
        }

        private void DrawSelectedTrajectories()
        {
            double dpi = 96;
            int width = 640;
            int height = 480;
            byte[] pixelData = new byte[width * height];
            var visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                RenderBackground(drawingContext);

                foreach (var prototypeMovement in prototypeTrajectoriesList)
                {
                    RenderTrajectory(prototypeMovement, width, height, dpi, pixelData, drawingContext, System.Windows.Media.Brushes.Blue, .5);
                }

                foreach (var mwe in selectedPrototypeTrajectoriesList)
                {
                    RenderTrajectory(mwe.movement, width, height, dpi, pixelData, drawingContext, System.Windows.Media.Brushes.Aquamarine, .8);
                }

                foreach (var t in selectedTrajectoriesList)
                {
                    if(selectedTrajectoriesList.Count == 1)
                    {
                        RenderTrajectory(t, width, height, dpi, pixelData, drawingContext, System.Windows.Media.Brushes.Red, 2.0); 

                        foreach(var mwe in selectedPrototypeTrajectoriesList)
                        {
                            RenderJoins(t.StateEstimates,mwe.movement.StateEstimates, drawingContext, Brushes.Yellow, 0.5);

                            //var recalculateMatchCost = TrajectorySimilarity.PathIntegralCost(t.StateEstimates, mwe.movement.StateEstimates);
                            //var recalculatedExplanation = mwe.movement + " match-cost: " + Math.Round(recalculateMatchCost, 1) + "," + TrajectorySimilarity.CostExplanation(t.StateEstimates, mwe.movement.StateEstimates);
                            //System.Windows.Forms.MessageBox.Show(recalculatedExplanation);
                        }
                    }
                    else
                    {
                        RenderTrajectory(t, width, height, dpi, pixelData, drawingContext, System.Windows.Media.Brushes.Red, 0.5);
                    }
                }


            }
            var image = new DrawingImage(visual.Drawing);
            trajectoryRenderingImage.Source = image;
        }

        private void trajectoryMatchListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPrototypeTrajectoriesList.Clear();
            foreach (var item in e.AddedItems)
            {
                var lvi = (System.Windows.Forms.ListViewItem) item;
                MatchWithExplanation mwe = (MatchWithExplanation) lvi.Tag;
                
                selectedPrototypeTrajectoriesList.Add(mwe);
            }
            DrawSelectedTrajectories();
        }
    }

    public class Measurement
    {
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public int R;
        public int G;
        public int B;
        public int Size;
        public string Type;
    }
}
