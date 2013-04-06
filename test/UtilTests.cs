using System;
using NUnit.Framework;

/* Use our library */
using Firmata;

namespace LibVirmata {

  [TestFixture()]
	public class ByteUtilsTest {

    [Test()]
    public void TestLSBWithFixedNumber () {
      int val = 0x0000;
      Assert.AreEqual(0x00,Firmata.Util.LSB (val) );

      val = 0x00FF;
      Assert.AreEqual(0x7F,Firmata.Util.LSB (val) );

      val = 0xFF7F;
      Assert.AreEqual(0x7F,Firmata.Util.LSB (val) );
    }

    [Test()]
    public void TestLSBWithRange () {
      for (int i=0; i<0xFFFF; i++) {
        byte val = Firmata.Util.LSB(i);
        Assert.GreaterOrEqual( val, 0);
        Assert.LessOrEqual( val, 0x7F );
      }
    }

    [Test()]
    public void TestMSBWithFixedNumber () {
      int val = 0x0000;
      Assert.AreEqual(0x00,Firmata.Util.MSB (val) );

      val = 0x3FFF;
      Assert.AreEqual(0x7F,Firmata.Util.MSB (val) );

      val = 0x1FFF;
      Assert.AreEqual(0x3F,Firmata.Util.MSB (val) );
    }

    [Test()]
    public void TestMSBWithRange () {
      for (int i=127; i<0xFFFF; i++) {
        byte val = Firmata.Util.MSB(i);
        Assert.GreaterOrEqual( val, 0);
        Assert.LessOrEqual( val, 0x7F );
      }
    }

    [Test()]
    public void TestBytesToInt() {
      int msb = 0xFF;
      int lsb = 0xFF;
      int val = Firmata.Util.FromBytes( (byte)lsb, (byte)msb );
      Assert.AreEqual(0x3FFF, val);
      Assert.AreNotEqual(0xFFFF, val);
    }

    [Test()]
    public void TestIntToBytes() {
      byte lsb, msb;
      int val = 0xFFFF;
      Firmata.Util.ToBytes(val, out lsb, out msb);
      Assert.AreEqual(0x7F, lsb);
      Assert.AreEqual(0x7F, msb);

      val = 0x0000;
      Firmata.Util.ToBytes(val, out lsb, out msb);
      Assert.AreEqual(0x00, lsb);
      Assert.AreEqual(0x00, msb);
    }
  }

  [TestFixture()]
  public class EncodeMessageTest {

    [Test()]
    public void TestDigitalMessageEncoding8Pins () {
      int port = 1;
      int[]  values = { 1, 1, 1, 1, 1, 1, 1, 1 };

      byte[] msg    = Firmata.Util.EncodeDigitalMessage(port,values);

      Assert.AreEqual( 3, msg.Length );
      Assert.AreEqual( Firmata.Command.DIGITAL_MESSAGE | port, msg[0]);
      Assert.AreEqual( 0x7F, msg[1]);
      Assert.AreEqual( 0x01, msg[2]);
    }

    [Test()]
    public void TestDigitalMessageEncoding14Pins () {
      int port = 1;
      int[]  values = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

      byte[] msg    = Firmata.Util.EncodeDigitalMessage(port,values);

      Assert.AreEqual( 3, msg.Length );
      Assert.AreEqual( Firmata.Command.DIGITAL_MESSAGE | port, msg[0]);
      Assert.AreEqual( 0x7F, msg[1]);
      Assert.AreEqual( 0x7F, msg[2]);

      // And decode again

      int _port;
      int[] _values = new int[14];

      Firmata.Util.DecodeDigitalMessage(msg, out _port, out _values);

      Assert.AreEqual( port, _port);
      Assert.AreEqual( values, _values);
    }


    [Test()]
    public void TestAnalogMessageEncoding() {
      int pin = 1;
      int val = 255;
      byte[] msg = Firmata.Util.EncodeAnalogMessage(pin,val);

      Assert.AreEqual( Firmata.Command.ANALOG_MESSAGE | pin, msg[0]);
      Assert.AreEqual( Firmata.Util.LSB(val), msg[1]);
      Assert.AreEqual( Firmata.Util.MSB(val), msg[2]);

      // And decode again

      int _pin;
      int _value;

      Firmata.Util.DecodeAnalogMessage(msg, out _pin, out _value);

      Assert.AreEqual(pin, _pin);
      Assert.AreEqual(val, _value);

    }

  }

  [TestFixture()]
  public class CommandUtilTest {

    [Test()]
    public void TestGetCommandWithDigitalMessage() {
      int port = 1;
      int[]  values = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
      byte[] msg    = Firmata.Util.EncodeDigitalMessage(port,values);

      byte command = Firmata.Util.GetCommand(msg[0]);

      Assert.AreEqual(Firmata.Command.DIGITAL_MESSAGE, command);
    }

    [Test()]
    public void TestVerifyCommandWithDigitalMessage() {
      int port = 1;
      int[]  values = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
      byte[] msg    = Firmata.Util.EncodeDigitalMessage(port,values);

      bool isCommand = Firmata.Util.VerifiyCommand(msg[0],Firmata.Command.DIGITAL_MESSAGE);

      Assert.That(isCommand);
    }

    [Test()]
    public void TestContainsCommandWithDigitalMessage() {
      int port = 1;
      int[]  values = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
      byte[] msg    = Firmata.Util.EncodeDigitalMessage(port,values);

      bool hasCommand = Firmata.Util.ContainsCommand(msg,Firmata.Command.DIGITAL_MESSAGE);

      Assert.That(hasCommand);
    }


    [Test()]
    public void TestContainsSysexCommand() {
      byte[] msg = Firmata.Util.RequestFirmwareInformation();
      Assert.That (Firmata.Util.ContainsCommand(msg, Firmata.Command.SYSEX_START));
      Assert.That (Firmata.Util.ContainsCommand(msg, Firmata.Command.SYSEX_END));
    }
  }
}

