using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Security.Cryptography;
using System.Windows.Media.Media3D;
using System.Windows.Data;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;
using System.Runtime.CompilerServices;

namespace WpfApp1
{
    public class FilterExecutor
    {
        public static void Execute(Queue<CommandBase> commands, Mat src, Action<Mat> act)
        {
            if(commands.Count == 0)
            {
                act(src);
                return;
            }

            var cmd = commands.Dequeue();
            using (var dst = new Mat())
            {
                cmd.Execute(src, dst);

                Execute(commands, dst, act);
            }
        }
    }

    public abstract class CommandBase : BindableBase
    {
        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(ref _enabled, value); }
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public CommandBase(string id, string name, bool enabled = true)
        {
            Id = id;
            Name = name;
            Enabled = enabled;
        }

        public override string ToString()
        {
            return Name;
        }

        public abstract CommandBase DeepCopy();

        public abstract void Execute(Mat src, Mat dst);
    }

    public class GaussianBlurCommand : CommandBase
    {
        public class GaussianBlurArgs
        {
            public OpenCvSharp.Size Size { get; private set; }

            public GaussianBlurArgs(GaussianBlurCommand cmd)
            {
                Size = new OpenCvSharp.Size(cmd.Size, cmd.Size);
            }
        }

        private int _size = 15;
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public GaussianBlurCommand() : base("GaussianBlur", "ガウシアンぼかし") { }

        public GaussianBlurCommand(GaussianBlurCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _size = a.Size;
        }

        public override CommandBase DeepCopy()
        {
            return new GaussianBlurCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new GaussianBlurArgs(this);
            Cv2.GaussianBlur(src, dst, args.Size, 0);
        }
    }


    public class MedianBlurCommand : CommandBase
    {
        public class MedianBlurArgs
        {
            public int Size { get; private set; }
            public MedianBlurArgs(int size)
            {
                Size = size;
            }

            public MedianBlurArgs(MedianBlurCommand cmd)
            {
                Size = cmd.Size;
            }
        }

        private int _size = 15;
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public MedianBlurCommand() : base("MedianBlur", "メディアンぼかし") { }

        public MedianBlurCommand(MedianBlurCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _size = a.Size;
        }

        public override CommandBase DeepCopy()
        {
            return new MedianBlurCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new MedianBlurArgs(this);
            Cv2.MedianBlur(src, dst, args.Size);
        }
    }

    public class BilateralFilterCommand : CommandBase
    {
        public class BilateralFilterArgs
        {
            public int Size { get; private set; }
            public int SigmaColor { get; private set; }
            public int SigmaSpace { get; private set; }

            public BilateralFilterArgs(BilateralFilterCommand cmd)
            {
                Size = cmd.Size;
                SigmaColor = cmd.SigmaColor;
                SigmaSpace = cmd.SigmaSpace;
            }
        }

        private int _size = 15;
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        private int _sigmaColor = 75;
        public int SigmaColor
        {
            get { return _sigmaColor; }
            set { SetProperty(ref _sigmaColor, value); }
        }

        private int _sigmaSpace = 75;
        public int SigmaSpace
        {
            get { return _sigmaSpace; }
            set { SetProperty(ref _sigmaSpace, value); }
        }

        public BilateralFilterCommand() : base("BilateralFilter", "バイラテラルフィルタ") { }

        public BilateralFilterCommand(BilateralFilterCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _size = a.Size;
            _sigmaColor = a.SigmaColor;
            _sigmaSpace = a.SigmaSpace;
        }

        public override CommandBase DeepCopy()
        {
            return new BilateralFilterCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new BilateralFilterArgs(this);
            Cv2.BilateralFilter(src, dst, args.Size, args.SigmaColor, args.SigmaColor);
        }
    }

    public class ThresholdCommand : CommandBase
    {
        public class ThresholdArgs
        {
            public int Threshold { get; private set; }

            public ThresholdArgs(ThresholdCommand cmd)
            {
                Threshold = cmd.Threshold;
            }
        }

        private int _threshold = 100;
        public int Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }

        public ThresholdCommand() : base("Threshold", "二値化") { }

        public ThresholdCommand(ThresholdCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _threshold = a.Threshold;
        }

        public override CommandBase DeepCopy()
        {
            return new ThresholdCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new ThresholdArgs(this);
            Cv2.Threshold(src, dst, args.Threshold, 255, ThresholdTypes.Binary);
        }
    }

    public class MorphologyExCommand : CommandBase
    {
        public class MorphologyExArgs
        {
            public MorphTypes Type { get; private set; }

            public OpenCvSharp.Size Size { get; private set; }

            public MorphologyExArgs(MorphologyExCommand cmd)
            {
                switch (cmd.Types[cmd.SelectedIndex])
                {
                    case "open":
                        Type = MorphTypes.Open; break;
                    case "close":
                        Type = MorphTypes.Close; break;
                    case "erode":
                        Type = MorphTypes.Erode; break;
                    case "dilate":
                        Type = MorphTypes.Dilate; break;
                    case "tophat":
                        Type = MorphTypes.TopHat; break;
                    case "blackhat":
                        Type = MorphTypes.BlackHat; break;
                    default:
                        Type = MorphTypes.Open; break;
                }

                Size = new OpenCvSharp.Size(cmd.Size, cmd.Size);
            }
        }

        private int _size = 5;
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        private List<string> types = new List<string>()
        {
            "open", "close", "erode", "dilate", "tophat", "blackhat"
        };
        public List<string> Types
        {
            get { return types; }
        }

        public MorphologyExCommand() : base("MorphologyEx", "モーフォロジー変換") { }

        public MorphologyExCommand(MorphologyExCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _size = a.Size;
            selectedIndex = a.SelectedIndex;
        }

        public override CommandBase DeepCopy()
        {
            return new MorphologyExCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new MorphologyExArgs(this);
            Cv2.MorphologyEx(src, dst, args.Type, Cv2.GetStructuringElement(MorphShapes.Ellipse, args.Size));
        }
    }

    public class ScaleAbsCommand : CommandBase
    {
        public class ScaleAbsArgs
        {
            public double Alpha { get; private set; }

            public double Beta { get; private set; }

            public ScaleAbsArgs(ScaleAbsCommand cmd)
            {
                Alpha = cmd.Alpha;
                Beta = cmd.Beta;
            }
        }

        private double _alpha = 1.0;
        public double Alpha
        {
            get { return _alpha; }
            set { SetProperty(ref _alpha, value); }
        }

        private double _beta = 0.0;
        public double Beta
        {
            get { return _beta; }
            set { SetProperty(ref _beta, value); }
        }

        public ScaleAbsCommand() : base("ConvertScaleAbs", "コントラスト・明度") { }

        public ScaleAbsCommand(ScaleAbsCommand a) : base(a.Id, a.Name, a.Enabled)
        {
            _alpha = a.Alpha;
            _beta = a.Beta;
        }

        public override CommandBase DeepCopy()
        {
            return new ScaleAbsCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new ScaleAbsArgs(this);
            Cv2.ConvertScaleAbs(src, dst, args.Alpha, args.Beta);
        }
    }

    public class GrayscaleCommand : CommandBase
    {
        public class GrayscaleArgs
        {
            public ColorConversionCodes Code
            {
                get { return ColorConversionCodes.RGB2GRAY; }
            }

            public GrayscaleArgs(GrayscaleCommand cmd)
            {
            }
        }

        public GrayscaleCommand() : base("CvtColor", "グレイスケール") { }

        public GrayscaleCommand(GrayscaleCommand a) : base(a.Id, a.Name, a.Enabled) { }

        public override CommandBase DeepCopy()
        {
            return new GrayscaleCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new GrayscaleArgs(this);
            Cv2.CvtColor(src, dst, args.Code);
        }
    }

    public class BitwiseNotCommand : CommandBase
    {
        public BitwiseNotCommand() : base("BitwiseNot", "白黒反転") { }

        public BitwiseNotCommand(BitwiseNotCommand a) : base(a.Id, a.Name, a.Enabled) { }

        public override CommandBase DeepCopy()
        {
            return new BitwiseNotCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            Cv2.BitwiseNot(src, dst);
        }
    }

    public class RotateCommand : CommandBase
    {
        public class RotateArgs
        {
            public int Angle { get; private set; }

            public int Scale { get; private set; }

            public RotateArgs(RotateCommand cmd)
            {
                Angle = cmd.Angle;
                Scale = 1;
            }
        }

        private int _angle = 0;
        public int Angle
        {
            get { return _angle; }
            set { SetProperty(ref _angle, value); }
        }

        public RotateCommand() : base("Rotate", "回転") { }

        public RotateCommand(RotateCommand a) : base(a.Id, a.Name, a.Enabled) { }

        public override CommandBase DeepCopy()
        {
            return new RotateCommand(this);
        }

        public override void Execute(Mat src, Mat dst)
        {
            var args = new RotateArgs(this);
            var height = src.Height;
            var width = src.Width;
            var center = new Point2f((float)(width / 2.0), (float)(height / 2.0));
            var trans = Cv2.GetRotationMatrix2D(center, args.Angle, args.Scale);

            double absCos = Math.Abs(trans.At<double>(0, 0));
            double absSin = Math.Abs(trans.At<double>(0, 1));

            int boundW = (int)(src.Rows * absSin + src.Cols * absCos);
            int boundH = (int)(src.Rows * absCos + src.Cols * absSin);

            trans.At<double>(0, 2) += boundW / 2 - center.X;
            trans.At<double>(1, 2) += boundH / 2 - center.Y;

            Cv2.WarpAffine(src, dst, trans, new OpenCvSharp.Size(boundW, boundH));
        }
    }

    public class Rectangle : BindableBase
    {
        private double left;
        public double Left
        {
            get { return left; }
            set { SetProperty(ref left, value); }
        }

        private double top;
        public double Top
        {
            get { return top; }
            set { SetProperty(ref top, value);}
        }

        private int width;
        public int Width
        {
            get { return width; }
            set {
                SetProperty(ref width, value);
            }
        }

        private int height;
        public int Height
        {
            get { return height; }
            set {
                SetProperty(ref height, value);
            }
        }
    }

    class MainWindowVM : BindableBase
    {
        private Bitmap _bitmap;
        public ImageSource Source
        {
            get { return _bitmap.ToImageSource(); }
        }

        private Bitmap _trimedBitmap;
        public ImageSource TrimedSource
        {
            get { return _trimedBitmap.ToImageSource(); }
        }

        private int _commandTypeSelectedIndex = 0;
        public int CommandTypeSelectedIndex
        {
            get { return _commandTypeSelectedIndex; }
            set { SetProperty(ref _commandTypeSelectedIndex, value); }
        }

        private List<CommandBase> _commandTypes = new List<CommandBase>()
        {
            new GrayscaleCommand(),
            new GaussianBlurCommand(),
            new MedianBlurCommand(),
            new BilateralFilterCommand(),
            new ThresholdCommand(),
            new BitwiseNotCommand(),
            new MorphologyExCommand(),
            new ScaleAbsCommand(),
            new RotateCommand(),
        };
        public List<CommandBase> CommandTypes
        {
            get { return _commandTypes; }
        }

        private ObservableCollection<CommandBase> _filters = new ObservableCollection<CommandBase>();
        public ObservableCollection<CommandBase> Filters
        {
            get { return _filters; }
            private set { 
                SetProperty(ref _filters, value);
            }
        }

        private int _filterSelectedIndex = -1;
        public int FilterSelectedIndex
        {
            get { return _filterSelectedIndex; }
            set { 
                SetProperty(ref _filterSelectedIndex, value);
                UpFilterCommand.RaiseCanExecuteChanged();
                DownFilterCommand.RaiseCanExecuteChanged();
                DeleteFilterCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _recognizedText = "";
        public string RecognizedText
        {
            get { return _recognizedText; }
            set { SetProperty(ref _recognizedText, value); }
        }

        private Rectangle _region = new Rectangle();
        public Rectangle Region
        {
            get { return _region; }
        }

        private Rectangle _scaledRegion = new Rectangle();

        public DelegateCommand AddFilterCommand { get; private set; }

        public DelegateCommand DeleteFilterCommand { get; private set; }

        public DelegateCommand UpFilterCommand { get; private set; }

        public DelegateCommand DownFilterCommand { get; private set; }

        public DelegateCommand VideoFileOpenCommand { get; private set; }

        private string _videoFilename = "";
        public string VideoFilename
        {
            get { return _videoFilename; }
            set { SetProperty(ref _videoFilename, value); }
        }

        private int _posMsec = 0;
        public int PosMsec
        {
            get { return _posMsec; }
            private set { SetProperty(ref _posMsec, value); }
        }

        private CancellationTokenSource tokenSource;

        private Task filterTask;

        private object lockObj = new object();

        private double _imgActualWidth;
        private double _imgActualHeight;

        public MainWindowVM()
        {
            AddFilterCommand = new DelegateCommand(() =>
            {
                var command = _commandTypes[_commandTypeSelectedIndex];

                if(command is GrayscaleCommand)
                {
                    foreach (var filter in _filters)
                    {
                        if (filter is GrayscaleCommand)
                        {
                            MessageBox.Show("グレイスケールフィルタは複数追加することはできません。");
                            return;
                        }
                    }
                }

                _filters.Add(command.DeepCopy());

                DeleteFilterCommand?.RaiseCanExecuteChanged();
            },
            () => true);

            DeleteFilterCommand = new DelegateCommand(() =>
            {
                var newFilters = new ObservableCollection<CommandBase>();
                for(int i = 0; i < _filters.Count; i++)
                {
                    if(i != _filterSelectedIndex)
                    {
                        newFilters.Add(_filters[i].DeepCopy());
                    }
                }

                var newSelectedIndex = _filterSelectedIndex;
                if(newFilters.Count <= 0)
                {
                    newSelectedIndex = -1;
                }
                if(newSelectedIndex >= newFilters.Count)
                {
                    newSelectedIndex = newFilters.Count - 1;
                }

                Filters = newFilters;

                FilterSelectedIndex = newSelectedIndex;

                DeleteFilterCommand?.RaiseCanExecuteChanged();
            },
            () =>
            {
                return _filters.Count > 0 && _filterSelectedIndex >= 0;
            });

            UpFilterCommand = new DelegateCommand(() =>
            {
                var newFilters = new ObservableCollection<CommandBase>();
                foreach (var filter in _filters) {
                    newFilters.Add(filter.DeepCopy());
                }

                var temp = newFilters[_filterSelectedIndex - 1];
                newFilters[_filterSelectedIndex - 1] = newFilters[_filterSelectedIndex];
                newFilters[_filterSelectedIndex] = temp;

                var newSelectedIndex = _filterSelectedIndex - 1;

                Filters = newFilters;

                FilterSelectedIndex = newSelectedIndex;
            }, () => {
                return _filterSelectedIndex > 0;
            });

            DownFilterCommand = new DelegateCommand(() =>
            {
                var newFilters = new ObservableCollection<CommandBase>();
                foreach (var filter in _filters)
                {
                    newFilters.Add(filter.DeepCopy());
                }

                var temp = newFilters[_filterSelectedIndex + 1];
                newFilters[_filterSelectedIndex + 1] = newFilters[_filterSelectedIndex];
                newFilters[_filterSelectedIndex] = temp;

                var newSelectedIndex = _filterSelectedIndex + 1;

                Filters = newFilters;

                FilterSelectedIndex = newSelectedIndex;
            }, () => {
                return !(_filterSelectedIndex >= (_filters.Count() - 1) || _filterSelectedIndex < 0);
            });

            VideoFileOpenCommand = new DelegateCommand(() =>
            {
                // ダイアログのインスタンスを生成
                var dialog = new OpenFileDialog();

                // ファイルの種類を設定
                dialog.Filter = "すべてのファイル (*.*)|*.*";

                // ダイアログを表示する
                if (dialog.ShowDialog() == true)
                {
                    VideoFilename = dialog.FileName;

                    if(filterTask != null)
                    {
                        tokenSource.Cancel();

                        while (!(filterTask.IsCanceled || filterTask.IsCompleted))
                        {
                            Task.Delay(100).Wait();
                        }

                        tokenSource.Dispose();
                    }

                    tokenSource = new CancellationTokenSource();

                    CancellationToken ct = tokenSource.Token;

                    filterTask = Task.Run(() =>
                    {
                        StartScanning(ct);
                    }, tokenSource.Token);
                }
            }, () => true);
        }

        private void StartScanning(CancellationToken ct)
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                var outputdir = string.Format("images/{0}", Path.GetFileNameWithoutExtension(_videoFilename));
                Directory.CreateDirectory(outputdir);
                Directory.CreateDirectory(Path.Combine(outputdir, "img"));

                // var capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                var capture = new VideoCapture(_videoFilename);
                if (!capture.IsOpened())
                {
                    throw new Exception("capture initialization failed");
                }

                using (var frame = new Mat(capture.FrameHeight, capture.FrameWidth, MatType.CV_8UC3))
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }

                        if (!capture.Read(frame))
                        {
                            break;
                        }

                        PosMsec = (int)capture.Get(VideoCaptureProperties.PosMsec);

                        Action<Mat> updateSource = (dst) =>
                        {
                            _bitmap = BitmapConverter.ToBitmap(dst);
                            RaisePropertyChanged("Source");

                            if (_imgActualWidth <= 0 || _imgActualHeight <= 0
                             || _region.Width <=0 || _region.Height <= 0)
                            {
                                _scaledRegion.Left = 0;
                                _scaledRegion.Top = 0;
                                _scaledRegion.Width = dst.Width;
                                _scaledRegion.Height = dst.Height;
                            }
                            else
                            {
                                var scale = dst.Width / _imgActualWidth;

                                if ((_region.Left + _region.Width) * scale > dst.Width)
                                {
                                    _region.Left = (dst.Width - _region.Width * scale) / scale;
                                }

                                if ((_region.Top + _region.Height) * scale > dst.Height)
                                {
                                    _region.Top = (dst.Height - _region.Height * scale) / scale;
                                }

                                _scaledRegion.Left = _region.Left * scale;
                                _scaledRegion.Top = _region.Top * scale;
                                _scaledRegion.Width = (int)((double)_region.Width * scale);
                                _scaledRegion.Height = (int)((double)_region.Height * scale);
                            }

                            using (var dst2 = dst.Clone(new OpenCvSharp.Rect((int)_scaledRegion.Left, (int)_scaledRegion.Top, _scaledRegion.Width, _scaledRegion.Height)))
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    var bmp4recog = BitmapConverter.ToBitmap(dst2);

                                    bmp4recog.Save(Path.Combine(outputdir, "img", string.Format("{0}.png", PosMsec.ToString("D8"))));

                                    bmp4recog.Save(ms, ImageFormat.Png);

                                    var ocr = new WindowsOcr();

                                    RecognizedText = ocr.Recognize(ms);

                                    File.AppendAllText(Path.Combine(outputdir, string.Format("recognized.txt")), string.Format("{0}\t{1}\n", PosMsec.ToString("D8"), RecognizedText));
                                }

                                _trimedBitmap = BitmapConverter.ToBitmap(dst2);

                                RaisePropertyChanged("TrimedSource");
                            }
                        };

                        var commands = new Queue<CommandBase>();

                        foreach (var filter in _filters)
                        {
                            if ((filter.Enabled))
                            {
                                commands.Enqueue(filter);
                            }
                        }

                        FilterExecutor.Execute(commands, frame, updateSource);
                    }
                }
            }
        }

        public void SendActualWidthAndActualHeight(double width, double height)
        {
            lock(lockObj)
            {
                _imgActualWidth = width;
                _imgActualHeight = height;
            }
        }
    }


    public static class BitmapExtention
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            if (bmp == null)
                return null;

            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
