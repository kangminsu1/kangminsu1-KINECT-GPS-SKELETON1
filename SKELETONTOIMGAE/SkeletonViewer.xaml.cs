using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace SKELETONTOIMGAE
{
    
    public partial class SkeletonViewer : UserControl
    {
        // 스켈레톤 변수 선언
        private Skeleton[] _FrameSkeletons;

        // 키넥트 스켈레톤 브러쉬 생성
        private readonly Brush[] _SkeletonBrushes;

        public SkeletonViewer()
        {
            InitializeComponent();

            // 사용할 브러쉬 컬러 설정
            this._SkeletonBrushes = new[] {Brushes.Red,Brushes.Crimson,Brushes.Indigo,
                Brushes.DodgerBlue,Brushes.DodgerBlue,Brushes.Purple,Brushes.Pink};

        }

        // **************************************************
        // 유저 컨트롤을 키넥트 센서 객체에 연결시키는 매서드
        // **************************************************
        protected const string KinectDevicePropertyName = "KinectDevice";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewer),
            new PropertyMetadata(null, KinectDeviceChanged));

        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewer viewer = (SkeletonViewer)owner;

            if (e.OldValue != null)
            {
                ((KinectSensor)e.OldValue).SkeletonFrameReady -= viewer.KinectDevice_SkeletonFrameReady;
                viewer._FrameSkeletons = null;
            }

            if (e.NewValue != null)
            {
                viewer.KinectDevice = (KinectSensor)e.NewValue;
                viewer.KinectDevice.SkeletonFrameReady += viewer.KinectDevice_SkeletonFrameReady;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
            }
        }

        public KinectSensor KinectDevice
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }

        // **************************************************
        // **************************************************



        // 스켈레톤 이벤트 핸들러
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            // 스켈레톤 시작전 값 초기화
            SkeletonPanel.Children.Clear();
            JointInfoPanel.Children.Clear();

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    if (this.IsEnabled)
                    {

                        frame.CopySkeletonDataTo(this._FrameSkeletons);


                        for (int i = 0; i < this._FrameSkeletons.Length; i++)
                        {

                            DrawSkeleton(this._FrameSkeletons[i], this._SkeletonBrushes[i]);


                            // you can track all the joints if you want
                            // 손의 위치값 트레킹 매서드
                            //      TrackJoint(skeleton.Joints[JointType.HandLeft], brush);
                            //      TrackJoint(skeleton.Joints[JointType.HandRight], brush);

                        }

                    }
                }
            }
        }
        // 스켈레톤 그리기

        private void DrawSkeleton(Skeleton skeleton, Brush brush)
        {
            if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                //Draw head and torso
                Polyline figure = CreateFigure(skeleton, brush, new[] {  JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                             JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter});
                

                SkeletonPanel.Children.Add(figure);

                figure = CreateFigure(skeleton, brush, new[] { JointType.HipLeft, JointType.HipRight });
                SkeletonPanel.Children.Add(figure);

                //Draw left leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonPanel.Children.Add(figure);

                //Draw right leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                SkeletonPanel.Children.Add(figure);

                //Draw left arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonPanel.Children.Add(figure);

                //Draw right arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonPanel.Children.Add(figure);
            }
        }



        
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 2;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position, this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        // ********************************************************
    }

}
