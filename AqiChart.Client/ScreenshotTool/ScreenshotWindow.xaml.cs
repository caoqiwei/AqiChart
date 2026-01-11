using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using Path = System.Windows.Shapes.Path;

namespace AqiChart.Client.ScreenshotTool
{
    public partial class ScreenshotWindow : Window
    {
        public event EventHandler<BitmapSource> ScreenshotTaken;
        public event EventHandler ScreenshotCancelled;

        private Point startPoint;
        private Point endPoint;
        private Rect selectionRect;
        private bool isSelecting;
        private bool isEditing;
        private DrawingMode currentDrawingMode;
        private SolidColorBrush currentBrush;
        private double currentThickness;

        private Stack<UIElement> undoStack = new Stack<UIElement>();
        private Stack<UIElement> redoStack = new Stack<UIElement>();

        private UIElement currentDrawingElement;
        private Path currentBrushPath;
        private Point brushLastPoint;

        private enum DrawingMode
        {
            None,
            Rectangle,
            Ellipse,
            Line,
            Arrow,
            Brush,
            Text,
            Mosaic
        }

        public ScreenshotWindow()
        {
            InitializeComponent();

            // 初始化默认值
            currentBrush = new SolidColorBrush(Colors.Red);
            currentThickness = 3;
            currentDrawingMode = DrawingMode.Rectangle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取当前鼠标所在的屏幕
                var currentScreen = ScreenHelper.GetCurrentScreen();

                // 只覆盖当前屏幕
                this.Left = currentScreen.Left;
                this.Top = currentScreen.Top;
                this.Width = currentScreen.Width;
                this.Height = currentScreen.Height;

                OverlayRect.Width = currentScreen.Width;
                OverlayRect.Height = currentScreen.Height;

                // 设置初始模式
                EnterSelectionMode();

                // 播放截图音效
                PlayScreenshotSound();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化截图窗口失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void PlayScreenshotSound()
        {
            try
            {
                // 简单的系统提示音
                System.Media.SystemSounds.Beep.Play();
            }
            catch
            {
                // 忽略声音播放错误
            }
        }

        private void EnterSelectionMode()
        {
            isSelecting = true;
            isEditing = false;

            // 显示提示
            HintBorder.Visibility = Visibility.Visible;
            ToolbarBorder.Visibility = Visibility.Collapsed;
            PositionTip.Visibility = Visibility.Visible;

            // 重置选择
            ResetSelection();

            // 设置光标
            this.Cursor = Cursors.Cross;

            // 确保DrawingCanvas不拦截鼠标事件
            DrawingCanvas.IsHitTestVisible = false;

            // 清理绘图状态
            currentDrawingElement = null;
            currentBrushPath = null;

            // 清除绘图栈
            undoStack.Clear();
            redoStack.Clear();
        }

        private void EnterEditingMode()
        {
            isSelecting = false;
            isEditing = true;

            // 显示工具栏
            ToolbarBorder.Visibility = Visibility.Visible;
            HintBorder.Visibility = Visibility.Collapsed;
            PositionTip.Visibility = Visibility.Collapsed;
            SizeTip.Visibility = Visibility.Collapsed;

            // 设置光标
            UpdateCursorForDrawingMode();

            // 允许DrawingCanvas接收鼠标事件
            DrawingCanvas.IsHitTestVisible = true;

            // 初始化绘图状态
            currentDrawingElement = null;
            currentBrushPath = null;

            // 更新工具栏位置
            UpdateToolbarPosition();
        }

        #region 截图选择功能

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting && e.ChangedButton == MouseButton.Left)
            {
                Point mousePos = e.GetPosition(MainCanvas);
                startPoint = mousePos;
                endPoint = mousePos;

                UpdateSelectionRect();
                SizeTip.Visibility = Visibility.Collapsed;

                CaptureMouse();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(MainCanvas);

            // 更新坐标提示
            UpdatePositionTip(mousePos);

            if (isSelecting && IsMouseCaptured)
            {
                endPoint = mousePos;
                UpdateSelectionRect();
                UpdateSizeTip();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting && IsMouseCaptured)
            {
                // 确保最小选择区域
                if (selectionRect.Width < 10 || selectionRect.Height < 10)
                {
                    ResetSelection();
                }
                else
                {
                    // 显示尺寸提示
                    UpdateSizeTip();
                    SizeTip.Visibility = Visibility.Visible;
                }

                ReleaseMouseCapture();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting && selectionRect.Width >= 10 && selectionRect.Height >= 10)
            {
                TakeScreenshot();
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isEditing && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // 按住Ctrl键滚动滚轮调整画笔粗细
                double delta = e.Delta > 0 ? 1 : -1;
                double newThickness = currentThickness + delta;

                if (newThickness >= 1 && newThickness <= 20)
                {
                    currentThickness = newThickness;

                    // 更新UI显示
                    for (int i = 0; i < ThicknessComboBox.Items.Count; i++)
                    {
                        if (ThicknessComboBox.Items[i] is ComboBoxItem item &&
                            item.Content.ToString() == newThickness.ToString())
                        {
                            ThicknessComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                e.Handled = true;
            }
        }

        private void UpdateSelectionRect()
        {
            double x = Math.Min(startPoint.X, endPoint.X);
            double y = Math.Min(startPoint.Y, endPoint.Y);
            double width = Math.Abs(endPoint.X - startPoint.X);
            double height = Math.Abs(endPoint.Y - startPoint.Y);

            selectionRect = new Rect(x, y, width, height);

            // 更新UI元素
            SelectionRect.Width = width;
            SelectionRect.Height = height;
            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);

            BorderRect.Width = width;
            BorderRect.Height = height;
            Canvas.SetLeft(BorderRect, x);
            Canvas.SetTop(BorderRect, y);

            // 更新遮罩层
            UpdateOverlayMask();
        }

        private void UpdateOverlayMask()
        {
            RectangleGeometry overlayGeometry = new RectangleGeometry(
                new Rect(0, 0, OverlayRect.Width, OverlayRect.Height));

            RectangleGeometry selectionGeometry = new RectangleGeometry(selectionRect);

            CombinedGeometry combinedGeometry = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                overlayGeometry,
                selectionGeometry);

            OverlayRect.Clip = combinedGeometry;
        }

        private void UpdateSizeTip()
        {
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                SizeTipText.Text = $"{selectionRect.Width:F0} × {selectionRect.Height:F0}";

                double tipX = selectionRect.Right - SizeTip.ActualWidth;
                double tipY = selectionRect.Top - SizeTip.ActualHeight - 5;

                if (tipY < 0) tipY = selectionRect.Bottom + 5;

                Canvas.SetLeft(SizeTip, tipX);
                Canvas.SetTop(SizeTip, tipY);
            }
        }

