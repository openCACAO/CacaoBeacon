using CoreBluetooth;
using Foundation;
using System;
using UIKit;

namespace CocoaBeaconMonitor.iOS
{
    public partial class ViewController : UIViewController
    {

        CBUUID serviceUUID = CBUUID.FromString("");
        CBCentralManager manager;

        public ViewController (IntPtr handle) : base (handle)
        {
            // ここでBluetoothの権限を要求する
            this.manager = new CBCentralManager();
            this.manager.DiscoveredPeripheral += Manager_DiscoveredPeripheral;
            this.manager.UpdatedState += Manager_UpdatedState;
        }


        /// <summary>
        /// スキャン開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_TouchDown(object sender, EventArgs e)
        {
            if ( this.manager.State == CBCentralManagerState.PoweredOn)
            {
                this.manager.ScanForPeripherals(
                    this.serviceUUID, 
                    new PeripheralScanningOptions() { AllowDuplicatesKey = true }.Dictionary);
            }
        }
        /// <summary>
        /// ステータス変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_UpdatedState(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Manager_DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
        {
            // アバタイズのデータを受信
            var dic = e.AdvertisementData;
            var peripheral = e.Peripheral;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}