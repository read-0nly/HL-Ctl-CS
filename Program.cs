using System;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace BleConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var watcher = new BluetoothLEAdvertisementWatcher();
            watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };

            watcher.Received += Watcher_Received;
            watcher.Start();

            await Task.Delay(Timeout.Infinite);
        }

        private static async void Watcher_Received(
            BluetoothLEAdvertisementWatcher sender,
            BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (device != null)
            {
                if(device.DeviceInformation!=null && device.DeviceInformation.Name.Contains("QHM"))
                {
                    if(string.Format("{0:X}", device.BluetoothAddress)== "720513000274")
                    {
                        await device.RequestAccessAsync();
                        await device.GetGattServicesAsync();
                        foreach(GattDeviceService service in device.GattServices)
                        {
                            Console.WriteLine("Found serv " + service.Uuid);
                            await service.RequestAccessAsync();
                            GattCharacteristicsResult gcr = await service.GetCharacteristicsAsync();
                            foreach(var x in gcr.Characteristics)
                            {
                                GattCharacteristicProperties properties = x.CharacteristicProperties;
                                if (properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse) && properties.HasFlag(GattCharacteristicProperties.Write))
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Found TX");
                                    Console.ResetColor();
                                }
                                else if (service.Uuid.ToString() == "00001800-0000-1000-8000-00805f9b34fb")
                                {

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Found RX");
                                    Console.ResetColor();
                                }
                                Console.WriteLine(properties.ToString());
                            }

                        }
                    }

                }
                /*
                 * This passes tiny red line check - still gotta figure out everything between advertisement and this.
                Windows.Storage.Streams.IBuffer buff = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(
    "What fools these mortals be", Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
                await device.GetGattService(new Guid()).GetCharacteristics(new Guid())[0].WriteValueAsync(buff);

                */
            }
        }
    }
}