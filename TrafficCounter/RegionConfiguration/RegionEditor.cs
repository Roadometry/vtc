using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Forms;
using VTC.Common.RegionConfig;
using NLog;

namespace VTC.RegionConfiguration
{
    using Kernel.Video;
    using ThumbnailImage = Image<Bgr, float>;

    public partial class RegionEditor : Form
    {
        private Polygon _editingPolygon;
        private ExamplePath _editingExamplePath;
        private readonly Dictionary<Button, Polygon> _polygonLookup = new Dictionary<Button, Polygon>();
        private readonly Dictionary<Button, ExamplePath> _pathLookup = new Dictionary<Button, ExamplePath>();

        private readonly PictureBox _preview = new PictureBox();

        private readonly IRegionConfigDataAccessLayer _regionConfigDal;

        private readonly Dictionary<string, ThumbnailImage> _thumbnails = new Dictionary<string, ThumbnailImage>();

        private BindingSource _regionConfigurationsBindingSource;

        private List<RegionConfig> _regionConfigs;

        private static readonly Logger Logger = LogManager.GetLogger("main.form");
   
        public RegionEditor(IEnumerable<ICaptureSource> captureSources, IRegionConfigDataAccessLayer regionConfigDal, RegionConfig currentlySelectedConfig = null)
        {
            InitializeComponent();

            _regionConfigDal = regionConfigDal;

            _regionConfigs = _regionConfigDal.LoadRegionConfigList();

            _regionConfigurationsBindingSource = new BindingSource { DataSource = _regionConfigs };
            PopulateConfigurationList();

            lbRegionConfigurations.DataSource = _regionConfigurationsBindingSource;
            lbRegionConfigurations.DisplayMember = "Title";
            lbRegionConfigurations.ValueMember = "";

            tbRegionConfigName.DataBindings.Add("Text", _regionConfigurationsBindingSource, "Title", true, DataSourceUpdateMode.OnPropertyChanged);

            foreach (var cs in captureSources)
            {
                if (cs == null)
                {
                    continue;
                }

                try
                {
                    var name = cs.Name;
                    int i = 1;
                    while (_thumbnails.Keys.Contains(name))
                    {
                        name = $"{cs.Name} ({i++})";
                    }
                    var gotFrame = false;
                    var attempts = 0;
                    while(!gotFrame && attempts < 100)
                    {
                        var frame = cs.QueryFrame();
                        if(frame != null)
                        {
                            var frameConverted = frame.Convert<Bgr, float>();
                            _thumbnails[name] = frameConverted;
                            gotFrame = true;
                            break;
                        }
                        attempts++;
                    }
                    if(gotFrame == false)
                    {
                        Logger.Log(LogLevel.Error, "Region Editor was unable to obtain a frame from " + name);
                    }
                }
                catch (Exception e) //Todo: Make this more specific
                {
                    Logger.Log(LogLevel.Error, e.Message);
                }
                
            }
            if (!_thumbnails.Any())
            {
                _thumbnails["No capture sources found!"] = new ThumbnailImage(640, 480, new Bgr(Color.White));
            }

            var thumbnailBindingSource = new BindingSource {DataSource = _thumbnails};
            cbCaptureSource.DataSource = thumbnailBindingSource;
            cbCaptureSource.DisplayMember = "Key";
            cbCaptureSource.ValueMember = "Value";

            _preview.Dock = DockStyle.Fill;
            
            panelImage.Controls.Add(_preview);

            try
            {
                if (currentlySelectedConfig != null)
                {
                    for (int i = 0; i < lbRegionConfigurations.Items.Count; i++)
                    {
                        if (lbRegionConfigurations.Items[i].Equals(currentlySelectedConfig))
                        {
                            lbRegionConfigurations.SelectedIndex = i;
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        public RegionEditor(Image<Bgr,byte> background, string name, IRegionConfigDataAccessLayer regionConfigDal, RegionConfig currentlySelectedConfig = null)
        {
            InitializeComponent();

            _regionConfigDal = regionConfigDal;

            var regionConfigs = _regionConfigDal.LoadRegionConfigList();

            _regionConfigurationsBindingSource = new BindingSource { DataSource = regionConfigs };
            PopulateConfigurationList();

            lbRegionConfigurations.DataSource = _regionConfigurationsBindingSource;
            lbRegionConfigurations.DisplayMember = "Title";
            lbRegionConfigurations.ValueMember = "";

            tbRegionConfigName.DataBindings.Add("Text", _regionConfigurationsBindingSource, "Title", true, DataSourceUpdateMode.OnPropertyChanged);

            _thumbnails[name] = background.Convert<Bgr,float>();   

            var thumbnailBindingSource = new BindingSource {DataSource = _thumbnails};
            cbCaptureSource.DataSource = thumbnailBindingSource;
            cbCaptureSource.DisplayMember = "Key";
            cbCaptureSource.ValueMember = "Value";

            _preview.Dock = DockStyle.Fill;
            
            panelImage.Controls.Add(_preview);

            if (currentlySelectedConfig != null)
            {
                for (int i = 0; i < lbRegionConfigurations.Items.Count; i++)
                {
                    if (lbRegionConfigurations.Items[i].Equals(currentlySelectedConfig))
                    {
                        lbRegionConfigurations.SelectedIndex = i;
                    }
                }
            }
        }

        public RegionConfig SelectedRegionConfig => lbRegionConfigurations.SelectedItem as RegionConfig;

        private ThumbnailImage _thumbnail;
        private ThumbnailImage Thumbnail
        {
            get
            {
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                UpdateImage();
            }
        }
        private ThumbnailImage _mask;
        private ThumbnailImage Mask
        {
            get
            {
                return _mask;
            }
            set
            {
                _mask = value;
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            if (null == Thumbnail)
            {
                _preview.Image = null;
                return;
            }

            _preview.Dock = DockStyle.None;
            _preview.Anchor = AnchorStyles.Left;
            _preview.Image = null == Mask ? Thumbnail.ToBitmap() : Thumbnail.Add(Mask).ToBitmap();
            _preview.Size = Thumbnail.ToBitmap().Size;
            panelImage.Size = _preview.Size;
        }

        private void InitializeToggleButtons(RegionConfig regionConfig)
        {
            if (null == regionConfig)
                return;

            tlpPolygonToggles.RowStyles.Clear();
            tlpPolygonToggles.RowStyles.Add(new RowStyle() { SizeType=SizeType.AutoSize });
            tlpPolygonToggles.Controls.Clear();
            tlpPolygonToggles.RowCount = 1;

            var roiButton = CreateEditRegionButton("ROI", regionConfig.RoiMask);
            AddEditAndDeleteRegionButtons(roiButton, null, regionConfig.RoiMask);

            foreach (var regionKvp in regionConfig.Regions)
            {
                var edit = CreateEditRegionButton(regionKvp.Key, regionKvp.Value);
                var delete = CreateDeleteRegionButton(edit, regionKvp.Value);
                AddEditAndDeleteRegionButtons(edit, delete, regionKvp.Value);
            }

            foreach (var path in regionConfig.ExamplePaths)
            {
                var edit = CreateEditExamplePathButton(path);
                var delete = CreateDeleteExamplePathButton(edit, path);
                AddEditAndDeleteExamplePathButtons(edit, delete, path);
            }
        }

        private Button CreateEditRegionButton(string text, Polygon polygon)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.MouseEnter += tb_MouseEnter;
            button.MouseLeave += tb_MouseLeave;
            button.Click += (sender, args) =>
            {
                SetEditingRegion(true, button, polygon);
            };

            return button;
        }

        private Button CreateEditExamplePathButton(ExamplePath path)
        {
            var button = new Button
            {
                Text = path.Description(),
                Dock = DockStyle.Fill,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.MouseEnter += path_MouseEnter;
            button.MouseLeave += path_MouseLeave;
            button.Click += (sender, args) =>
            {
                SetEditingExamplePath(true, button, path);
            };

            return button;
        }

        private Button CreateDeleteRegionButton(Button editButton, Polygon polygon)
        {
            var deleteButton = new Button
            {
                Font =
                    new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold,
                        GraphicsUnit.Point, 0),
                Size = new Size(22, 22),
                Text = "X",
                UseVisualStyleBackColor = false
            };


            deleteButton.Click += (sender, args) =>
            {
                if (DialogResult.Yes == MessageBox.Show("Remove region " + editButton.Text + "?", string.Empty, MessageBoxButtons.YesNo))
                {
                    foreach(var kvp in SelectedRegionConfig.Regions.Where(r => r.Value == polygon).ToList())
                    {
                        SelectedRegionConfig.Regions.Remove(kvp.Key);
                    }
                    
                    tlpPolygonToggles.Controls.Remove(deleteButton);
                    tlpPolygonToggles.Controls.Remove(editButton);
                    _polygonLookup.Remove(editButton);
                }
            };

            return deleteButton;
        }

        private Button CreateDeleteExamplePathButton(Button editButton, ExamplePath path)
        {
            var deleteButton = new Button
            {
                Font =
                    new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold,
                        GraphicsUnit.Point, 0),
                Size = new Size(22, 22),
                Text = "X",
                UseVisualStyleBackColor = false
            };

            deleteButton.Click += (sender, args) =>
            {
                if (DialogResult.Yes == MessageBox.Show("Remove path " + editButton.Text + "?", string.Empty, MessageBoxButtons.YesNo))
                {
                    foreach (var removeThisPath in SelectedRegionConfig.ExamplePaths.Where(r => r.Equals(path)).ToList())
                    {
                        SelectedRegionConfig.ExamplePaths.Remove(removeThisPath);
                    }

                    tlpPolygonToggles.Controls.Remove(deleteButton);
                    tlpPolygonToggles.Controls.Remove(editButton);
                    _pathLookup.Remove(editButton);
                }
            };

            return deleteButton;
        }

        private void AddEditAndDeleteRegionButtons(Button edit, Button delete, Polygon polygon)
        {
            if (null != polygon)
                _polygonLookup[edit] = polygon;

            tlpPolygonToggles.Controls.Remove(btnAddApproachExit);
            tlpPolygonToggles.Controls.Remove(btnAddExamplePath);

            if (null != edit)
                tlpPolygonToggles.Controls.Add(edit, 0, tlpPolygonToggles.RowCount - 1);
            if (null != delete) 
                tlpPolygonToggles.Controls.Add(delete, 1, tlpPolygonToggles.RowCount - 1);

            tlpPolygonToggles.RowCount++;
            tlpPolygonToggles.Controls.Add(btnAddApproachExit, 0, tlpPolygonToggles.RowCount - 1);

            tlpPolygonToggles.RowCount++;
            tlpPolygonToggles.Controls.Add(btnAddExamplePath, 0, tlpPolygonToggles.RowCount - 1);
        }

        private void AddEditAndDeleteExamplePathButtons(Button edit, Button delete, ExamplePath path)
        {
            if (null != path)
                _pathLookup[edit] = path;

            tlpPolygonToggles.Controls.Remove(btnAddApproachExit);
            tlpPolygonToggles.Controls.Remove(btnAddExamplePath);

            if (null != edit)
                tlpPolygonToggles.Controls.Add(edit, 0, tlpPolygonToggles.RowCount - 1);
            if (null != delete)
                tlpPolygonToggles.Controls.Add(delete, 1, tlpPolygonToggles.RowCount - 1);

            tlpPolygonToggles.RowCount++;
            tlpPolygonToggles.Controls.Add(btnAddApproachExit, 0, tlpPolygonToggles.RowCount - 1);

            tlpPolygonToggles.RowCount++;
            tlpPolygonToggles.Controls.Add(btnAddExamplePath, 0, tlpPolygonToggles.RowCount - 1);
        }

        void tb_MouseLeave(object sender, EventArgs e)
        {
            Mask = null;
        }

        void path_MouseLeave(object sender, EventArgs e)
        {
            Mask = null;
        }

        void tb_MouseEnter(object sender, EventArgs e)
        {
            var rb = sender as Button;
            if (rb == null) return;
            var polygon = _polygonLookup[rb];
            var mask = polygon.GetMask(Thumbnail.Width, Thumbnail.Height, new Bgr(Color.Blue));
            Mask = mask;
        }

        void path_MouseEnter(object sender, EventArgs e)
        {
            var rb = sender as Button;
            if (rb == null) return;
            var path = _pathLookup[rb];
            var mask = path.GetMask(Thumbnail.Width, Thumbnail.Height, new Bgr(Color.Blue));
            Mask = mask;
        }

        private void SetEditingRegion(bool editing, Button activeButton, Polygon polygon)
        {
            // Disable all buttons while editing
            tlpRegionConfigSelector.Enabled = !editing;
            btnOK.Enabled = !editing;
            btnCancel.Enabled = !editing;
            foreach (var control in tlpRegionConfigEditor.Controls)
            {
                if (control == panelImage || !(control is Control))
                    continue;

                ((Control)control).Enabled = !editing;
            }

            if (editing)
            {
                _editingPolygon = polygon;

                if (null != activeButton)
                {
                    var control = new PolygonBuilderControl(Thumbnail, _polygonLookup[activeButton])
                    {
                        Dock = DockStyle.Fill
                    };
                    control.OnDoneClicked += (sender, args) =>
                    {
                        _editingPolygon.Clear();
                        foreach (var coord in control.Coordinates)
                        {
                            _editingPolygon.Add(coord);
                        }
                        polygon.UpdateCentroid();
                        SetEditingRegion(false, null, null);
                    };
                    control.OnCancelClicked += (sender, args) =>
                    {
                        SetEditingRegion(false, null, null);
                    };
                    panelImage.Controls.Clear();
                    panelImage.Controls.Add(control);
                }
            }
            else
            {
                // Restore the preview image
                panelImage.Controls.Clear();
                panelImage.Controls.Add(_preview);
            }
        }

        private void SetEditingExamplePath(bool editing, Button activeButton, ExamplePath path)
        {
            // Disable all buttons while editing
            tlpRegionConfigSelector.Enabled = !editing;
            btnOK.Enabled = !editing;
            btnCancel.Enabled = !editing;
            foreach (var control in tlpRegionConfigEditor.Controls)
            {
                if (control == panelImage || !(control is Control))
                    continue;

                ((Control)control).Enabled = !editing;
            }

            if (editing)
            {
                _editingExamplePath = path;

                if (null != activeButton)
                {
                    var control = new ExamplePathBuilderControl(Thumbnail, _pathLookup[activeButton])
                    {
                        Dock = DockStyle.Fill
                    };
                    control.OnDoneClicked += (sender, args) =>
                    {
                        _editingExamplePath.Points.Clear();
                        foreach (var coord in control.Coordinates.Points)
                        {
                            _editingExamplePath.Points.Add(coord);
                        }
                        
                        SetEditingExamplePath(false, null, null);
                    };
                    control.OnCancelClicked += (sender, args) =>
                    {
                        SetEditingExamplePath(false, null, null);
                    };
                    panelImage.Controls.Clear();
                    panelImage.Controls.Add(control);
                }
            }
            else
            {
                // Restore the preview image
                panelImage.Controls.Clear();
                panelImage.Controls.Add(_preview);
            }
        }

        private void btnAddApproachExit_Click(object sender, EventArgs e)
        {
            var selectedRegionConfig = lbRegionConfigurations.SelectedItem as RegionConfig;
            if (null == selectedRegionConfig)
                return;

            var input = new InputPrompt("Region Name", "Enter Region Name:");
            if (DialogResult.OK != input.ShowDialog())
                return;

            var polygon = new Polygon();
            var edit = CreateEditRegionButton(input.InputString, polygon);
            var delete = CreateDeleteRegionButton(edit, polygon);
            AddEditAndDeleteRegionButtons(edit, delete, polygon);

            selectedRegionConfig.Regions.Add(input.InputString, polygon);
        }

        private object _previousSelectedValue;
        private void lbRegionConfigurations_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_previousSelectedValue == lbRegionConfigurations.SelectedValue)
                return;

            if (null == lbRegionConfigurations.SelectedValue)
                return;

            _previousSelectedValue = lbRegionConfigurations.SelectedValue;

            var selectedRegionConfig = lbRegionConfigurations.SelectedItem as RegionConfig;
            if (null == selectedRegionConfig)
                return;

            InitializeToggleButtons(selectedRegionConfig);
        }

        private void PopulateConfigurationList()
        {
            int currentHeight = 20;
            int x_offset = 20;
            panel1.Controls.Clear();

            ToolTip tp = new ToolTip();
            tp.AutoPopDelay = 10000;
            tp.InitialDelay = 500;
            tp.ReshowDelay = 0;
            tp.ShowAlways = true;

            foreach (var p in typeof(RegionConfig).GetProperties())
            {
                if (p.PropertyType.Name == "Boolean")
                {
                    var newCheckbox = new CheckBox();
                    newCheckbox.SetBounds(x_offset, currentHeight, 250, 20);
                    newCheckbox.Text = p.Name;
                    currentHeight += newCheckbox.Height + 5;
                    newCheckbox.Name = p.Name + "checkBox";
                    var description = p.GetCustomAttribute<DescriptionAttribute>().Description;
                    if(description != null)
                    {
                        tp.SetToolTip(newCheckbox,description); 
                    }
                    panel1.Controls.Add(newCheckbox);
                    newCheckbox.DataBindings.Add("Checked", _regionConfigurationsBindingSource, p.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                }
                else if (p.PropertyType.Name == "String" || p.PropertyType.Name == "Int32" || p.PropertyType.Name == "Double")
                {
                    var newControl = new TextBox();
                    newControl.SetBounds(x_offset, currentHeight, 200, 20);
                    var newLabel = new Label();
                    newLabel.SetBounds(x_offset + newControl.Width + 5, currentHeight, 250, 20);
                    newLabel.Text = p.Name;
                    currentHeight += newControl.Height + 5;
                    newControl.Name = p.Name + "textBox";
                    var description = p.GetCustomAttribute<DescriptionAttribute>().Description;
                    if(description != null)
                    {
                        tp.SetToolTip(newControl,description); 
                        tp.SetToolTip(newLabel,description); 
                    }
                    panel1.Controls.Add(newControl);
                    panel1.Controls.Add(newLabel);
                    newControl.DataBindings.Add("Text", _regionConfigurationsBindingSource, p.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var input = new InputPrompt("Region Configuration Name", "Enter Region Configuration Name:");
            if (DialogResult.OK != input.ShowDialog())
                return;

            var newConfig = new RegionConfig()
            {
                Title = input.InputString
            };

            lbRegionConfigurations.DataSource = null;

            _regionConfigs.Add(newConfig);
            _regionConfigurationsBindingSource = new BindingSource { DataSource = _regionConfigs };

            lbRegionConfigurations.DataSource = _regionConfigurationsBindingSource;
            lbRegionConfigurations.DisplayMember = "Title";
            lbRegionConfigurations.ValueMember = "";

            lbRegionConfigurations.SelectedItem = newConfig;

            PopulateConfigurationList();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var item = lbRegionConfigurations.SelectedItem as RegionConfig;
            if (null == item)
                return;

            var result = MessageBox.Show("Are you sure you want to delete " + item.Title, "Confirm Delete", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _regionConfigurationsBindingSource.Remove(item);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Discard changes?", "Confirm", MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
                return;

            DialogResult = DialogResult.Cancel;

            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            var regionConfigs = _regionConfigurationsBindingSource.DataSource as List<RegionConfig>;
            if (null != regionConfigs)
            {
                foreach (var regionConfig in regionConfigs)
                {
                    foreach (var example in regionConfig.ExamplePaths)
                    {
                        Console.WriteLine("Name: " + example.Description());
                    }
                }

                _regionConfigDal.SaveRegionConfigList(regionConfigs);
            }

            Close();
        }

        private void cbCaptureSource_SelectedValueChanged(object sender, EventArgs e)
        {
            Thumbnail = cbCaptureSource.SelectedValue as Image<Bgr, float>;
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            //Select file
            if (importRegionConfigFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Deserialize RegionConfig
                System.IO.StreamReader sr = new System.IO.StreamReader(importRegionConfigFileDialog.FileName);
                System.Xml.XmlReader xr = new System.Xml.XmlTextReader(sr);
                
                DataContractSerializer s = new DataContractSerializer(typeof(RegionConfig));
                var importedRegionConfig = (RegionConfig) s.ReadObject(xr);
                xr.Close();
                sr.Close();

                //Add to list of RegionConfigs
                var regionConfigs = _regionConfigDal.LoadRegionConfigList();
                regionConfigs.Add(importedRegionConfig);
                _regionConfigurationsBindingSource = new BindingSource { DataSource = regionConfigs };

                lbRegionConfigurations.DataSource = _regionConfigurationsBindingSource;
                lbRegionConfigurations.DisplayMember = "Title";
                lbRegionConfigurations.ValueMember = "";
                tbRegionConfigName.DataBindings.Clear();
                tbRegionConfigName.DataBindings.Add("Text", _regionConfigurationsBindingSource, "Title", true, DataSourceUpdateMode.OnPropertyChanged);

                lbRegionConfigurations.SelectedIndex = lbRegionConfigurations.Items.Count - 1;
                lbRegionConfigurations_SelectedValueChanged(null, null);

                PopulateConfigurationList();

                btnOK_Click(null, null);
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            //Choose file location
            if (exportRegionConfigFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Serialize RegionConfig
                System.IO.StreamWriter sr = new System.IO.StreamWriter(exportRegionConfigFileDialog.FileName);
                System.Xml.XmlWriter xr = new System.Xml.XmlTextWriter(sr);
                DataContractSerializer s = new DataContractSerializer(typeof(RegionConfig));
                s.WriteObject(xr, SelectedRegionConfig);
                xr.Close();
                sr.Close();
            }
        }

        private void btnAddExamplePath_Click(object sender, EventArgs e)
        {
            var selectedRegionConfig = lbRegionConfigurations.SelectedItem as RegionConfig;
            if (null == selectedRegionConfig)
                return;

            var input = new ExamplePathCreatePrompt();
            if (DialogResult.OK != input.ShowDialog())
                return;

            var examplePath = new ExamplePath();
            examplePath.Approach = input.Approach;
            examplePath.Exit = input.Exit;
            examplePath.Ignored = input.Ignored;
            examplePath.PedestrianOnly = input.PedestrianOnly;
            examplePath.TurnType = input.SelectedTurn;

            var edit = CreateEditExamplePathButton(examplePath);
            var delete = CreateDeleteExamplePathButton(edit, examplePath);
            AddEditAndDeleteExamplePathButtons(edit, delete, examplePath);

            selectedRegionConfig.ExamplePaths.Add(examplePath);
        }
    }
}
