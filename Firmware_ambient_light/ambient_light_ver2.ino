/*
 * Application: A,bietLight firmware
 * Date: 24.02.2020
 * Author: Quang Hai Nguyen
 * Version: 1.0.0
 */

/* Include Section*/
#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
  #include <avr/power.h>
#endif


/* Define Section*************************************************************/
// On a Trinket or Gemma we suggest changing this to 1
#define PIN            6

// How many NeoPixels are attached to the Arduino?
#define NUMPIXELS      22

/* Variable Section***********************************************************/
Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUMPIXELS, PIN, NEO_GRB + NEO_KHZ800);

String inputString = "";
boolean stringComplete = false;  // whether the string is complete

String index_str = "";
String r_str = "";
String g_str = "";
String b_str = "";

boolean command_found = false;
boolean r_found = false;
boolean g_found = false;
boolean b_found = false;

int delayval = 1;

/* Setup Section**************************************************************/
void setup() 
{
  // initialize serial:
  Serial.begin(115200);

  // This is for Trinket 5V 16MHz, you can remove these three lines if you are not using a Trinket
  #if defined (__AVR_ATtiny85__)
    if (F_CPU == 16000000) clock_prescale_set(clock_div_1);
  #endif
  // End of trinket special code

  // This initializes the NeoPixel library.
  pixels.begin();
   
  // reserve bytes for the String:
  inputString.reserve(500);
  index_str.reserve(10);
  r_str.reserve(10);
  g_str.reserve(10);
  b_str.reserve(10);
  ResetRGB();
  
}

/* Main Section***************************************************************/
void loop() 
{
  /*
   * If receive stringComplete Flag
   * send ack to PC software 
   * Process the data
   */
  if (stringComplete) 
  {
    SetLight();
    Serial.print("ack\n");
    inputString = ""; 
    stringComplete = false;
  }
}

/*
 * Turn off the RGB strip
 */
void ResetRGB()
{
  for(int i=0;i<NUMPIXELS;i++)
  {  
    // Set the color of the complete LED strip to dark/black
    pixels.setPixelColor(i, pixels.Color(0,0,0));
    pixels.show();
  }
}

/*
 * Interrupt handler of the serial port
 */
void serialEvent() 
{
  /*
   * If we have available byte on the serial, read it and store to buffer
   * if we receive line feed, notify the statemachine to process the data stream
   */
  while (Serial.available()) 
  {
    // get the new byte:
    char inChar = (char)Serial.read();
    inputString += inChar;
    if(inChar == '\n' )
      stringComplete = true;    
  }
}

/*
 * Set the RGB strip according to the data we receive
 */
 void SetLight(void)
 {
    //Scan thru the data buffer and looking the RGB data 
    //Buffer has the format Ar123g132b232...r231g213b232E\n
    int index = 0;
    while(inputString[index] != '\n')
    {
      switch(inputString[index])
      {
        
        case 'E':
          b_found = false;
          pixels.setPixelColor(index_str.toInt(), pixels.Color(r_str.toInt(), g_str.toInt(), b_str.toInt()));
          pixels.show(); // This sends the updated pixel color to the hardware.
          //delay(delayval);
          index_str = "";
          r_str = "";
          g_str = "";
          b_str = ""; 
          break;
          
        case 'A':
          command_found = true;
          break;
        
        case 'r':
          command_found = false;
          r_found = true;
          break;
        
        case 'g':
          r_found = false;
          g_found = true;
          break;
        
        case 'b':
          g_found = false;
          b_found = true;
          break;
        
        default:
          if(command_found)
          {
            index_str += inputString[index];
          }
          if(r_found)
          {
            r_str += inputString[index];
          }
          
            if(g_found)
          {
            g_str += inputString[index];
          }
          
          if(b_found)
          {
            b_str += inputString[index];
          }
          break;        
        }
        index++; 
    }
    
 }
