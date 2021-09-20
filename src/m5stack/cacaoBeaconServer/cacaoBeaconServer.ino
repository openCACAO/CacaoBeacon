#ifdef ARDUINO_M5Stack_Core_ESP32
#include <M5Stack.h>
#endif
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>


#define BEACON_UUID "0000fd6f-0000-1000-8000-00805f9b34fb"

char RPI[16] = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
char METADATA[4] { 0x00, 0x00, 0x00, 0x00 };



void setAdvData(BLEAdvertising *pAdvertising) {

    BLEAdvertisementData oAdvertisementData;
    oAdvertisementData.setFlags(0x1A);
    std::string strServiceData = "";
    // Complete 16-bit Service UUID
    strServiceData += (char)0x03; // length
    strServiceData += (char)0x03; // type
    strServiceData += (char)0x6F; // uuid
    strServiceData += (char)0xFD;
    // Service Data 16-bit Service UUID
    strServiceData += (char)0x17; // length
    strServiceData += (char)0x16; // type
    strServiceData += (char)0x6F; // uuid
    strServiceData += (char)0xFD;
    for ( int i=0; i<16; i++ ) {  // RPI 16 bytes
      strServiceData += (char)RPI[i];
    }
    for ( int i=0; i<4; i++ ) {   // meatdata 4 byes
      strServiceData += (char)METADATA[4];
    }
    oAdvertisementData.addData(strServiceData);
    pAdvertising->setAdvertisementData(oAdvertisementData);
}

BLEServer *pServer ;

void setup() {
    M5.begin();
    M5.Lcd.print("start Beacon");    
    BLEDevice::init("");                    // デバイスを初期化
    BLEDevice::setPower(ESP_PWR_LVL_N12);   // パワーを設定
    pServer = BLEDevice::createServer();    // サーバーを生成
}

void loop() {
    M5.Lcd.setTextFont(4);

    // TODO: ちょうど "Advertizing" が表示する前にボタンを押さないといけない
    M5.update();
    // RPI の先頭を書き換える
    if ( M5.BtnA.wasPressed()){
      RPI[0] = RPI[0] + 1 ;
    }
    
    // アドバタイズオブジェクトを取得
    BLEAdvertising *pAdvertising = pServer->getAdvertising(); 
    // アドバタイジングデーターをセット
    setAdvData(pAdvertising);                          
    // 起動
    pAdvertising->start(); 
     .setCursor(0, 0);
    M5.Lcd.println("Advertizing");  
    // RPIの先頭だけ表示する
    M5.Lcd.printf("RPI: 0x%02X", RPI[0] );  
    delay(1000);             // 1秒アドバタイズする
    pAdvertising->stop();    // アドバタイズ停止

    M5.Lcd.setCursor(0, 0);
    M5.Lcd.println("Enter sleep");    
    delay(5000);             // 5秒スリープ

}
