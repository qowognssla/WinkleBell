﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.ApplicationModel;
using Windows.Foundation;

using Windows.UI.Core;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using System.Diagnostics;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinkleBell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private SuspendingEventHandler appSuspendEventHandler;
        private EventHandler<Object> appResumeEventHandler;

        private ObservableCollection<DeviceListEntry> listOfDevices;

        private Dictionary<DeviceWatcher, String> mapDeviceWatchersToDeviceSelector;
        private Boolean watchersSuspended;
        private Boolean watchersStarted;

        private Boolean isAllDevicesEnumerated;

        //App Version Infomation
        private PackageVersion AppVersion = Package.Current.Id.Version;
        private ObservableCollection<TextBox> RDataList;
        private ObservableCollection<TextBox> GDataList;
        private ObservableCollection<TextBox> BDataList;

        private bool isActive = false;
        public static MainPage Current;

        private CancellationTokenSource ReadCancellationTokenSource;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Object ReadCancelLock = new Object();
        DataWriter DataWriteObject = null;
        DataReader DataReaderObject = null;

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            AppVersionText.Text = string.Format("{0}.{1}.{2}.{3}", AppVersion.Major, AppVersion.Minor, AppVersion.Build, AppVersion.Revision);
            Current = this;
            RDataList = new ObservableCollection<TextBox>();
            // RDataList.Add(RText0);
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

            listOfDevices = new ObservableCollection<DeviceListEntry>();
            mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
            watchersStarted = false;
            watchersSuspended = false;

            isAllDevicesEnumerated = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected || (EventHandlerForDevice.Current.IsEnabledAutoReconnect
                && EventHandlerForDevice.Current.DeviceInformation != null))
            {
                UpdateConnectDisconnectButtonsAndList(false);

                EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
                EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;
            }
            else
            {
                UpdateConnectDisconnectButtonsAndList(true);
            }

            StartHandlingAppEvents();

            InitializeDeviceWatchers();
            StartDeviceWatchers();

            DeviceListSource.Source = listOfDevices;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            StopDeviceWatchers();
            StopHandlingAppEvents();

            EventHandlerForDevice.Current.OnDeviceConnected = null;
            EventHandlerForDevice.Current.OnDeviceClose = null;
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
            WriteButton_Click();
        }

        private async void ConnectBtn_Clicked(Object sender, RoutedEventArgs eventArgs)
        {
            var selection = ConnectDevices.SelectedItems;
            DeviceListEntry entry = null;

            if (selection.Count > 0)
            {
                var obj = selection[0];
                entry = (DeviceListEntry)obj;

                if (entry != null)
                {
                    EventHandlerForDevice.CreateNewEventHandlerForDevice();
                    EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
                    EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;
                    Boolean openSuccess = await EventHandlerForDevice.Current.OpenDeviceAsync(entry.DeviceInformation, entry.DeviceSelector);
                    UpdateConnectDisconnectButtonsAndList(!openSuccess);
                }
            }
        }

        private void DisconnectBtn_Clicked(Object sender, RoutedEventArgs eventArgs)
        {
            isActive = false;
            var selection = ConnectDevices.SelectedItems;
            DeviceListEntry entry = null;

            EventHandlerForDevice.Current.IsEnabledAutoReconnect = false;

            if (selection.Count > 0)
            {
                var obj = selection[0];
                entry = (DeviceListEntry)obj;

                if (entry != null)
                {
                    EventHandlerForDevice.Current.CloseDevice();
                }
            }
            UpdateConnectDisconnectButtonsAndList(true);
        }


        private void InitializeDeviceWatchers()
        {
            var deviceSelector = SerialDevice.GetDeviceSelector();
            var deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
            AddDeviceWatcher(deviceWatcher, deviceSelector);
        }

        private void StartHandlingAppEvents()
        {
            appSuspendEventHandler = new SuspendingEventHandler(this.OnAppSuspension);
            appResumeEventHandler = new EventHandler<Object>(this.OnAppResume);

            App.Current.Suspending += appSuspendEventHandler;
            App.Current.Resuming += appResumeEventHandler;
        }

        private void StopHandlingAppEvents()
        {
            App.Current.Suspending -= appSuspendEventHandler;
            App.Current.Resuming -= appResumeEventHandler;
        }

        private void AddDeviceWatcher(DeviceWatcher deviceWatcher, String deviceSelector)
        {
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(this.OnDeviceEnumerationComplete);

            mapDeviceWatchersToDeviceSelector.Add(deviceWatcher, deviceSelector);
        }

        private void StartDeviceWatchers()
        {
            watchersStarted = true;
            isAllDevicesEnumerated = false;

            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status != DeviceWatcherStatus.Started)
                    && (deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Start();
                }
            }
        }

        private void StopDeviceWatchers()
        {
            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status == DeviceWatcherStatus.Started)
                    || (deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Stop();
                }
            }
            ClearDeviceEntries();

            watchersStarted = false;
        }

        private void AddDeviceToList(DeviceInformation deviceInformation, String deviceSelector)
        {
            var match = FindDevice(deviceInformation.Id);
            if (match == null)
            {
                match = new DeviceListEntry(deviceInformation, deviceSelector);
                listOfDevices.Add(match);
            }
        }

        private void RemoveDeviceFromList(String deviceId)
        {
            var deviceEntry = FindDevice(deviceId);
            listOfDevices.Remove(deviceEntry);
        }

        private void ClearDeviceEntries()
        {
            listOfDevices.Clear();
        }

        private DeviceListEntry FindDevice(String deviceId)
        {
            if (deviceId != null)
            {
                foreach (DeviceListEntry entry in listOfDevices)
                {
                    if (entry.DeviceInformation.Id == deviceId)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        private void OnAppSuspension(Object sender, SuspendingEventArgs args)
        {
            if (watchersStarted)
            {
                watchersSuspended = true;
                StopDeviceWatchers();
            }
            else
            {
                watchersSuspended = false;
            }
        }

        private void OnAppResume(Object sender, Object args)
        {
            if (watchersSuspended)
            {
                watchersSuspended = false;
                StartDeviceWatchers();
            }
        }

        private async void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {


                    RemoveDeviceFromList(deviceInformationUpdate.Id);
                }));
        }

        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    AddDeviceToList(deviceInformation, mapDeviceWatchersToDeviceSelector[sender]);
                }));
        }
        private async void OnDeviceEnumerationComplete(DeviceWatcher sender, Object args)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    isAllDevicesEnumerated = true;

                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);

                        ButtonDisconnectFromDevice.Content = "Disconnect";

                    }
                    else if (EventHandlerForDevice.Current.IsEnabledAutoReconnect && EventHandlerForDevice.Current.DeviceInformation != null)
                    {
                        ButtonDisconnectFromDevice.Content = "Disconnect"; 
                    }
                }));
        }

        private void OnDeviceConnected(EventHandlerForDevice sender, DeviceInformation deviceInformation)
        {
            // Find and select our connected device
            if (isAllDevicesEnumerated)
            {
                SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);
                ButtonDisconnectFromDevice.Content = "Disconnect";
            }

            if (EventHandlerForDevice.Current.Device.PortName != "")
            {
                EventHandlerForDevice.Current.Device.Parity = SerialParity.None;
                EventHandlerForDevice.Current.Device.StopBits = SerialStopBitCount.One;
                EventHandlerForDevice.Current.Device.Handshake = SerialHandshake.None;
                EventHandlerForDevice.Current.Device.DataBits = 8;
                EventHandlerForDevice.Current.Device.BaudRate = 115200;
                ResetReadCancellationTokenSource();
                ResetWriteCancellationTokenSource();

                isActive = true;
                EventHandlerForDevice.Current.Device.ReadTimeout = new System.TimeSpan(10 * 10000);
                ReadButton_Click();
            }
        }
        private async void OnDeviceClosing(EventHandlerForDevice sender, DeviceInformation deviceInformation)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    if (ButtonDisconnectFromDevice.IsEnabled && EventHandlerForDevice.Current.IsEnabledAutoReconnect)
                    {
                        ButtonDisconnectFromDevice.Content = "Disconnect"; 
                    }
                }));
        }

        private void SelectDeviceInList(String deviceIdToSelect)
        {
            ConnectDevices.SelectedIndex = -1;

            for (int deviceListIndex = 0; deviceListIndex < listOfDevices.Count; deviceListIndex++)
            {
                if (listOfDevices[deviceListIndex].DeviceInformation.Id == deviceIdToSelect)
                {
                    ConnectDevices.SelectedIndex = deviceListIndex;
                    break;
                }
            }
        }

        private void UpdateConnectDisconnectButtonsAndList(Boolean enableConnectButton)
        {
            ButtonConnectToDevice.IsEnabled = enableConnectButton;
            ButtonDisconnectFromDevice.IsEnabled = !ButtonConnectToDevice.IsEnabled;
            SetBtn.IsEnabled = !ButtonConnectToDevice.IsEnabled;
            ConnectDevices.IsEnabled = ButtonConnectToDevice.IsEnabled;
        }

        /////////////////////////////////////
        // Read Write Page

        public void Dispose()
        {
            if (ReadCancellationTokenSource != null)
            {
                ReadCancellationTokenSource.Dispose();
                ReadCancellationTokenSource = null;
            }

            if (WriteCancellationTokenSource != null)
            {
                WriteCancellationTokenSource.Dispose();
                WriteCancellationTokenSource = null;
            }
        }
        async private void ReadButton_Click()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);

                    while (isActive)
                        await ReadAsync(ReadCancellationTokenSource.Token);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    DataReaderObject.DetachStream();
                    DataReaderObject = null;
                }
            }

        }
        private async Task ReadAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            lock (ReadCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                loadAsyncTask = DataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                var Str = DataReaderObject.ReadString(bytesRead);
                Debug.Write(Str);
                PlayingSound(CheckReadString(Str));
            }
        }
        private int CheckReadString(string str)
        {
            if (str.Contains("15"))
                return 15;
            else if (str.Contains("14"))
                return 14;
            else if (str.Contains("13"))
                return 13;
            else if (str.Contains("12"))
                return 12;
            else if (str.Contains("11"))
                return 11;
            else if (str.Contains("10"))
                return 10;
            else if (str.Contains("9"))
                return 9;
            else if (str.Contains("8"))
                return 8;
            else if (str.Contains("7"))
                return 7;
            else if (str.Contains("6"))
                return 6;
            else if (str.Contains("5"))
                return 5;
            else if (str.Contains("4"))
                return 4;
            else if (str.Contains("3"))
                return 3;
            else if (str.Contains("2"))
                return 2;
            else if (str.Contains("1"))
                return 1;
            else
                return 0;
        }

        private void CancelReadTask()
        {
            lock (ReadCancelLock)
            {
                if (ReadCancellationTokenSource != null)
                {
                    if (!ReadCancellationTokenSource.IsCancellationRequested)
                    {
                        ReadCancellationTokenSource.Cancel();

                        // Existing IO already has a local copy of the old cancellation token so this reset won't affect it
                        ResetReadCancellationTokenSource();
                    }
                }
            }
        }

        private void ResetReadCancellationTokenSource()
        {
            // Create a new cancellation token source so that can cancel all the tokens again
            ReadCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            ReadCancellationTokenSource.Token.Register(() => NotifyReadCancelingTask());
        }
        private void NotifyReadCancelingTask()
        {

        }
        private async void WriteButton_Click()
        {

            EventHandlerForDevice.Current.Device.ReadTimeout = new System.TimeSpan(10 * 10000);
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    await WriteAsync(WriteCancellationTokenSource.Token);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    DataWriteObject.DetachStream();
                    DataWriteObject = null;

                }
            }
        }
        private async Task WriteAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            if ((WriteBytesInputValue.Text.Length != 0))
            {
                char[] buffer = new char[WriteBytesInputValue.Text.Length];
                WriteBytesInputValue.Text.CopyTo(0, buffer, 0, WriteBytesInputValue.Text.Length);
                String InputString = new string(buffer);
                DataWriteObject.WriteString(InputString);
                WriteBytesInputValue.Text = "";

                lock (WriteCancelLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    storeAsyncTask = DataWriteObject.StoreAsync().AsTask(cancellationToken);
                }

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    Debug.Write(InputString.Substring(0, (int)bytesWritten) + '\n');
                }
            }
        }
        private void CancelWriteTask()
        {
            lock (WriteCancelLock)
            {
                if (WriteCancellationTokenSource != null)
                {
                    if (!WriteCancellationTokenSource.IsCancellationRequested)
                    {
                        WriteCancellationTokenSource.Cancel();
                        ResetWriteCancellationTokenSource();
                    }
                }
            }
        }
        private void ResetWriteCancellationTokenSource()
        {
            WriteCancellationTokenSource = new CancellationTokenSource();
            WriteCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTask());
        }
        private void NotifyWriteCancelingTask()
        {
        }
    }
}