        private void UpdatePositionTip(Point point)
        {
            if (isSelecting)
            {
                PositionTipText.Text = $"({point.X:F0}, {point.Y:F0})";

                double tipX = point.X + 10;
                double tipY = point.Y + 10;

                Canvas.SetLeft(PositionTip, tipX);
                Canvas.SetTop(PositionTip, tipY);
            }
        }

        private void ResetSelection()
        {
            selectionRect = Rect.Empty;
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
            BorderRect.Width = 0;
            BorderRect.Height = 0;
            SizeTip.Visibility = Visibility.Collapsed;
            OverlayRect.Clip = null;
        }

        private void TakeScreenshot()
        {
            try
            {
                // 截取选择区域
                var bitmapSource = ScreenHelper.CaptureRegion(selectionRect);

                if (bitmapSource != null)
                {
                    // 进入编辑模式
                    EnterEditingMode();

                    // 将截图作为DrawingCanvas的背景
                    DrawingCanvas.Background = new ImageBrush(bitmapSource);

                    // 设置DrawingCanvas的位置和大小
                    DrawingCanvas.Width = selectionRect.Width;
                    DrawingCanvas.Height = selectionRect.Height;
                    Canvas.SetLeft(DrawingCanvas, selectionRect.Left);
                    Canvas.SetTop(DrawingCanvas, selectionRect.Top);

                    // 清除之前的绘图
                    ClearDrawings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"截图失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CancelScreenshot();
            }
        }

        private void UpdateToolbarPosition()
        {
            double toolbarX = selectionRect.Left;
            double toolbarY = selectionRect.Top - ToolbarBorder.ActualHeight - 10;

            if (toolbarY < 10)
            {
                toolbarY = selectionRect.Bottom + 10;
            }

            ToolbarBorder.Margin = new Thickness(toolbarX, toolbarY, 0, 0);
        }

        #endregion

        #region 绘图功能

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isEditing) return;

