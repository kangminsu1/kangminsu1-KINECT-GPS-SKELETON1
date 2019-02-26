using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Windows.Threading;
using System.IO;
using System.IO.Ports;
using SKELETONTOIMGAE.ServiceReference2;
using System.Windows.Media.Animation;


// 키넥트 네임스페이스
using Microsoft.Kinect;


namespace SKELETONTOIMGAE
{

    public partial class MainWindow : Window
    {
        
        private WebBrowser webBrowser1 = new WebBrowser();
        // 키넥트 객체 생성
        private KinectSensor _KinectDevice;
        // 키넥트 스켈레톤 배열 생성
        private Skeleton[] _FrameSkeletons;
        // 키넥트 타이머 생성
        private Timer timer = null;
        private Timer timer2 = null;
       
        // 키넥트 이미지 생성
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        // 키넥트 로그인 생성
        LogIn login = new LogIn();

        // 카메라 앵글
        private int CameraAngle = 0;
        private int MaxAnlge = 0;

        // 트레킹 캡쳐
        private bool TrackingStatued = false;
        private bool CaptureStep = false;

        // 사운드
        
        private WMPLib.WindowsMediaPlayer AlamSound = null;
        private WMPLib.WindowsMediaPlayer AccessSound = null;
        private bool emergency = false;
        private bool emergencyswitch = false;

        // GPS 정보
        // 위도
        public string Latitude;
        // 경도
        public string Longitude;
        // 시간
        public string hour;
        public string min;
        public string sec;
        public int times;
        // GPS 상태
        public int gpsstep;
        private bool scanstep = true;

        // 시리얼 포트
        public SerialPort serialPort1 = new SerialPort();
        public Timer gpstimer = new Timer();
        private bool mapstep = false;






        public MainWindow()
        {
            // GPS 수신기 세팅
            serialPort1.PortName = "COM5";
            serialPort1.BaudRate = 9600;
            serialPort1.Parity = Parity.None;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.ReadBufferSize = 600;
            
            InitializeComponent();
            login.Show();
            TimeInterval();
            GPStimeinterval();

            // GPS 포트 시작
            try
            {
                serialPort1.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                gpstimer.Enabled = false;
                
                return;
            }
            


        }

        // 기본 UI 코딩*********************************************************************************************************************



        // ***********************************************************************
        // 상황 발생 메서드  *****************************************************
        // ***********************************************************************
        private void AlamMaker()
        {
            AlamSound = new WMPLib.WindowsMediaPlayer();
            AlamSound.URL = @"C:\Users\KIM\Desktop\Sound\Alarm_Clock.mp3";
        }
        private void AccessSoundMaker()
        {
            AccessSound = new WMPLib.WindowsMediaPlayer();
            AccessSound.URL = @"C:\Users\KIM\Desktop\Sound\Puppy_Love_Sting.mp3";
        }
        private void EmergencyCheck(object sender, ElapsedEventArgs e)
        {
            emergency = false;
        }



        // ***********************************************************************
        // 실시간 시간 업데이트// ************************************************
        // ***********************************************************************
        private void TimeInterval()
        {
            timer = new Timer();
            timer.Interval = 500;

            timer2 = new Timer();
            timer2.Interval = 5000;


            timer.Start();
            timer2.Start();
     //       timer.Elapsed += RealTime;
            timer.Elapsed += opnekinect;
            timer.Elapsed += TrackingCapture;
            timer2.Elapsed += EmergencyCheck;
        }

        private void GPStimeinterval()
        {
            gpstimer = new Timer();
            gpstimer.Interval = 1000;
            gpstimer.Start();
            gpstimer.Elapsed += Gpstimer_Elapsed;
            gpstimer.Elapsed += Scan_effect;

            
        }

       



        //  GPS 파싱 데이터 분리

