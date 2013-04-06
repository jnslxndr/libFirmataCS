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
    }

  }
}
