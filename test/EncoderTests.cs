using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO.Ports;

using Firmata;
using Firmata.Extensions;

namespace LibVirmata {

  [TestFixture()]
	public class EncoderTests {

    [Test()]
    public void TestEncodeDecode () {

      Encoder encoder = new Encoder();

      encoder.SystemReset();
      Assert.AreEqual(1,encoder.Count);

      encoder.SampleRate = 50; // Set to 50 Hz
      Assert.AreEqual(6,encoder.Count);

      Console.WriteLine(encoder);

      encoder.RequestFirmwareInformation();
      Assert.AreEqual(9,encoder.Count);

      Console.WriteLine(encoder);

      //encoder.DigitalWrite(9,1);

      //encoder.AnalogWrite(15, 64);
      //Assert.AreEqual(15,encoder.Count);

      //encoder.DigitalWrite(11,1);
      //Assert.AreEqual(15,encoder.Count);

      //encoder.AnalogWrite(1, 768);
      //Assert.AreEqual(18,encoder.Count);

      Console.WriteLine(encoder);


      for (int i = 0; i < 16; i++) {
        for (int j = 0; j < 256; j++) {
          encoder.AnalogWrite(i, j );
        }
      }

      // Console.WriteLine(encoder);

      encoder.DigitalWrite(new int[128]);

      encoder.DigitalWrite(new int[] { 1, 1, 0, 0, 0, 0, 0, 0});

      Console.WriteLine(encoder);


      /// ------------------------------------
      // Decode the formerly encoded stream

      Decoder decoder = new Decoder();
      decoder.AnalogEvent  +=  OnAnalog;
      decoder.DigitalEvent += OnDigital;
      decoder.SysexEvent += OnSysex;

      decoder.Parse(encoder.BaseStream);


      Assert.That (true);
    }

    [Test()]
    public void TestPartialData() {
      Console.WriteLine("Testing partial data insertion...");

      Decoder decoder = new Decoder();
      decoder.AnalogEvent  +=  OnAnalog;
      decoder.DigitalEvent += OnDigital;
      decoder.SysexEvent += OnSysex;

      Stream stream = new MemoryStream();

      stream.Write(new byte[] { 0xFF, 0xF0, 0x7A }, 0, 3);

      Console.WriteLine("First parse!");
      decoder.Parse(stream);
      Console.WriteLine("Internal buffer {0}: {1}",decoder.buffer.Count.ToString(),BitConverter.ToString(decoder.buffer.ToArray()));

      stream.Close();
      stream = new MemoryStream();
      stream.Write(new byte[] { 0x14, 0x00, 0xF7 }, 0, 3);

      Console.WriteLine("Second parse!");
      decoder.Parse(stream);
      Console.WriteLine("Internal buffer {0}: {1}",decoder.buffer.Count.ToString(),BitConverter.ToString(decoder.buffer.ToArray()));

    }

    [Test()]
    public void TestWithPort(){
      SerialPort port;

      // port = new SerialPort("/dev/tty.usbserial-00002014",57600);
      port = new SerialPort("/dev/tty.usbserial-A9007VQg",57600);

      Encoder encoder = new Encoder();

      encoder.RequestCapabilityReport();
      encoder.RequestFirmwareInformation();

      Decoder decoder = new Decoder();
      decoder.AnalogEvent  += OnAnalog;
      decoder.DigitalEvent += OnDigital;
      decoder.SysexEvent   += OnSysex;
      decoder.FirmwareReportEvent += (sender, args) => {
        FirmwareReportEventArgs firmware = args as FirmwareReportEventArgs;
        Console.WriteLine("Firmware version report: {0} ({1}.{2})",firmware.Name,firmware.Major,firmware.Minor);
      };

      port.Open();
      while(!port.IsOpen){}

      Thread.Sleep(5000);

      Console.WriteLine("Sending encoder to port");
      port.ReadExisting(); // Clear the receive buffer
      port.Write(encoder.BaseStream.ToArray(),0, (int) encoder.Count);

      Thread.Sleep(1000);

      Console.WriteLine("Bytes to read {0}",port.BytesToRead);

      while (port.BytesToRead > 0) {
        decoder.Push((byte) port.ReadByte());
      }

      decoder.ToString();
      //Thread.Sleep(5000);

      port.Close();

      Console.WriteLine("Test with serial port done");
    }


    void OnSysex (object sender, SysexEventArgs args)
    {
      /*
      Console.WriteLine("Sysex messge received (Command: {1}, {0})",
                        BitConverter.ToString(args.Data),
                        args.Command.ToFirmataCommandString());
                        */
    }
    

    public void OnAnalog(object sender, AnalogMessageEventsArgs args) {
      Console.WriteLine("Analog event for pin {0} with value {1}", args.Pin, args.Value);
    }

    public void OnDigital(object sender, DigitalMessageEventsArgs args) {
      Console.WriteLine("Digital event for port {0} with values {1}", args.Port, args.Values.ToString("X"));
    }
  }


}