        private void Gpstimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (serialPort1.IsOpen)
            {
                    WARNING_TEXT.Visibility = Visibility.Collapsed;
                    string data = serialPort1.ReadExisting();
                string[] strArr = data.Split('$');
                for (int i = 0; i < strArr.Length; i++)
                {
                    string strTemp = strArr[i];
                    string[] lineArr = strTemp.Split(',');
                    if (lineArr[0] == "GPGGA")
                    {
                        try
                            {
                                
                                // 위도값
                                Double dLat = Convert.ToDouble(lineArr[2]);
                                dLat = dLat / 100;
                                string[] lat = dLat.ToString().Split('.');
                                Latitude = //lineArr[3].ToString() +
                                lat[0].ToString() + "." +
                                ((Convert.ToDouble(lat[1]) /
                                60.00)*10000).ToString("#########");

                                // 경도값
                                Double dLon = Convert.ToDouble(lineArr[4]);
                                dLon = dLon / 100;
                                string[] lon = dLon.ToString().Split('.');
                                Longitude = //lineArr[5].ToString() +                          
                                lon[0].ToString() + "." +                            
                                ((Convert.ToDouble(lon[1]) /
                                60)*10000).ToString("#########");

                            // 시간값

                            Double dTime = Convert.ToDouble(lineArr[1]);
                            dTime = dTime + 90000;

                                if (dTime >= 240000)
                                {
                                   
                                    dTime = dTime - 240000;
                                    hour = dTime.ToString().Substring(0, 2) + " 시";
                                }
                                else
                                {
                                    
                                    hour = dTime.ToString().Substring(0, 2) + " 시";
                                }
                           
                            min = dTime.ToString().Substring(2, 2)+" 분";
                            sec = dTime.ToString().Substring(4)+" 초";

                                // GPS 상태값
                                Int16 dGPS_STEP = Convert.ToInt16(lineArr[6]);
                                if (dGPS_STEP == 0)
                                {
                                    GPS_상태.Text = "Unavailable";
                                    GPS_상태.Foreground = Brushes.Red;
                                }
                                else if (dGPS_STEP == 1)
                                {
                                    GPS_상태.Text = "Good";
                                    GPS_상태.Foreground = Brushes.Yellow;
                                }
                                else
                                {
                                    GPS_상태.Text = "Best";
                                    GPS_상태.Foreground = Brushes.Green;
                                }
                                // DOP 상태값
                                Double dGPS = Convert.ToDouble(lineArr[7]);
                                GPS_위성_상태.Text = dGPS.ToString();

                                // DOP 상태값
                                Double dDop = Convert.ToDouble(lineArr[8]);
                                GPS_DOP_상태.Text = dDop.ToString();







                                //Display
                                위도.Text = "N "+ Latitude;
                            경도.Text = "E "+Longitude;
                            TodayTime.Text = hour;
                            TodayTime_m.Text = min;
                            TodayTime_s.Text = sec;
                            mapamp(Latitude, Longitude);
                                GPS_DOP_상태.Foreground = Brushes.GreenYellow;
                                GPS_위성_상태.Foreground = Brushes.GreenYellow;
                               

                            }
                        catch
                        {
                                //Cannot Read GPS values
                              
                                위도.Text = "Serching";
                                경도.Text = "Serching";
                                GPS_상태.Text = "Serching";
                                GPS_상태.Foreground = Brushes.OrangeRed;
                                GPS_DOP_상태.Text = "Serching";
                                GPS_DOP_상태.Foreground = Brushes.OrangeRed;
                                GPS_위성_상태.Text = "Serching";
                                GPS_위성_상태.Foreground = Brushes.OrangeRed;
                                TextAni(scanstep, WARNING_TEXT);
                                WARNING_TEXT.Visibility = Visibility.Visible;


                            }
                    }
                }
            }
            else
            {
                    
                    위도.Text = "GPS OFF";
                    경도.Text = "GPS OFF";

            }
            }));
        }

        

















    //    private void RealTime(object sender, ElapsedEventArgs e)
    //    {
    //        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
    //        {
    //            DateTime now = DateTime.Now;
    //            TodayTime.Text = string.Format("{0:yyyy년 MM월 dd일 HH시 mm분 ss초}", now);

    //        }));
    //    }

        // ***********************************************************************
        // 로그인 상태 메서드 ****************************************************
        // ***********************************************************************

        private void opnekinect(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (login.OPENKINECT == true)
                {
                    AccessSoundMaker();
                    KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                    this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                    SupervisorName.Text = login.SupervisorName;
                    login.OPENKINECT = false;
                    ElemantLayoutRoot.Background.Opacity = 0;
                    _KinectDevice.ElevationAngle = CameraAngle;
                    EYE_POD_WINDOW.Topmost = true;
                    EYE_POD_WINDOW.IsEnabled = true;
                    MAP2_Copy.Visibility = Visibility.Collapsed;
                    
                    //          GPStimeinterval();




                }
            }));

        }

        // ********************************************************************************************************
        // 화면 캡쳐 메서드****************************************************************************************
        // ********************************************************************************************************

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBox.Show(DateTime.Now.ToString("   yyyy년MM월dd일 HH시mm분ss초 화면 캡쳐"),"EYE POD");

            
            AutoCapture();

        }



        // ********************************************************************************************************
        // 키넥트 앵글 컨트롤 메서드 ******************************************************************************
        // ********************************************************************************************************

        private void AngleUpDown(bool OR)
        {
            if (OR == true)
            {
                CameraAngle += 10;
                _KinectDevice.ElevationAngle = CameraAngle;
            }
            else
            {
                CameraAngle -= 10;
                _KinectDevice.ElevationAngle = CameraAngle;
            }
        }
        private void AngleUp_Click(object sender, RoutedEventArgs e)
        {
            if (MaxAnlge < 2)
            {
                MaxAnlge++;
                AngleUpDown(true);
            }
            else
            {
                MessageBox.Show(" Max Angle 입니다.", "경고");
            }
        }
        private void AngleDown_Click_1(object sender, RoutedEventArgs e)
        {
            if (MaxAnlge > -2)
            {
                MaxAnlge--;
                AngleUpDown(false);
            }
            else
            {
                MessageBox.Show(" Min Angle 입니다.", "경고");
            }
        }


        // ***************************************************************************************************************************************



        // ********************************************************************************************************
        // 키넥트 이벤트 핸들러 생성*******************************************************************************
        // ********************************************************************************************************

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:

                    this.KinectDevice = null;
                    break;
                default:
                    break;
            }
        }

        private void DangerImage(int DangerStep )
        {
            if (DangerStep ==0)
            {
                DangerImageStep0.Visibility = Visibility.Visible;
                DangerImageStep1.Visibility = Visibility.Collapsed;
                DangerImageStep2.Visibility = Visibility.Collapsed;
                DangerImageStep3.Visibility = Visibility.Collapsed;
            }
            if (DangerStep == 1)
            {
                DangerImageStep0.Visibility = Visibility.Collapsed;
                DangerImageStep1.Visibility = Visibility.Visible;
                DangerImageStep2.Visibility = Visibility.Collapsed;
                DangerImageStep3.Visibility = Visibility.Collapsed;
            }
            if (DangerStep == 2)
            {
                DangerImageStep0.Visibility = Visibility.Collapsed;
                DangerImageStep1.Visibility = Visibility.Collapsed;
                DangerImageStep2.Visibility = Visibility.Visible;
                DangerImageStep3.Visibility = Visibility.Collapsed;
            }
            if (DangerStep == 3)
            {
                DangerImageStep0.Visibility = Visibility.Collapsed;
                DangerImageStep1.Visibility = Visibility.Collapsed;
                DangerImageStep2.Visibility = Visibility.Collapsed;
                DangerImageStep3.Visibility = Visibility.Visible;
            }
            {

            }
        }

        // ********************************************************************************************************
        // 키넥트 스트림 생성**************************************************************************************
        // ********************************************************************************************************

        public KinectSensor KinectDevice
        {
            get { return this._KinectDevice; }
            set
            {
                if (this._KinectDevice != value)
                {
                    // 센서 종료

                    if (this._KinectDevice != null)
                    {
                        this._KinectDevice.Stop();
                        this._KinectDevice.ColorStream.Disable();
                        this._KinectDevice.SkeletonStream.Disable();
                        this._KinectDevice.AllFramesReady -= KinectDevice_AllFramesReady;
                        SkeletonViewerElement.KinectDevice = null;
                        this._FrameSkeletons = null;
                        CameraSwitchOFF.Opacity = 1;
                        CameraSwitchON.Opacity = 0;
                  
                    }

                    this._KinectDevice = value;

                    // 센서 시작

                    if (this._KinectDevice != null)
                    {


                        if (this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            CameraSwitchON.Opacity = 1;
                            CameraSwitchOFF.Opacity = 0;
                         
                            this.KinectDevice.AllFramesReady += KinectDevice_AllFramesReady;


                            this._KinectDevice.SkeletonStream.Enable();
                            this._FrameSkeletons = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            SkeletonViewerElement.KinectDevice = this.KinectDevice;


                            ColorImageStream colorStream = _KinectDevice.ColorStream;
                            this._KinectDevice.ColorStream.Enable();
                            // 컬러 화면 생성
                            this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                            this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;

                            ColorImageElement.Source = this._ColorImageBitmap;


                            this._KinectDevice.Start();

                        }
                    }
                }
            }
        }

        private void KinectDevice_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {


                    byte[] pixelData = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixelData);
                    this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData, this._ColorImageStride, 0);


                    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                    {
                        if (skeletonFrame != null)
                        {
                            skeletonFrame.CopySkeletonDataTo(this._FrameSkeletons);
                            // 가장 가까운 사용자 추적
                            Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);


                            if (skeleton == null)
                            {
                                WarningElement.Visibility = Visibility.Collapsed;
                                DangerImage(0);
                                DepthData.Text = " X ";
                                SkeletonTracking.Text = ("감시중");
                                DangerLavel.Text = ("양호");
                                DangerLavel.Foreground = Brushes.Green;
                                SkeletonTracking.Foreground = Brushes.White;


                                TrackingStatued = false;
                                
                            }
                            else
                            {
                                Joint getspine = GetSpine(skeleton);
                                TrackHuman(skeleton.Joints[JointType.HipCenter], WarningElement_Copy, LayoutRoot);
                               
                            }
                        }
                    }
                }
            }
        }

        // 우선순위 사용자 인덱스 설정 매서드
        private Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;
            if (skeletons != null)
            {
                //가까운 스켈레톤 데이터를 찾는다
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }
            return skeleton;
        }

        // 추적 위치 우선순위 설정
        private static Joint GetSpine(Skeleton skeleton)
        {
            Joint getspine = new Joint();

            if (skeleton != null)
            {
                getspine = skeleton.Joints[JointType.Spine];
                Joint hipcenter = skeleton.Joints[JointType.HipCenter];

                if (hipcenter.TrackingState != JointTrackingState.NotTracked)
                {
                    if (getspine.TrackingState == JointTrackingState.NotTracked)
                    {
                        getspine = hipcenter;
                    }
                    else
                    {
                        if (getspine.Position.Z > hipcenter.Position.Z)
                        {
                            getspine = hipcenter;
                        }
                    }
                }
            }
            return getspine;
        }

        // ********************************************************************************************************
        // 표적 트레킹 메서드 *************************************************************************************
        // ********************************************************************************************************

        private void TrackHuman(Joint human, FrameworkElement cursorElement, FrameworkElement container)
        {
            if (human.TrackingState == JointTrackingState.NotTracked)
            {
               
                cursorElement.Visibility = Visibility.Collapsed;
                SkeletonTracking.Text = ("감시중");
                SkeletonTracking.Foreground = Brushes.White;
                DepthData.Text = (" X ");
                DangerLavel.Text = ("양호");
                DangerLavel.Foreground = Brushes.Green;
               
            }

            else
            {


                double z = human.Position.Z;
                cursorElement.Visibility = Visibility.Visible;

                DepthImagePoint point = this._KinectDevice.MapSkeletonPointToDepth(human.Position, this.KinectDevice.DepthStream.Format);

                point.Y = (int)((point.Y * LayoutRoot.ActualHeight / this.KinectDevice.DepthStream.FrameHeight) - (cursorElement.ActualHeight / 2.0));

                Canvas.SetLeft(cursorElement, point.X-48);
                Canvas.SetTop(cursorElement, point.Y-15);
                Canvas.SetZIndex(cursorElement, (int)(4 - (z * 100)));

                DepthData.Text = string.Format("{0:0.000}M", z);
                SkeletonTracking.Text = ("감지");
                SkeletonTracking.Foreground = Brushes.Red;

              

                DangerLavelTextChange(z);
            }

            // ********************************************************************************************************
            // 위험 단계 변화 메서드 **********************************************************************************
            // ********************************************************************************************************

        }
        private void DangerLavelTextChange(double z)
        {

            if (z >= 3)
            {
                TrackingStatued = false;
                DangerLavel.Foreground = Brushes.Yellow;
                DangerImage(0);
            }
            if (z < 3 && z > 2.2)
            {
                TrackingStatued = false;
                DangerLavel.Text = ("80%");
                DangerLavel.Foreground = Brushes.Orange;
                DangerImage(1);


            }
            if (z <= 2.2 && z > 1.5)
            {
                TrackingStatued = false;
                DangerLavel.Text = ("90%");
                DangerLavel.Foreground = Brushes.Red;
                DangerImage(2);
            }

            if (z <= 1.5)
            {
                TrackingStatued = true;
                DangerLavel.Text = ("100%");
                DangerImage(3);
                emergencyswitch = true;
                if (emergencyswitch == true && emergency == false)
                {
                    emergency = true;
                    AlamMaker();
                    
                }
                
            }
        }

        private void TrackingCapture(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (TrackingStatued == true)
            {
                AutoCapture();
                AutoCaptureStatus.Text = "캡쳐중";
                AutoCaptureStatus.Foreground = Brushes.Yellow;


            }
            else
            {
                AutoCaptureStatus.Text = "POD";
                AutoCaptureStatus.Foreground = Brushes.White;
            }
            }));

        }

        private void AutoCapture()
        {
            if (CaptureStep == false)
            {
                string FileName = string.Format("EYE_POD_관리자_" + "{0}{1}{2}", SupervisorName.Text, DateTime.Now.ToString("_[yyyy.MM.dd]-[HH-mm-ss]"), ".jpg");
                CaptureFileEasy(FileName);
                CaptureStep = true;
            }
            else
            {
                string FileName = string.Format("EYE_POD_관리자_"+"{0}{1}{2}", SupervisorName.Text, DateTime.Now.ToString("_[yyyy.MM.dd]-[HH-mm-ss.5]"), ".jpg");
                CaptureFileEasy(FileName);
                CaptureStep = false;               
            }
            

        }
        private void CaptureFileEasy(string FileName)
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
            //파일 생성 메서드 인피던스 불러오기 (파일이름과 파일저장변수)
            using (FileStream saveSnapShot = new FileStream(FileName, FileMode.CreateNew))
            {
                // 비트맵 소스의 이미지 저장할 원본 소스 영상 설정
                BitmapSource image = (BitmapSource)ColorImageElement.Source;

                // jpg 변환기            
                JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                jpgEncoder.QualityLevel = 100;
                jpgEncoder.Frames.Add(BitmapFrame.Create(image));
                jpgEncoder.Save(saveSnapShot);

                saveSnapShot.Flush();

                saveSnapShot.Close();
                saveSnapShot.Dispose();
            }
        }

        private void CCTV_WINDOW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("EYE POD 를 종료합니다.","EYE POD");
        }

     
       

       

        private void mapamp(string lat,string lon)
        {
            string maping;
            maping = lat + "," + lon;
            
            string imageURI = GetImagery(maping);

            if (mapstep == false)
            {
                MAP.Opacity = 1;
                MAP.Source = new BitmapImage(new Uri(imageURI));
                MAP2.Opacity = 0.6;
                mapstep = true;

            }
            else
            {
                MAP2.Opacity = 1;
                MAP2.Source = new BitmapImage(new Uri(imageURI));
                MAP.Opacity = 0.6;
                mapstep = false;
            }

           
            
                
            

        }


        private string GetImagery(string locationString)
        {
            string key = "AjA1h-2j-QY5KsjUNXk7a02rF0u11lwMrKyx-I0_7v5bZ9LRmDR2-OdTtiY1bxg4";
            MapUriRequest mapUriRequest = new MapUriRequest();

            // Set credentials using a valid Bing Maps key
            mapUriRequest.Credentials = new ServiceReference2.Credentials();
            mapUriRequest.Credentials.ApplicationId = key;

            // Set the location of the requested image
            mapUriRequest.Center = new ServiceReference2.Location();
            string[] digits = locationString.Split(',');
            mapUriRequest.Center.Latitude = double.Parse(digits[0].Trim());
            mapUriRequest.Center.Longitude = double.Parse(digits[1].Trim());

            // Set the map style and zoom level
            MapUriOptions mapUriOptions = new MapUriOptions();
            mapUriOptions.Style = MapStyle.AerialWithLabels;
            mapUriOptions.ZoomLevel = 18;

            // Set the size of the requested image in pixels
            mapUriOptions.ImageSize = new ServiceReference2.SizeOfint();
            mapUriOptions.ImageSize.Height = 233;
            mapUriOptions.ImageSize.Width = 305;

            mapUriRequest.Options = mapUriOptions;

            //Make the request and return the URI
            ImageryServiceClient imageryService = new ImageryServiceClient("BasicHttpBinding_IImageryService");

            MapUriResponse mapUriResponse = imageryService.GetMapUri(mapUriRequest);
            return mapUriResponse.Uri;


        }
        private void Scan_effect(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (scanstep == true)
                {
                    ImageAni(scanstep, ScanPanel);

                    scanstep = false;
                }
                else
                {
                    ImageAni(scanstep, ScanPanel);
                    scanstep = true;
                }
            }));
        }

        private void ImageAni(bool OnOff, FrameworkElement image)
        {

            string name = image.Name;
            Storyboard OnOffEvent = new Storyboard();
            if (OnOff == true)
            {

                DoubleAnimation OffAni = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(1050));
                Storyboard.SetTargetName(OffAni, name);

                Storyboard.SetTargetProperty(OffAni, new PropertyPath(Image.OpacityProperty));

                OnOffEvent.Children.Add(OffAni);
                OnOffEvent.Begin(this);
            }
            else
            {
                DoubleAnimation OnAni = new DoubleAnimation(0.1, TimeSpan.FromMilliseconds(1050));
                Storyboard.SetTargetName(OnAni, name);

                Storyboard.SetTargetProperty(OnAni, new PropertyPath(Image.OpacityProperty));

                OnOffEvent.Children.Add(OnAni);
                OnOffEvent.Begin(this);





            }
        }

        private void TextAni(bool OnOff, FrameworkElement text)
        {

            string name = text.Name;
            Storyboard OnOffEvent = new Storyboard();
            if (OnOff == true || text.Opacity == 1)
            {

                DoubleAnimation OffAni = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
                Storyboard.SetTargetName(OffAni, name);

                Storyboard.SetTargetProperty(OffAni, new PropertyPath(TextBlock.OpacityProperty));

                OnOffEvent.Children.Add(OffAni);
                OnOffEvent.Begin(this);
            }
            else
            {
                DoubleAnimation OnAni = new DoubleAnimation(1, TimeSpan.FromMilliseconds(600));
                Storyboard.SetTargetName(OnAni, name);

                Storyboard.SetTargetProperty(OnAni, new PropertyPath(TextBlock.OpacityProperty));

                OnOffEvent.Children.Add(OnAni);
                OnOffEvent.Begin(this);





            }
        }



    }

}

        