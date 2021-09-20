#ifdef ARDUINO_M5Stack_Core_ESP32
#include <M5Stack.h>
#endif
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>
#include <list>


const char* uuid = "0000fd6f-0000-1000-8000-00805f9b34fb";
unsigned int count = 0;
std::list<std::string> lst ;

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
    void onResult(BLEAdvertisedDevice advertisedDevice) {
        if(advertisedDevice.haveServiceUUID()){
            if(strncmp(advertisedDevice.getServiceUUID().toString().c_str(),uuid, 36) == 0){
                std::string s = toRPI(advertisedDevice.getServiceData().data());
                lst.push_back( s );
                count++;
            }
        }
    }

    char *toRPI( const char *str ) {
      static char buf[32+1] ;
      static char hex[] = "0123456789ABCDEF";
      for ( int i=0; i<16; i++ ) {
        buf[i*2]   = hex[(str[i] >> 4) & 0xF];
        buf[i*2+1] = hex[str[i] & 0xF];
      }
      buf[32] = 0x00;
      return buf;
    }
};



void setup() {
    M5.begin();
    BLEDevice::init("");
}

void loop(){
    count = 0;
    BLEScan* pBLEScan = BLEDevice::getScan();
    pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
    pBLEScan->setActiveScan(true);
    pBLEScan->start(5, false);

    M5.Lcd.fillScreen(BLACK);
    M5.Lcd.setCursor(20,12,6);
    M5.Lcd.println(count);
    M5.Lcd.setCursor(40,0,2);
    M5.Lcd.println("COCOA users here");
    for ( auto it = lst.begin(); it != lst.end(); ++it ) {
      M5.Lcd.println( (*it).c_str());
    }
    lst.clear();
    
    delay(1000);
    M5.update();
}
