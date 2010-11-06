using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SampleEditor
{
    public partial class WaveDisplay : UserControl
    {
        bool drawing;
        bool hasSelection;
        bool playing;
        byte[] audioData;
        AudioFormat format;
        Point endPoint;
        Point startPoint;
        AudioDecoder decoder;

        public WaveDisplay()
        {
            InitializeComponent();          
        }      

        public double RelativeStartPosition
        {
            get
            {
                return this.startPoint.X / this.audioCanvas.Width;
            }
        }

        public double RelativeEndPosition
        {
            get
            {
                return this.endPoint.X / this.audioCanvas.Width;
            }
        }

        public void Load(byte[] audioData, AudioFormat format)
        {
            this.ClearGraph();
            this.format = format;
            this.audioData = audioData;
            var graph = new PolyLineSegment();
            int step = this.GetSampleStep();
            int index = 0;
            int count = 0;

            double yScale = (this.audioCanvas.Height / 2) / short.MaxValue;

            int leftSampleCount = audioData.Length / 4; //assume 2 channels
            int actualLeftSampleCount = leftSampleCount / (step / 4);

            while (index < audioData.Length)
            {
                short sample = BitConverter.ToInt16(new byte[2] { audioData[index], audioData[index + 1] }, 0);
                double yPoint = sample * yScale + (this.audioCanvas.Height / 2);
                double xPoint = count * this.audioCanvas.Width / actualLeftSampleCount;

                graph.Points.Add(new Point() { X = xPoint, Y = yPoint });
                index += step;
                count++;
            }
            this.figure.Segments.Add(graph);
        }

        public void ClearGraph()
        {
            this.figure.Segments.Clear();
        }

        public void ResetSelection()
        {
            this.hasSelection = false;
            this.rectangle.Visibility = Visibility.Collapsed;
        }

        public void Play()
        {
            if (!this.playing)
            {
                this.decoder = new AudioDecoder(this.audioData, this.format);
                this.SetMediaEndPoints();
                this.media.SetSource(this.decoder);
                this.media.Play();
                this.playing = true;
            }
        }

        void SetMediaEndPoints()
        {
            if (this.decoder != null && this.audioData != null)
            {
                this.decoder.StartPoint = this.GetActualAudioStartPoint();
                this.decoder.EndPoint = this.GetActualAudioEndPoint();
            }
        }

        public void Stop()
        {
            this.playing = false;
            this.media.Stop();
        }

        void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.hasSelection = false;
            this.rectangle.Visibility = Visibility.Visible;
            this.rectangle.Width = 1;
            this.drawing = true;
            this.startPoint = e.GetPosition(this.audioCanvas);
            this.rectangle.SetValue(Canvas.LeftProperty, (double)this.startPoint.X);
        }

        void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.EndSelection(e.GetPosition(this.audioCanvas));
        }

        void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                this.endPoint = e.GetPosition(this.audioCanvas);
                if (this.endPoint.X > this.startPoint.X)
                {
                    this.hasSelection = true;
                    this.rectangle.Width = this.endPoint.X - this.startPoint.X;
                    this.SetMediaEndPoints();
                }
            }
        }

        void CanvasMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.drawing)
            {
                this.EndSelection(e.GetPosition(this.audioCanvas));
            }
        }

        void EndSelection(Point point)
        {
            this.endPoint = point;
            this.drawing = false;
            this.hasSelection = true;
        }

        int GetActualAudioStartPoint()
        {
            if (!this.hasSelection)
                return 0;

            int actual = (int)((double)this.audioData.Length
                * this.RelativeStartPosition);

            int remainder = actual % 4;

            actual -= remainder;

            return actual;
        }

        int GetActualAudioEndPoint()
        {
            if (!this.hasSelection)
                return this.audioData.Length - 1;

            int actual = (int)((double)this.audioData.Length *
                this.RelativeEndPosition);

            int remainder = actual % 4;

            actual += remainder;
            return actual;
        }

        int GetSampleStep()
        {
            if (this.format.SamplesPerSecond >= 96000)
            {
                return 512;
            }

            if (this.format.SamplesPerSecond >= 32000)
            {
                return 256;
            }


            if (this.format.SamplesPerSecond >= 20000)
            {
                return 128;
            }

            return 64;
        }
    }


}
