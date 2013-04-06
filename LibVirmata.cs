#region Copyright notice
/*
A Firmata Helper Library v1.0.x
-------------------------------
Encoding control and configuration messages for Firmata enabled MCUs.

For more information on Firmata see: http://firmata.org
Get the source from: https://github.com/jens-a-e/VVVVirmata
Any issues & feature requests should be posted to: https://github.com/jens-a-e/VVVVirmata/issues

Copyleft 2011-2013
Jens Alexander Ewald, http://lea.io
Both: http://www.muthesius-kunsthochschule.de

Inspired by the Sharpduino project by Tasos Valsamidis (LSB and MSB operations)
See http://code.google.com/p/sharpduino if interested.


Copyright notice
----------------

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
*/
#endregion

#region usings

using System;
using System.Collections.Generic;

#endregion usings

namespace Firmata
{
  #region Definitions
  /// For the Specs see: http://firmata.org/wiki/Protocol

  public struct Command
  {
    /// <summary>
    /// The distinctive value that states that this message is a digital message.
    /// It comes as a report or as a command
    /// </summary>
    public const byte DIGITAL_MESSAGE          = 0x90;

    /// <summary>
    /// The command that toggles the continuous sending of the
    /// analog reading of the specified pin
    /// </summary>
    public const byte REPORT_ANALOG           = 0xC0;

    /// <summary>
    /// The command that toggles the continuous sending of the
    /// digital state of the specified port
    /// </summary>
    public const byte REPORT_DIGITAL          = 0xD0;

    /// <summary>
    /// The distinctive value that states that this message is an analog message.
    /// It comes as a report for analog in pins, or as a command for PWM
    /// </summary>
    public const byte ANALOG_MESSAGE           = 0xE0;

    /// <summary>
    /// A command to change the pin mode for the specified pin
    /// </summary>
    public const byte SET_PIN_MODE              = 0xF4;

    /// <summary>
    /// Report Protocol Version
    /// </summary>
    public const byte REPORT_VERSION          = 0xF9;

     /// <summary>
    /// Reset System Command
    /// </summary>
    public const byte SYSTEM_RESET            = 0xFF;


    /// ----- SYSEX Commands: ----- ///

    /// <summary>
    /// Sysex start command
    /// </summary>
    public const byte SYSEX_START             = 0xF0;

    /// <summary>
    /// Sysex end command
    /// </summary>
    public const byte SYSEX_END               = 0xF7;

    
    /// Subcommands
    public const byte RESERVED_COMMAND        = 0x00; // 2nd SysEx data byte is a chip-specific command (AVR, PIC, TI, etc).

    public const byte ANALOG_MAPPING_QUERY    = 0x69; // ask for mapping of analog to pin numbers

    public const byte ANALOG_MAPPING_RESPONSE = 0x6A; // reply with mapping info

    public const byte CAPABILITY_QUERY        = 0x6B; // ask for supported modes and resolution of all pins

    public const byte CAPABILITY_RESPONSE     = 0x6C; // reply with supported modes and resolution

    public const byte PIN_STATE_QUERY         = 0x6D; // ask for a pin's current mode and value

    public const byte PIN_STATE_RESPONSE      = 0x6E; // reply with a pin's current mode and value

    public const byte EXTENDED_ANALOG         = 0x6F; // analog write (PWM, Servo, etc) to any pin

    public const byte SERVO_CONFIG            = 0x70; // set max angle, minPulse, maxPulse, freq

    public const byte STRING_DATA             = 0x71; // a string message with 14-bits per char

    public const byte SHIFT_DATA              = 0x75; // shiftOut config/data message (34 bits)

    public const byte I2C_REQUEST             = 0x76; // I2C request messages from a host to an I/O board

    public const byte I2C_REPLY               = 0x77; // I2C reply messages from an I/O board to a host

    public const byte I2C_CONFIG              = 0x78; // Configure special I2C settings such as power pins and delay times

    public const byte REPORT_FIRMWARE         = 0x79; // report name and version of the firmware

    public const byte SAMPLING_INTERVAL       = 0x7A; // sampling interval

    public const byte SYSEX_NON_REALTIME      = 0x7E; // MIDI Reserved for non-realtime messages

    public const byte SYSEX_REALTIME          = 0x7F; // MIDI Reserved for realtime messages
    
  }

  public enum PinMode
  {
    /// <summary>
    /// Pinmode INPUT
    /// </summary>
    INPUT = 0x00,

    /// <summary>
    /// Pinmode OUTPUT
    /// </summary>
    OUTPUT = 0x01,

    /// <summary>
    /// Pinmode ANALOG
    /// </summary>
    ANALOG = 0x02,

    /// <summary>
    /// Pinmode PWM
    /// </summary>
    PWM = 0x03,

    /// <summary>
    /// Pinmode SERVO
    /// </summary>

    SERVO = 0x04,

    /// <summary>
    /// Pinmode for Shift Registers
    /// </summary>
    SHIFT = 0x05,

    /// <summary>
    /// Pinmode I2C
    /// </summary>
    I2C = 0x06,
  }

  #endregion
}