            Point canvasPoint = e.GetPosition(DrawingCanvas);

            if (currentDrawingMode == DrawingMode.Text)
            {
                AddTextElement(canvasPoint);
            }
            else if (currentDrawingMode == DrawingMode.Mosaic)
            {
                AddMosaicElement(canvasPoint);
            }
            else
            {
                StartDrawing(canvasPoint);
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isEditing || (currentDrawingElement == null && currentBrushPath == null)) return;

            Point canvasPoint = e.GetPosition(DrawingCanvas);

            if (currentBrushPath != null)
            {
                UpdateDrawingBrush(canvasPoint);
            }
            else if (currentDrawingElement != null)
            {
                UpdateDrawing(canvasPoint);
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isEditing) return;

            FinishDrawing();
        }

        private void StartDrawing(Point start)
        {
            brushLastPoint = start;

            switch (currentDrawingMode)
            {
                case DrawingMode.Rectangle:
                    currentDrawingElement = CreateRectangle();
                    break;
                case DrawingMode.Ellipse:
                    currentDrawingElement = CreateEllipse();
                    break;
                case DrawingMode.Line:
                    currentDrawingElement = CreateLine(start);
                    break;
                case DrawingMode.Arrow:
                    currentDrawingElement = CreateArrow(start);
                    break;
                case DrawingMode.Brush:
                    currentBrushPath = CreateBrushPath(start);
                    break;
            }

            if (currentDrawingElement != null)
            {
                DrawingCanvas.Children.Add(currentDrawingElement);
                Canvas.SetLeft(currentDrawingElement, start.X);
                Canvas.SetTop(currentDrawingElement, start.Y);
            }
            else if (currentBrushPath != null)
            {
                DrawingCanvas.Children.Add(currentBrushPath);
            }
        }

        private void UpdateDrawing(Point current)
        {
            if (currentDrawingElement is Rectangle rect)
            {
                double x = Math.Min(brushLastPoint.X, current.X);
                double y = Math.Min(brushLastPoint.Y, current.Y);
                double width = Math.Abs(current.X - brushLastPoint.X);
                double height = Math.Abs(current.Y - brushLastPoint.Y);

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                rect.Width = width;
                rect.Height = height;
            }
            else if (currentDrawingElement is Ellipse ellipse)
            {
                double x = Math.Min(brushLastPoint.X, current.X);
                double y = Math.Min(brushLastPoint.Y, current.Y);
                double width = Math.Abs(current.X - brushLastPoint.X);
                double height = Math.Abs(current.Y - brushLastPoint.Y);

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                ellipse.Width = width;
                ellipse.Height = height;
            }
            else if (currentDrawingElement is Line line)
            {
                line.X2 = current.X - brushLastPoint.X;
                line.Y2 = current.Y - brushLastPoint.Y;
            }
            else if (currentDrawingElement is Path arrow)
            {
                UpdateArrow(arrow, current);
            }
        }

        private void UpdateDrawingBrush(Point current)
        {
            if (currentBrushPath != null && currentBrushPath.Data is PathGeometry geometry)
            {
                if (geometry.Figures.Count > 0)
                {
                    PathFigure figure = geometry.Figures[0];
                    figure.Segments.Add(new LineSegment(current, true));
                }
            }
        }

        private void FinishDrawing()
        {
            if (currentBrushPath != null)
            {
                undoStack.Push(currentBrushPath);
                redoStack.Clear();
                currentBrushPath = null;
            }
            else if (currentDrawingElement != null)
            {
                undoStack.Push(currentDrawingElement);
                redoStack.Clear();
                currentDrawingElement = null;
            }
        }

        private Rectangle CreateRectangle()
        {
            return new Rectangle
            {
                Stroke = currentBrush,
                StrokeThickness = currentThickness,
                Fill = Brushes.Transparent,
                Width = 0,
                Height = 0
            };
        }

        private Ellipse CreateEllipse()
        {
            return new Ellipse
            {
                Stroke = currentBrush,
                StrokeThickness = currentThickness,
                Fill = Brushes.Transparent,
                Width = 0,
                Height = 0
            };
        }

