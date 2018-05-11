namespace nsjsdotnet.Runtime.Systematic
{
    using global::System;
    using BITCONVERTER = global::System.BitConverter;

    static class BitConverter
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static BitConverter()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;

            owner.AddFunction("ToInt32", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToInt32));
            owner.AddFunction("ToUInt32", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToUInt32));
            owner.AddFunction("ToInt16", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToInt16));
            owner.AddFunction("ToUInt16", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToUInt16));
            owner.AddFunction("ToSByte", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToSByte));
            owner.AddFunction("ToByte", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToByte));

            owner.AddFunction("ToSingle", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToSingle));
            owner.AddFunction("ToDouble", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToDouble));
            owner.AddFunction("ToBoolean", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToBoolean));
            owner.AddFunction("ToDateTime", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(ToDateTime));

            owner.AddFunction("IsLittleEndian", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(IsLittleEndian));
            owner.AddFunction("GetBytes", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(GetBytes));
        }

        private static void IsLittleEndian(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            arguments.SetReturnValue(BITCONVERTER.IsLittleEndian);
        }

        private static void GetBytes(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            byte[] result = null;
            if (arguments.Length > 0)
            {
                NSJSValue value = arguments[0];
                if (value != null)
                {
                    NSJSInt32 int32 = value as NSJSInt32;
                    if (int32 != null)
                    {
                        result = BITCONVERTER.GetBytes(int32.Value);
                    }
                    NSJSUInt32 uint32 = value as NSJSUInt32;
                    if (uint32 != null)
                    {
                        result = BITCONVERTER.GetBytes(uint32.Value);
                    }
                    NSJSBoolean boolean = value as NSJSBoolean;
                    if (boolean != null)
                    {
                        result = BITCONVERTER.GetBytes(boolean.Value);
                    }
                    NSJSDouble float64 = value as NSJSDouble;
                    if (float64 != null)
                    {
                        result = BITCONVERTER.GetBytes(float64.Value);
                    }
                    NSJSDateTime datetime = value as NSJSDateTime;
                    if (datetime != null)
                    {
                        result = BITCONVERTER.GetBytes(NSJSDateTime.DateTimeToLocalDate(datetime.Value));
                    };
                }
            }
            if (result != null)
            {
                arguments.SetReturnValue(result);
            }
            else
            {
                arguments.SetReturnValue(NSJSValue.Undefined(arguments.VirtualMachine));
            }
        }

        private static void ToUInt32(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            uint result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToUInt32(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToInt32(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            int result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToInt32(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToSingle(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            float result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToSingle(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToDouble(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            double result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToDouble(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToBoolean(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool result = false;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToBoolean(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToDateTime(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue result = null;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        long ticks = BITCONVERTER.ToInt64(buffer, offset);
                        result = NSJSDateTime.New(arguments.VirtualMachine, ticks);
                    }
                }
            }
            if (result == null)
            {
                result = NSJSValue.Undefined(arguments.VirtualMachine);
            }
            arguments.SetReturnValue(result);
        }

        private static void ToUInt16(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            uint result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToUInt16(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToInt16(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            int result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = BITCONVERTER.ToInt16(buffer, offset);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToByte(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            uint result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = buffer[offset];
                    }
                }
            }
            arguments.SetReturnValue(result);
        }

        private static void ToSByte(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            int result = 0;
            if (arguments.Length > 0)
            {
                NSJSUInt8Array value = arguments[0] as NSJSUInt8Array;
                NSJSInt32 startIndex = null;
                if (arguments.Length > 1)
                {
                    startIndex = arguments[1] as NSJSInt32;
                }
                if (value != null)
                {
                    int offset = 0;
                    if (startIndex != null)
                    {
                        offset = startIndex.Value;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    byte[] buffer = value.Buffer;
                    if (buffer != null)
                    {
                        result = (sbyte)(buffer[offset]);
                    }
                }
            }
            arguments.SetReturnValue(result);
        }
    }
}
