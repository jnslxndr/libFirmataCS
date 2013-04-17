using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Firmata;
using Firmata.Extensions;

namespace Firmata {
  public class Decoder : IFormattable {
    int remaining = 0;
    byte lastCommand = Command.RESERVED_COMMAND;
    public Queue<byte> buffer = new Queue<byte>();

    public event OnSysex SysexEvent;
    public event OnAnalog AnalogEvent;
    public event OnDigital DigitalEvent;
    public event OnPinMode PinModeEvent;
    public event OnFirmwareReport FirmwareReportEvent;


    public Decoder() : this(Stream.Null) {
    }

    public Decoder(Stream s, bool DoParse=true) {
      remaining = 0;
      lastCommand = Command.RESERVED_COMMAND;
      buffer = new Queue<byte>(Constants.MAX_DATA_BYTES);

      SysexEvent += OnSysex;

      BaseStream = s ?? Stream.Null;

      if (DoParse) Parse();
    }

    public Stream BaseStream { get; set; }

    public void Parse(Stream stream=null) {
      stream = stream ?? this.BaseStream;
      foreach (byte b in stream.Bytes()) {
        Decode(b);
      }
    }

    public void Push(char c) {
      Decode((byte) c);
    }
    public void Push(byte b) {
      Decode(b);
    }

    protected void Decode( byte data ) {
      // Check if the 8th bit is set, then we have a command
      if ((data & 0x80) > 0) {
        byte cmd = Util.GetCommand( data );

        lastCommand = cmd;
        switch (cmd) {
          case Command.SYSTEM_RESET:
            Console.WriteLine("System reset, please!");
            buffer.Clear();
            break;
          case Command.DIGITAL_MESSAGE:
          case Command.ANALOG_MESSAGE:
            remaining = 2;
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.REPORT_VERSION:
          case Command.SET_PIN_MODE:
            remaining = 2;
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.REPORT_DIGITAL:
          case Command.REPORT_ANALOG:
            remaining = 1;
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.SYSEX_START:
            buffer.Clear();
            buffer.Enqueue(data);
            break;
          case Command.SYSEX_END:
            // Fire Sysex event
            if (SysexEvent!=null) {
              buffer.Dequeue(); // pop off the sysex start command
              SysexEvent.Invoke(this,new SysexEventArgs(buffer.Dequeue(),buffer.ToArray()));
            }
            break;
          default:
            // unknown command
            break;
        }
      }
      else {
        buffer.Enqueue(data);
        if (--remaining==0) {
          // process the message
          switch (lastCommand) {
            case Command.ANALOG_MESSAGE:
              int pin,value;
              Util.DecodeAnalogMessage(buffer.ToArray(), out pin, out value);
              if (AnalogEvent!=null) AnalogEvent.Invoke(this, new AnalogMessageEventsArgs(pin,value));
              break;
            case Command.DIGITAL_MESSAGE:
              int port;
              int[] values;
              Util.DecodeDigitalMessage(buffer.ToArray(),out port, out values);
              if (DigitalEvent!=null) DigitalEvent.Invoke(this, new DigitalMessageEventsArgs(port,values));
              break;
            case Command.REPORT_DIGITAL:
            case Command.REPORT_ANALOG:
              //Console.WriteLine("Toogle analog/digital pin reporting ({0})", BitConverter.ToString(buffer.ToArray()));
              break;
            case Command.SET_PIN_MODE:
              // Console.WriteLine("Please set pin mode ({0})", BitConverter.ToString(buffer.ToArray()));
              if (PinModeEvent!=null) PinModeEvent.Invoke(this, null);
              break;
            case Command.REPORT_VERSION:
              int major = (int) buffer.Dequeue();
              int minor = (int) buffer.Dequeue();
              if (FirmwareReportEvent!=null) FirmwareReportEvent.Invoke(this, new FirmwareReportEventArgs(major,minor,""));
              break;
            default:
              // unkown byte...
              break;
          }

        }
      }
    }

    protected void OnSysex(object sender, SysexEventArgs Args) {
      switch (Args.Command) {
        case Command.REPORT_FIRMWARE:
          if (Args.Data==null || Args.Data.Length <= 0) break;
          Queue<byte> buffer = new Queue<byte>(Args.Data);
          int major = (int) buffer.Dequeue();
          int minor = (int) buffer.Dequeue();
          string name = Encoding.ASCII.GetString(buffer.ToArray().From7BitAsBytes());
          if (FirmwareReportEvent!=null) FirmwareReportEvent.Invoke(this, new FirmwareReportEventArgs(major,minor,name));
          break;
        case Command.SAMPLING_INTERVAL:
          //
          int interval = Args.Data.From7Bit()[0];
          Console.WriteLine("Sampling interval message received! New Interval: {0}",interval);
          break;

        case Command.CAPABILITY_RESPONSE:
          string report = "";
          int pinCount = 0;

          int digitalPins = 0;
          int analogPins = 0;
          int servoPins = 0;
          int pwmPins = 0;
          int shiftPins = 0;
          int i2cPins = 0;

          for(int a=0; a<Args.Data.Length; a++) {
            pinCount++;
            report += "Pin "+pinCount.ToString()+":\r\n";
            while(Args.Data[a]!=0x7f) {
              PinMode mode = (PinMode) Args.Data[a++];
              switch(mode) {
                case PinMode.ANALOG:
                  analogPins++;
                  break;
                case PinMode.INPUT:
                case PinMode.OUTPUT:
                  digitalPins++;
                  break;
                case PinMode.SERVO:
                  servoPins++;
                  break;
                case PinMode.PWM:
                  pwmPins++;
                  break;
                case PinMode.I2C:
                  i2cPins++;
                  break;
                case PinMode.SHIFT:
                  shiftPins++;
                  break;
              }

              int resolution = Args.Data[a++];
              report += "  Mode: "+Util.PinModeToString(mode);
              report += "("+resolution.ToString()+" bit)\r\n";
            }
          }
          digitalPins /= 2;
          report += "Total number of pins: "+pinCount.ToString()+"\r\n";
          report += string.Format("{0} digital, {1} analog, {2} servo, {3} pwm and {4} i2c pins\r\n",digitalPins,analogPins,servoPins,pwmPins,i2cPins);
          Console.WriteLine(report);
          break;
      }
    }

    public string ToString( string s, IFormatProvider provider ) {
      return s;
    }

  }
}

