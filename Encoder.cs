using System;
using System.IO;

using Firmata.Extensions;

namespace Firmata {

  public class I2CRequest {
    public I2CRequest() {
      Mode = I2CMode.READ;
      Data = new int[ Defaults.MaxDataBytes ];
      Use10BitAddress = false;
    }

    public int Address;
    public I2CMode Mode;
    public int[] Data;
    public bool Use10BitAddress {
     get { return (Mode & I2CMode.TENBIT) > 0; }
     set {
        if (value) Mode |= I2CMode.TENBIT;
        else Mode &= ~I2CMode.TENBIT;
      }
    }
    public byte[] Command {
      set {}
      get {
        return Util.I2CRequest(Address,Data,Mode);
      }
    }
  }

  public class Encoder : IFormattable {

    int[][] DigitalPins;
    bool[]  DigitalPortsChanged;
    long[]  DigitalMessages;

    int[]   AnalogPins;
    bool[]  AnalogPinsChanged;
    long[]  AnalogMessages;

    int BitsPerPort = 8;

    ByteWriter Writer;

    public Encoder() : this(8) {} // Default to 8 Bits per port
    public Encoder(int BPP) : this(new MemoryStream(4096),BPP) {}
    public Encoder(Stream s, int BPP) {
      OutStream = s==null ? Stream.Null : s;

      BitsPerPort = BPP;

      DigitalPins = new int[Constants.MaxDigitalPorts][];
      DigitalMessages = new long[Constants.MaxDigitalPorts];

      for (int i=0; i < Constants.MaxDigitalPorts; i++) {
        DigitalPins[i] = new int[BitsPerPort];
      }

      DigitalPortsChanged = new bool[Constants.MaxDigitalPorts];

      AnalogPins          = new int[Constants.MaxAnalogPins];
      AnalogPinsChanged   = new bool[Constants.MaxAnalogPins];
      AnalogMessages      = new long[Constants.MaxAnalogPins];
    }

    Stream _OutStream;
    Stream OutStream {
      set {
        _OutStream = value;
        Writer = new ByteWriter(_OutStream);
      }
      get {
        return _OutStream;
      }
    }

    public Stream BaseStream {
      get { return OutStream; }
      set { if (value!=null) OutStream = value; }
    }

    public long Count {
      get { return Writer.BaseStream.Length; }
    }

    public bool Changed {
     get {
        foreach (bool state in DigitalPortsChanged) {
          if (state) return true;
        }
        foreach (bool state in AnalogPinsChanged) {
          if (state) return true;
        }
        return false;
      }
     set {} // read-only
    }
 
    public void DigitalWrite(int pin, int value) {
      int port = pin / BitsPerPort;
      int bit  = pin % BitsPerPort;

      if (DigitalPins[port][bit] == value) return;

      DigitalPins[port][bit] = value;

      WriteDigitalMessage(port);
    }

    public void DigitalWrite(int[] values) {
      int port = -1;
      for (int i=0; i<Math.Min(values.Length,Constants.MaxDigitalPins); i++) {
        port = i / BitsPerPort;
        DigitalWrite(i,values[i]);
        if (i % BitsPerPort == BitsPerPort-1) {
          WriteDigitalMessage(port,false);
        }
      }
      if ( port >= 0 ) WriteDigitalMessage(port);
    }

    void WriteDigitalMessage(int port, bool DoFlush=true) {

      if (DigitalPortsChanged[port]) {
        // remember the position in the stream
        long lastPos = Writer.BaseStream.Position;

        // seek to the position 
        Writer.Seek((int)DigitalMessages[port],SeekOrigin.Begin);

        // override the message on the stream
        Writer += Util.EncodeDigitalMessage(port,DigitalPins[port]);

        // go back to the last position
        Writer.Seek((int)lastPos,SeekOrigin.Begin);
      } else {
        // remember the position in the stream
        DigitalMessages[port] = Writer.BaseStream.Position;
        // append the message to the stream
        Writer += Util.EncodeDigitalMessage(port,DigitalPins[port]);
      }

      // Flush:
      if (DoFlush) Writer.Flush();
      // and flag as changed:
      DigitalPortsChanged[port] = true;
    }

    public void AnalogWrite(int pin, int value) {
      if (AnalogPins[pin] == value) return;
      AnalogPins[pin] = value;

      if(AnalogPinsChanged[pin]) {
        long pos = Writer.BaseStream.Position;
        Writer.Seek((int)AnalogMessages[pin],SeekOrigin.Begin);
        Writer.Write(Util.EncodeAnalogMessage(pin,AnalogPins[pin]));
        Writer.Seek((int)pos,SeekOrigin.Begin);
      } else {
        AnalogMessages[pin] = Writer.BaseStream.Position;
        Writer.Write(Util.EncodeAnalogMessage(pin,AnalogPins[pin]));
        AnalogPinsChanged[pin] = true;
      }
    }

    public void AnalogWrite(int[] values) {
      for (int i=0; i<Math.Min(values.Length,Constants.MaxAnalogPins); i++) {
        AnalogWrite(i,values[i]);
      }
    }

    public void SystemReset() {
      Writer.Write(Util.SystemReset());
    }

    public void RequestFirmwareVersion() {
      Writer.Write(Util.RequestFirmwareVersion());
    }

    public void RequestFirmwareInformation() {
      Writer.Write(Util.RequestFirmwareInformation());
    }

    public void RequestCapabilityReport() {
      Writer.Write(Util.RequestCapabilityreport());
    }

    public void RequestAnalogMapping() {
      Writer.Write(Util.RequestAnalogMapping());
    }

    public void RequestPinState() {
      Writer.Write(Util.RequestPinState());
    }

    /// <summary>
    /// Set sampling rate or interval
    /// </summary>
    int _SamplingInterval = Defaults.SamplingInterval;
    public int SamplingInterval {
      get { return _SamplingInterval; }
      set {
        _SamplingInterval = Math.Max(1,value); // Keep away from 0 or negative values
        Writer.Write(Util.SetSamplingInterval(_SamplingInterval));
      }
    }
    public double SampleRate {
      get {
        return 1 / _SamplingInterval * 1000;
      }
      set {
        SamplingInterval = (int) (1 / value * 1000);
      }
    }

    public string ToString(string s, IFormatProvider provider) {
      s = s==null ? "{0}" : s;
      return string.Format(provider, s, ToString ());
    }

    public override string ToString() {
      /*
      byte[] buffer = new byte[OutStream.Length];

      OutStream.Seek(0,SeekOrigin.Begin);
      OutStream.Read(buffer,0,(int)OutStream.Length);
      OutStream.Seek(0,SeekOrigin.End);
      */
      return string.Format("Firmata Encoder ({0}): {1}",Count,BitConverter.ToString(OutStream.ToArray()));
    }
  }
}