        private Line CreateLine(Point start)
        {
            return new Line
            {
                Stroke = currentBrush,
                StrokeThickness = currentThickness,
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 0,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
        }

        private Path CreateArrow(Point start)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(0, 0),
                IsClosed = false
            };

            figure.Segments.Add(new LineSegment(new Point(10, 0), true));
            figure.Segments.Add(new LineSegment(new Point(7, -3), true));
            figure.Segments.Add(new LineSegment(new Point(7, 3), true));
            figure.Segments.Add(new LineSegment(new Point(10, 0), true));

            geometry.Figures.Add(figure);

            return new Path
            {
                Stroke = currentBrush,
                StrokeThickness = currentThickness,
                Fill = currentBrush,
                Data = geometry,
                Width = 0,
                Height = 0
            };
        }

        private void UpdateArrow(Path arrow, Point end)
        {
            double dx = end.X - brushLastPoint.X;
            double dy = end.Y - brushLastPoint.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double angle = Math.Atan2(dy, dx) * 180 / Math.PI;

            arrow.Width = length;
            arrow.Height = 10;
            arrow.RenderTransform = new RotateTransform(angle);
        }

        private Path CreateBrushPath(Point start)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = start,
                IsClosed = false
            };
            geometry.Figures.Add(figure);

            return new Path
            {
                Stroke = currentBrush,
                StrokeThickness = currentThickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = geometry
            };
        }

        private void AddTextElement(Point position)
        {
            TextBox textBox = new TextBox
            {
                Width = 200,
                MinHeight = 30,
                Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                BorderBrush = currentBrush,
                BorderThickness = new Thickness(1),
                Foreground = currentBrush,
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalContentAlignment = VerticalAlignment.Top
            };

            Canvas.SetLeft(textBox, position.X);
            Canvas.SetTop(textBox, position.Y);

            DrawingCanvas.Children.Add(textBox);
            textBox.Focus();

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    DrawingCanvas.Children.Remove(textBox);
                }
                else
                {
                    // 将TextBox转换为TextBlock
                    TextBlock textBlock = new TextBlock
                    {
                        Text = textBox.Text,
                        Foreground = currentBrush,
                        FontSize = 14,
                        FontWeight = FontWeights.Normal,
                        Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                        TextWrapping = TextWrapping.Wrap
                    };

                    // 计算文本大小
                    textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                    textBlock.Arrange(new Rect(textBlock.DesiredSize));

                    Canvas.SetLeft(textBlock, Canvas.GetLeft(textBox));
                    Canvas.SetTop(textBlock, Canvas.GetTop(textBox));

                    DrawingCanvas.Children.Remove(textBox);
                    DrawingCanvas.Children.Add(textBlock);

                    undoStack.Push(textBlock);
                    redoStack.Clear();
                }
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                else if (e.Key == Key.Escape)
                {
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            };
        }

        private void AddMosaicElement(Point position)
        {
            Rectangle mosaic = new Rectangle
            {
                Width = 60,
                Height = 60,
                Fill = CreateMosaicBrush(),
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(mosaic, position.X - 30);
            Canvas.SetTop(mosaic, position.Y - 30);

            DrawingCanvas.Children.Add(mosaic);
            undoStack.Push(mosaic);
            redoStack.Clear();
        }

        private DrawingBrush CreateMosaicBrush()
        {
            DrawingBrush brush = new DrawingBrush();

            GeometryDrawing drawing = new GeometryDrawing
            {
                Brush = Brushes.Gray,
                Geometry = new GeometryGroup
                {
                    Children = new GeometryCollection
                    {
                        new RectangleGeometry(new Rect(0, 0, 4, 4)),
                        new RectangleGeometry(new Rect(8, 0, 4, 4)),
                        new RectangleGeometry(new Rect(0, 8, 4, 4)),
                        new RectangleGeometry(new Rect(8, 8, 4, 4))
                    }
                }
            };

            brush.Drawing = drawing;
            brush.Viewport = new Rect(0, 0, 12, 12);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.TileMode = TileMode.Tile;

            return brush;
        }

        private void ClearDrawings()
        {
            DrawingCanvas.Children.Clear();
            undoStack.Clear();
            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                UIElement element = undoStack.Pop();
                DrawingCanvas.Children.Remove(element);
                redoStack.Push(element);
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                UIElement element = redoStack.Pop();
                DrawingCanvas.Children.Add(element);
                undoStack.Push(element);
            }
        }

        private void UpdateCursorForDrawingMode()
        {
            switch (currentDrawingMode)
            {
                case DrawingMode.Rectangle:
                case DrawingMode.Ellipse:
                    this.Cursor = Cursors.Cross;
                    break;
                case DrawingMode.Line:
                case DrawingMode.Arrow:
                    this.Cursor = Cursors.Pen;
                    break;
                case DrawingMode.Brush:
                    this.Cursor = Cursors.Pen;
                    break;
                case DrawingMode.Text:
                    this.Cursor = Cursors.IBeam;
                    break;
                case DrawingMode.Mosaic:
                    this.Cursor = Cursors.Hand;
                    break;
                default:
                    this.Cursor = Cursors.Arrow;
                    break;
            }
        }

        #endregion

        #region 工具栏事件处理

        private void RectToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Rectangle);
            UpdateToolButtonStyles(DrawingMode.Rectangle);
        }

        private void EllipseToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Ellipse);
            UpdateToolButtonStyles(DrawingMode.Ellipse);
        }

        private void LineToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Line);
            UpdateToolButtonStyles(DrawingMode.Line);
        }

        private void ArrowToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Arrow);
            UpdateToolButtonStyles(DrawingMode.Arrow);
        }

        private void BrushToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Brush);
            UpdateToolButtonStyles(DrawingMode.Brush);
        }

        private void TextToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Text);
            UpdateToolButtonStyles(DrawingMode.Text);
        }

        private void MosaicToolButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(DrawingMode.Mosaic);
            UpdateToolButtonStyles(DrawingMode.Mosaic);
        }

        private void SetDrawingMode(DrawingMode mode)
        {
            currentDrawingMode = mode;
            UpdateCursorForDrawingMode();
        }

        private void UpdateToolButtonStyles(DrawingMode selectedMode)
        {
            // 重置所有按钮样式
            RectToolButton.Style = (Style)FindResource("ToolButtonStyle");
            EllipseToolButton.Style = (Style)FindResource("ToolButtonStyle");
            LineToolButton.Style = (Style)FindResource("ToolButtonStyle");
            ArrowToolButton.Style = (Style)FindResource("ToolButtonStyle");
            BrushToolButton.Style = (Style)FindResource("ToolButtonStyle");
            TextToolButton.Style = (Style)FindResource("ToolButtonStyle");
            MosaicToolButton.Style = (Style)FindResource("ToolButtonStyle");

            // 设置选中按钮样式
            switch (selectedMode)
            {
                case DrawingMode.Rectangle:
                    RectToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Ellipse:
                    EllipseToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Line:
                    LineToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Arrow:
                    ArrowToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Brush:
                    BrushToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Text:
                    TextToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
                case DrawingMode.Mosaic:
                    MosaicToolButton.Style = (Style)FindResource("SelectedToolButtonStyle");
                    break;
            }
        }

        private void RedColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.Red);
            UpdateColorButtonStyles(Colors.Red);
        }

        private void BlueColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.Blue);
            UpdateColorButtonStyles(Colors.Blue);
        }

        private void GreenColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.Green);
            UpdateColorButtonStyles(Colors.Green);
        }

        private void YellowColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.Yellow);
            UpdateColorButtonStyles(Colors.Yellow);
        }

        private void BlackColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.Black);
            UpdateColorButtonStyles(Colors.Black);
        }

        private void WhiteColorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentColor(Colors.White);
            UpdateColorButtonStyles(Colors.White);
        }

        private void SetCurrentColor(Color color)
        {
            currentBrush = new SolidColorBrush(color);
        }

        private void UpdateColorButtonStyles(Color selectedColor)
        {
            // 重置所有颜色按钮样式
            RedColorButton.Style = (Style)FindResource("ColorButtonStyle");
            BlueColorButton.Style = (Style)FindResource("ColorButtonStyle");
            GreenColorButton.Style = (Style)FindResource("ColorButtonStyle");
            YellowColorButton.Style = (Style)FindResource("ColorButtonStyle");
            BlackColorButton.Style = (Style)FindResource("ColorButtonStyle");
            WhiteColorButton.Style = (Style)FindResource("ColorButtonStyle");

            // 设置选中颜色按钮样式
            if (selectedColor == Colors.Red)
                RedColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
            else if (selectedColor == Colors.Blue)
                BlueColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
            else if (selectedColor == Colors.Green)
                GreenColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
            else if (selectedColor == Colors.Yellow)
                YellowColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
            else if (selectedColor == Colors.Black)
                BlackColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
            else if (selectedColor == Colors.White)
                WhiteColorButton.Style = (Style)FindResource("SelectedColorButtonStyle");
        }

        private void ThicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThicknessComboBox.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Content.ToString(), out double thickness))
            {
                currentThickness = thickness;
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearDrawings();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveScreenshot();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyScreenshot();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelScreenshot();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            FinishScreenshot();
        }

        #endregion

        #region 快捷键处理

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (isEditing)
                {
                    // 如果正在编辑，取消编辑回到选择模式
                    EnterSelectionMode();
                }
                else
                {
                    CancelScreenshot();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && isSelecting &&
                     selectionRect.Width >= 10 && selectionRect.Height >= 10)
            {
                TakeScreenshot();
                e.Handled = true;
            }
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control && isEditing)
            {
                CopyScreenshot();
                e.Handled = true;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && isEditing)
            {
                SaveScreenshot();
                e.Handled = true;
            }
            else if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control && isEditing)
            {
                Undo();
                e.Handled = true;
            }
            else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control && isEditing)
            {
                Redo();
                e.Handled = true;
            }
            else if (e.Key == Key.R && isEditing)
            {
                RectToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.E && isEditing)
            {
                EllipseToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.L && isEditing)
            {
                LineToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.A && isEditing && Keyboard.Modifiers == ModifierKeys.None)
            {
                ArrowToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.B && isEditing)
            {
                BrushToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.T && isEditing)
            {
                TextToolButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.M && isEditing)
            {
                MosaicToolButton_Click(null, null);
                e.Handled = true;
            }
        }

        #endregion

        #region 截图处理

        private BitmapSource CaptureFinalScreenshot()
        {
            try
            {
                // 创建一个RenderTargetBitmap来合并截图和绘图
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)selectionRect.Width,
                    (int)selectionRect.Height,
                    96, 96, PixelFormats.Pbgra32);

                // 创建一个DrawingVisual来绘制所有元素
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext context = drawingVisual.RenderOpen())
                {
                    // 绘制原始截图
                    if (DrawingCanvas.Background is ImageBrush imageBrush &&
                        imageBrush.ImageSource is BitmapSource source)
                    {
                        context.DrawImage(source, new Rect(0, 0, selectionRect.Width, selectionRect.Height));
                    }

                    // 绘制所有绘图元素
                    foreach (UIElement element in DrawingCanvas.Children)
                    {
                        VisualBrush brush = new VisualBrush(element);
                        Rect bounds = VisualTreeHelper.GetDescendantBounds(element);

                        double left = Canvas.GetLeft(element);
                        double top = Canvas.GetTop(element);

                        if (double.IsNaN(left)) left = 0;
                        if (double.IsNaN(top)) top = 0;

                        context.DrawRectangle(brush, null,
                            new Rect(left, top, bounds.Width, bounds.Height));
                    }
                }

                renderBitmap.Render(drawingVisual);
                return renderBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成截图失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void FinishScreenshot()
        {
            BitmapSource finalScreenshot = CaptureFinalScreenshot();
            if (finalScreenshot != null)
            {
                ScreenshotTaken?.Invoke(this, finalScreenshot);
                this.Close();
            }
        }

        private void SaveScreenshot()
        {
            BitmapSource finalScreenshot = CaptureFinalScreenshot();
            if (finalScreenshot == null) return;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG 图片|*.png|JPEG 图片|*.jpg|BMP 图片|*.bmp",
                DefaultExt = "png",
                FileName = $"截图_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();
                    BitmapEncoder encoder;

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(finalScreenshot));

                    using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    MessageBox.Show($"截图已保存到: {saveDialog.FileName}",
                        "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyScreenshot()
        {
            BitmapSource finalScreenshot = CaptureFinalScreenshot();
            if (finalScreenshot != null)
            {
                try
                {
                    Clipboard.SetImage(finalScreenshot);
                    MessageBox.Show("截图已复制到剪贴板", "成功",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"复制失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelScreenshot()
        {
            ScreenshotCancelled?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        #endregion
    }
}