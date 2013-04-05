using System;
using System.Collections.Generic;

namespace Firmata {

  #region Static utils
  public static class Util {

    public static bool ContainsCommand(byte[] msg, byte cmd) {
      foreach (byte b in msg) {
        if (VerifiyCommand(b,cmd)) return true;
      }
      return false;
    }

    public static bool VerifiyCommand(byte b, byte cmd) {
      return GetCommand(b) == cmd;
    }

    public static byte GetCommand (byte data) {
      // Commands in the 0xF* region do not have channel data
      return data > 0xF0 ? data :  (byte)(data & 0xF0); // else mask out the Commandbits
    }

    public static byte[] PortMessage(int port, int[] values) {
      byte LSB,MSB;
      ToBytes( (int) ValuesToPortState(values), out LSB, out MSB);
      byte[] command = {(byte)(Command.DIGITALMESSAGE | port), LSB, MSB };
      return command;
    }

    /// <summary>
    /// Decodes a digital port message.
    /// </summary>
    /// <returns>
    /// Returns true, if successfully decoded. Otherwise false.
    /// </returns>
    /// <param name='data'>
    /// The raw message.
    /// </param>
    /// <param name='port'>
    /// Gets set to the port as encoded in the message.
    /// </param>
    /// <param name='vals'>
    /// Gets set to an array of integer values, relative to their port state.
    /// </param>
    public static bool DecodePortMessage(byte[] data, out int port, out int[] vals) {
      if (data.Length<3){
        port   = 0;
        vals = new int[8];
        return false;
      }
      port = (data[0] & 0x0f);
      vals = ValuesFromPortState( FromBytes(data[1], data[2]) );
      return true;
    }

    /// <summary>
    /// Decodes the analog message.
    /// </summary>
    /// <returns>
    /// The analog message.
    /// </returns>
    /// <param name='data'>
    /// If set to <c>true</c> data.
    /// </param>
    /// <param name='pin'>
    /// If set to <c>true</c> pin.
    /// </param>
    /// <param name='val'>
    /// If set to <c>true</c> value.
    /// </param>
    public static bool DecodeAnalogMessage(byte[] data, out int pin, out int val) {
      if (data.Length<3){
        pin = 0;
        val = 0;
        return false;
      }
      pin = (data[0] & 0x0f);
      val = FromBytes(data[1], data[2]);
      return true;
    }

    /// <summary>
    /// Get the integer value that was sent using the 7-bit messages of the firmata protocol
    /// </summary>
    public static int FromBytes(byte LSB, byte MSB)
    {
      return ((MSB & 0x7F) << 7) | (LSB & 0x7F);
    }

    /// <summary>
    /// Split an integer value to two 7-bit parts so it can be sent using the firmata protocol
    /// </summary>
    public static void ToBytes(int val, out byte LSB, out byte MSB)
    {
      LSB = (byte)( val & 0x7F );
      MSB = (byte)((val >> 7) & 0x7F);
    }

    /// <summary>
    /// Send an array of boolean values indicating the state of each individual
    /// pin and get a byte representing a port
    /// </summary>
    public static byte ValuesToPortState(int[] pins)
    {
      byte state = 0;
      for (int i = 0; i < pins.Length; i++) {
        state |= (byte) ((pins[i] & 0x01) << i);
      }
      return state;
    }

    /// <summary>
    /// Convert a port state to an array of integers.
    /// </summary>
    /// <returns>
    /// Returns the decoded array.
    /// </returns>
    /// <param name='state'>
    /// The port state.
    /// </param>
    public static int[] ValuesFromPortState(int state)
    {
      int[] port = new int[Constants.BitsPerPort];
      for (int i = 0; i < Constants.BitsPerPort; i++) {
        port[i] = (state>>i) & 0x01;
      }
      return port;
    }

    public static string CommandBufferToString(Queue<byte> CommandBuffer, string Glue = "\r\n")
    {
      string s = string.Empty;
      while (CommandBuffer.Count > 0) {
        byte b = CommandBuffer.Dequeue();
        int currentSize = s.Length;
        // The Command Switch:
        switch(Util.GetCommand(b)) {
          case Command.SYSEX_START:
            s+="Sysex (";
            byte sb = CommandBuffer.Dequeue();
            while(sb != Command.SYSEX_END) {
              switch(sb){
                case Command.SAMPLING_INTERVAL:
                  s+="SamplingInterval: ";
                  byte _LSB = CommandBuffer.Dequeue();
                  byte _MSB = CommandBuffer.Dequeue();
                  s+=Util.FromBytes(_MSB,_LSB).ToString();
                  break;
                case Command.EXTENDED_ANALOG:
                  int pin  = CommandBuffer.Dequeue() & 0x7f;
                  byte __LSB = CommandBuffer.Dequeue();
                  byte __MSB = CommandBuffer.Dequeue();
                  s+="Extended Analog Message for pin ";
                  s+=pin.ToString() + ": ";
                  s+=Util.GetValueFromBytes(__MSB,__LSB).ToString();
                  break;
                case Command.REPORT_FIRMWARE:
                  s+="ReportFirmwareVersion";
                  break;
                default:
                  s+= String.Format("{0:X}", sb);
                break;
              }
              sb = CommandBuffer.Dequeue();
            }
            s+=")";
            break;

          case Command.RESET:
            s+="Reset!";
            break;

          case Command.SETPINMODE:
            s+="Set PinMode of pin ";
            s+=CommandBuffer.Dequeue().ToString();
            s+=" to ";
            switch((PinMode)CommandBuffer.Dequeue()) {
              case PinMode.INPUT:   s+="INPUT";  break;
              case PinMode.OUTPUT:  s+="OUTPUT"; break;
              case PinMode.ANALOG:  s+="ANALOG"; break;
              case PinMode.PWM:     s+="PWM";    break;
              case PinMode.SERVO:   s+="SERVO";  break;
              case PinMode.SHIFT:   s+="SHIFT";  break;
              case PinMode.I2C:     s+="I2C";    break;
            }
            break;

          case Command.TOGGLEDIGITALREPORT:
            s+="DIGITAL Pin Reporting for port ";
            s+=(b&0x0f).ToString();
            s+=" set to: ";
            s+=CommandBuffer.Dequeue().ToString();
            break;

          case Command.TOGGLEANALOGREPORT:
            s+="ANALOG Pin Reporting for pin ";
            s+=(b&0x0f).ToString();
            s+=" set to: ";
            s+=CommandBuffer.Dequeue().ToString();
            break;

          case Command.DIGITALMESSAGE:
            s+="Digital message for port ";
            s+=(b&0x0f).ToString();
            s+=": ";
            s+=Convert.ToString(CommandBuffer.Dequeue(), 2);
            break;

          case Command.ANALOGMESSAGE:
            s+="Analog message for pin ";
            s+=(b&0x0f).ToString();
            s+=": ";
            byte LSB = CommandBuffer.Dequeue();
            byte MSB = CommandBuffer.Dequeue();
            s+=Util.GetValueFromBytes(MSB,LSB).ToString();
            break;
        }
        if (s.Length != currentSize) s+=Glue;
      }
      return s;
    }
  }
  #endregion
}