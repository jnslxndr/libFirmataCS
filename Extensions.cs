using System;
using System.IO;
using System.Collections.Generic;

namespace Firmata {
  namespace Extensions {

    public static class StreamExtensions {
      public static IEnumerable<byte> Bytes( this Stream stream ) {
        if (stream != null) {
          stream.Seek( 0, SeekOrigin.Begin );
          for (int i = stream.ReadByte(); i != -1; i = stream.ReadByte())
            yield return (byte) i;
        }
      }

      public static IEnumerable<int> Decode7BitBytes( this Stream stream, bool lsbfirst=true) {
        if (stream != null) {
          stream.Seek( 0, SeekOrigin.Begin );
          int[] data = new int[2];
          for (int i = stream.ReadByte(); i != -1; i = stream.ReadByte()) {
            if (stream.Position%2==0) {
              data[1] = i;
              yield return data.Decode14Bit(lsbfirst);
            } else {
              data[0] = i;
            }
          }
        }
      }

      public static byte[] ToArray( this Stream stream ) {
        if (stream != null) {
          byte[] bytes = new byte[stream.Length];
          stream.Seek( 0, SeekOrigin.Begin );
          stream.Read(bytes,0,(int)stream.Length);
          stream.Seek(0,SeekOrigin.End);
          return bytes;
        } else {
          return new byte[]{};
        }
      }
    }

    public static class ByteExtensions {
      public static byte LSB( this int i ) {
        return (byte) (i &0x7f);
      }

      public static byte MSB( this int i ) {
        return (byte) ((i >> 7) & 0x7f);
      }

      public static int DecodeWith7BitMSB(this int lsb, int msb) {
        if (lsb < 0 || msb < 0) return -1; // Return negative value on failure
        return (lsb & 0x7f) & ((msb & 0x7f) << 7);
      }

      public static int DecodeWith7BitLSB(this int msb, int lsb) {
        if (lsb < 0 || msb < 0) return -1; // Return negative value on failure
        return (lsb & 0x7f) & ((msb & 0x7f) << 7);
      }

      public static int Decode14Bit(this int[] source, bool lsbfirst=true) {
        return lsbfirst ? source[0].DecodeWith7BitMSB(source[1]) : source[0].DecodeWith7BitLSB(source[1]);
      }

      public static int Decode14Bit(this Queue<byte> q, bool lsbfirst=true) {
        return (new int[] {
          q.Dequeue(),
          q.Dequeue()
        }).Decode14Bit();
      }

      public static int Decode14Bit(this Stream stream, bool lsbfirst=true) {
        return (new int[] { 
          stream.ReadByte(),
          stream.ReadByte()
        }).Decode14Bit(lsbfirst);
      }
    }

    public static class FirmataExtensions {
      public static string ToFirmataCommandString(this byte b) {
        return Command.ToString(b);
      }
    }

    public class ByteWriter : BinaryWriter {
      public ByteWriter() : this(Stream.Null) {}
      public ByteWriter (Stream s) : base(s) {}
      public static ByteWriter operator +(ByteWriter writer, byte b) {
        writer.Write(b);
        return writer;
      }
      public static ByteWriter operator +(ByteWriter writer, byte[] bytes) {
        foreach (byte b in bytes) 
          writer.Write(b);
        return writer;
      }
    }

    public static class ArrayExtensions {
      public static byte[] From7BitAsBytes(this byte[] arr) {
        if (arr.Length % 2 == 0) {
          byte[] _out = new byte[arr.Length / 2];
          for (int i=0; i<arr.Length; i+=2) {
            _out[i / 2] = (byte) Util.FromBytes( arr[i], arr[i + 1] );
          }
          return _out;
        } else {
          return new byte[]{};
        }
      }
      public static int[] From7Bit( this byte[] arr ) {
        if (arr.Length % 2 == 0) {
          int[] _out = new int[arr.Length / 2];
          for (int i=0; i<arr.Length; i+=2) {
            _out[i / 2] = Util.FromBytes( arr[i], arr[i + 1] );
          }
          return _out;
        } else {
          return new int[]{};
        }
      }

      public static byte[] To7Bit( this int[] arr ) {
        byte[] _out = new byte[arr.Length * 2];
        for (int i=0; i<_out.Length; i+=2) {
          _out[i] = arr[i / 2].LSB( ); // LSB first
          _out[i + 1] = arr[i / 2].MSB( ); // MSB second
        }
        return _out;
      }

      public static string ToString(this int[] arr, string format, string delimiter=",") {
        string _out = "";
        for (int i = 0; i < arr.Length-1; i++) {
          _out += arr[i].ToString(format) + delimiter;
        }
        _out += arr[arr.Length-1].ToString(format);
        return _out;
      }

      public static string ToString(this byte[] arr, string format, string delimiter=",") {
        string _out = "";
        for (int i = 0; i < arr.Length-1; i++) {
          _out += arr[i].ToString(format) + delimiter;
        }
        _out += arr[arr.Length-1].ToString(format);
        return _out;
      }
    }
  }
}
