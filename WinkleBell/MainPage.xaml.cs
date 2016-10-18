using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinkleBell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //App Version Infomation
        private PackageVersion AppVersion = Package.Current.Id.Version;
        private ObservableCollection<Button> BellBtnDataList;
        private ObservableCollection<TextBox> PinDataList;
        private ObservableCollection<TextBox> RDataList;
        private ObservableCollection<TextBox> GDataList;
        private ObservableCollection<TextBox> BDataList;

        private StreamSocket Socket;
        private RfcommDeviceService Service;
        private DeviceInformationCollection Devices;
        private DataWriter BluetoothDataWriter;
        private DataReader dataReaderObject;
        private CancellationTokenSource ReadCancellationTokenSource;

        private bool isActive = false;


        public MainPage()
        {
            //Test 
            //Test2
            this.InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            AppVersionText.Text = string.Format("{0}.{1}.{2}.{3}", AppVersion.Major, AppVersion.Minor, AppVersion.Build, AppVersion.Revision);
            BellBtnDataList = new ObservableCollection<Button>();
            BellBtnDataList.Add(BellBtn0);
            BellBtnDataList.Add(BellBtn1);
            BellBtnDataList.Add(BellBtn2);
            BellBtnDataList.Add(BellBtn3);
            BellBtnDataList.Add(BellBtn4);
            BellBtnDataList.Add(BellBtn5);
            BellBtnDataList.Add(BellBtn6);
            BellBtnDataList.Add(BellBtn7);
            BellBtnDataList.Add(BellBtn8);
            BellBtnDataList.Add(BellBtn9);
            BellBtnDataList.Add(BellBtn10);
            BellBtnDataList.Add(BellBtn11);
            BellBtnDataList.Add(BellBtn12);
            BellBtnDataList.Add(BellBtn13);
            BellBtnDataList.Add(BellBtn14);
            BellBtnDataList.Add(BellBtn15);

            PinDataList = new ObservableCollection<TextBox>();
            PinDataList.Add(PinText0);
            PinDataList.Add(PinText1);
            PinDataList.Add(PinText2);
            PinDataList.Add(PinText3);
            PinDataList.Add(PinText4);
            PinDataList.Add(PinText5);
            PinDataList.Add(PinText6);
            PinDataList.Add(PinText7);
            PinDataList.Add(PinText9);
            PinDataList.Add(PinText10);
            PinDataList.Add(PinText11);
            PinDataList.Add(PinText12);
            PinDataList.Add(PinText13);
            PinDataList.Add(PinText14);
            PinDataList.Add(PinText15);

            RDataList = new ObservableCollection<TextBox>();
            RDataList.Add(RText0);
            RDataList.Add(RText1);
            RDataList.Add(RText2);
            RDataList.Add(RText3);
            RDataList.Add(RText4);
            RDataList.Add(RText5);
            RDataList.Add(RText6);
            RDataList.Add(RText7);
            RDataList.Add(RText8);
            RDataList.Add(RText9);
            RDataList.Add(RText10);
            RDataList.Add(RText11);
            RDataList.Add(RText12);
            RDataList.Add(RText13);
            RDataList.Add(RText14);
            RDataList.Add(RText15);

            GDataList = new ObservableCollection<TextBox>();
            GDataList.Add(GText0);
            GDataList.Add(GText1);
            GDataList.Add(GText2);
            GDataList.Add(GText3);
            GDataList.Add(GText4);
            GDataList.Add(GText5);
            GDataList.Add(GText6);
            GDataList.Add(GText7);
            GDataList.Add(GText8);
            GDataList.Add(GText9);
            GDataList.Add(GText10);
            GDataList.Add(GText11);
            GDataList.Add(GText12);
            GDataList.Add(GText13);
            GDataList.Add(GText14);
            GDataList.Add(GText15);

            BDataList = new ObservableCollection<TextBox>();
            BDataList.Add(BText0);
            BDataList.Add(BText1);
            BDataList.Add(BText2);
            BDataList.Add(BText3);
            BDataList.Add(BText4);
            BDataList.Add(BText5);
            BDataList.Add(BText6);
            BDataList.Add(BText7);
            BDataList.Add(BText8);
            BDataList.Add(BText9);
            BDataList.Add(BText10);
            BDataList.Add(BText11);
            BDataList.Add(BText12);
            BDataList.Add(BText13);
            BDataList.Add(BText14);
            BDataList.Add(BText15);

            try
            {
                Devices =
                await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

                ObservableCollection<TextBlock> BluetoothDeivces = new ObservableCollection<TextBlock>(); ;

                for (int i = 0; i < Devices.Count; i++)
                {
                    TextBlock Temp = new TextBlock();
                    Temp.Text = Devices[i].Name;
                    Debug.WriteLine(Devices[i].Name);
                    BluetoothDeivces.Add(Temp);
                }

                BluetoothCombo.ItemsSource = BluetoothDeivces;

                if (BluetoothDeivces.Count > 0)
                    BluetoothCombo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void ConnectBtn_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var DeivceName = (TextBlock)BluetoothCombo.SelectedItem;
                Debug.WriteLine(DeivceName.Text);
                var Device = Devices.Single(x => x.Name == DeivceName.Text);
                Debug.WriteLine(Device.Name);
                Service = await RfcommDeviceService.FromIdAsync(Device.Id);

                Socket = new StreamSocket();

                await Socket.ConnectAsync(Service.ConnectionHostName, Service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                BluetoothDataWriter = new DataWriter(Socket.OutputStream);

                isActive = true;
                Listen();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }

        }

        private async void DisconnectBtn_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                await Socket.CancelIOAsync();
                Socket.Dispose();
                Socket = null;
                Service.Dispose();
                Service = null;
                isActive = false;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private async Task<uint> Send(string msg)
        {
            try
            {
                BluetoothDataWriter.WriteString(msg);

                // Launch an async task to 
                //complete the write operation
                var store = BluetoothDataWriter.StoreAsync().AsTask();

                return await store;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();

                return 0;
            }
        }


        private async void PlayingSound(int Index, double Volume = 0.01)
        {

            MediaElement Sound = new MediaElement();

            string Mode = ((TextBlock)SoundModeCombo.SelectedItem).Text;
            try
            {
                Windows.Storage.StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets\\" + Mode);
                Windows.Storage.StorageFile file = await folder.GetFileAsync("sound" + Index + ".mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Sound.SetSource(stream, file.ContentType);
                Sound.Volume = Volume;
                Sound.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }


        private async void BellBtn0_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var SelectedBtn = sender as Button;
            int SelectedIndex = BellBtnDataList.IndexOf(SelectedBtn);
            try
            {
                PlayingSound(SelectedIndex);
                await Send(SelectedIndex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void Listen()
        {
            ReadCancellationTokenSource = new CancellationTokenSource();
            if (Socket.InputStream != null)
            {
                dataReaderObject = new DataReader(Socket.InputStream);
                // keep reading the serial input
                while (isActive)
                {
                    await ReadAsync(ReadCancellationTokenSource.Token);
                    await Task.Delay(1);

                }
            }
        }
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            var loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                string recvdtxt = dataReaderObject.ReadString(bytesRead);
                int RevInteger = int.Parse(recvdtxt);

                     Debug.WriteLine((RevInteger / 100));
                     Debug.WriteLine(RevInteger % 100);
                 if ((RevInteger / 100) > 0)
                {
                    Debug.WriteLine("DD");
                   //  PlayingSound(RevInteger % 100);
                 }
            }
        }
    }
}
