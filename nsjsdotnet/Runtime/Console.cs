namespace nsjsdotnet.Runtime
{
    using global::System;
    using global::System.Diagnostics;
    using global::System.Linq.Expressions;
    using global::System.Runtime.InteropServices;
    using nsjsdotnet.Core;
    using CONSOLE = global::System.Console;

    static class Console
    {
        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void system([MarshalAs(UnmanagedType.LPStr)]string command);

        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        private static readonly Action<bool, string, string> _assert;

        static Console()
        {
            _assert = compliedassertbridging();
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("title", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(title));
            owner.AddFunction("error", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(error));
            owner.AddFunction("assert", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(assert));
            owner.AddFunction("log", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(log));
            owner.AddFunction("printf", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(printf));
            owner.AddFunction("println", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(println));
            owner.AddFunction("sprintf", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(sprintf));
            owner.AddFunction("sprintln", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(sprintln));
            owner.AddFunction("system", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(system));
            owner.AddFunction("clear", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(clear));
            owner.AddFunction("message", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(message));
        }

        private static void title(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length <= 0)
            {
                arguments.SetReturnValue(CONSOLE.Title);
            }
            else
            {
                CONSOLE.Title = (arguments[0] as NSJSString)?.Value ?? string.Empty;
            }
        }

        private static Action<bool, string, string> compliedassertbridging()
        {
            ParameterExpression condition = Expression.Parameter(typeof(bool));
            ParameterExpression message = Expression.Parameter(typeof(string));
            ParameterExpression detailMessage = Expression.Parameter(typeof(string));
            return Expression.Lambda<Action<bool, string, string>>(Expression.
                Call(typeof(Debug).GetMethod("Assert", new[]
                {
                    typeof(bool),
                    typeof(string),
                    typeof(string),
                }), new[] { condition, message, detailMessage }),
            new[] { condition, message, detailMessage }).Compile();
        }

        private static void assert(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (arguments.Length > 0)
            {
                bool condition = ValueAuxiliary.ToBoolean(arguments[0]);
                string message = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null) ?? string.Empty;
                string detailMessage = ValueAuxiliary.ToString(arguments.Length > 1 ? arguments[1] : null) ?? string.Empty;
                NSJSException.Throw(arguments.VirtualMachine, string.Format("assert failed\r\n{0}\r\n{1}", message, detailMessage));
                _assert(condition, message, detailMessage);
            }
        }

        private static void error(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSValue exception = arguments.Length > 0 ? arguments[0] : null;
            if (exception != null)
            {
                NSJSException.Throw(exception);
            }
        }

        private static bool domessage(NSJSFunctionCallbackInfo arguments, NSJSMessage.OriginBehavior behavior, string message)
        {
            if (arguments == null)
            {
                return false;
            }
            NSJSMessage m = new NSJSMessage(arguments, behavior, message);
            return m.Post().Cancel;
        }

        private static void outputmesage(NSJSFunctionCallbackInfo arguments, NSJSMessage.OriginBehavior behavior, string message)
        {
            do
            {
                if (domessage(arguments, behavior, message))
                {
                    break;
                }
                if (!string.IsNullOrEmpty(message))
                {
                    CONSOLE.Write(message);
                }
            } while (false);
        }

        private static void clear(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            if (!domessage(arguments, NSJSMessage.OriginBehavior.kClear, null))
            {
                CONSOLE.Clear();
            }
        }

        private static void message(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            outputmesage(arguments, NSJSMessage.OriginBehavior.kMessage, arguments.Length > 0 ? ValueAuxiliary.ToString(arguments[0]) : null);
        }

        private static bool sprint_get_format_features(string format, int startIndex, int endIndex, ref char flags, ref char padding, ref int width)
        {
            padding = '0'; // 默认的占位符
            flags = '0';
            width = 0;
            if (format == null || startIndex >= format.Length || startIndex > endIndex)
            {
                return false;
            }
            string seg = format.Substring(startIndex, (endIndex - startIndex));
            if (seg.Length > 0)
            {
                flags = seg[0];
            }
            int ofs = 0;
            if (flags == '+' || flags == '-' || flags == '#')
            {
                ofs++;
            }
            else
            {
                flags = '+';
            }
            if (seg.Length > ofs)
            {
                if (ofs == (seg.Length - 2))
                {
                    padding = seg[ofs++];
                }
                if (seg.Length > ofs)
                {
                    int.TryParse(seg.Substring(ofs), out width);
                }
            }
            return true;
        }

        private static string sprint(NSJSFunctionCallbackInfo arguments, bool newline)
        {
            string contents = null;
            try
            {
                string format = arguments.Length > 0 ? ValueAuxiliary.ToString(arguments[0]) : null;
                if (format == null)
                {
                    format = newline ? Environment.NewLine : string.Empty;
                }
                else if (newline)
                {
                    format += Environment.NewLine;
                }
                int index = -1;
                int previous = 0;
                int solt = 1;
                do
                {
                    index = format.IndexOf('%', index + 1);
                    if (index > -1)
                    {
                        int ofs = index + 1;
                        for (int i = ofs; i < format.Length; i++)
                        {
                            char character = format[i];
                            char flags = '\0';
                            char pending = '\0';
                            int width = 0;
                            if (character == 'c')
                            {
                                if (solt >= arguments.Length)
                                {
                                    break;
                                }
                                int count = (index - previous);
                                count = count < 0 ? 0 : count;
                                string g = format.Substring(previous, count);
                                if (!string.IsNullOrEmpty(g))
                                {
                                    contents += g;
                                }
                                previous = i + 1;
                                contents += unchecked((char)ValueAuxiliary.ToInt64(arguments[solt++]));
                                index = i;
                                break;
                            }
                            else if (character == '%')
                            {
                                int count = (i - previous);
                                count = count < 0 ? 0 : count;
                                string g = format.Substring(previous, count);
                                if (!string.IsNullOrEmpty(g))
                                {
                                    contents += g;
                                }
                                previous = i + 1;
                                index = i;
                                break;
                            }
                            else if (character == 's')
                            {
                                if (solt >= arguments.Length)
                                {
                                    break;
                                }
                                int count = (index - previous);
                                count = count < 0 ? 0 : count;
                                string g = format.Substring(previous, count);
                                if (!string.IsNullOrEmpty(g))
                                {
                                    contents += g;
                                }
                                previous = i + 1;
                                contents += ValueAuxiliary.ToString(arguments[solt++]);
                                index = i;
                                break;
                            }
                            else if (character == 'i' || character == 'd' || character == 'u')
                            {
                                if (sprint_get_format_features(format, ofs, i, ref flags, ref pending, ref width))
                                {
                                    if (solt >= arguments.Length)
                                    {
                                        break;
                                    }
                                    int count = (index - previous);
                                    count = count < 0 ? 0 : count;
                                    string g = format.Substring(previous, count);
                                    if (!string.IsNullOrEmpty(g))
                                    {
                                        contents += g;
                                    }
                                    previous = i + 1;
                                    string s = NSJSString.Cast(arguments[solt++]).Value;
                                    if (flags == '+')
                                    {
                                        s = s.PadLeft(width, pending);
                                    }
                                    else
                                    {
                                        s = s.PadRight(width, pending);
                                    }
                                    contents += s;
                                    index = i;
                                }
                                break;
                            }
                            else if (character == 'b')
                            {
                                if (solt >= arguments.Length)
                                {
                                    break;
                                }
                                int count = (index - previous);
                                count = count < 0 ? 0 : count;
                                string g = format.Substring(previous, count);
                                if (!string.IsNullOrEmpty(g))
                                {
                                    contents += g;
                                }
                                previous = i + 1;
                                contents += "0b" + Convert.ToString(ValueAuxiliary.ToInt64(arguments[solt++]), 2);
                                index = i;
                                break;
                            }
                            else if (character == 'o' || character == 'x' || character == 'X')
                            {
                                if (sprint_get_format_features(format, ofs, i, ref flags, ref pending, ref width))
                                {
                                    if (solt >= arguments.Length)
                                    {
                                        break;
                                    }
                                    int count = (index - previous);
                                    count = count < 0 ? 0 : count;
                                    string g = format.Substring(previous, count);
                                    if (!string.IsNullOrEmpty(g))
                                    {
                                        contents += g;
                                    }
                                    previous = i + 1;
                                    int radix = character == 'o' ? 0x08 : 0x10;
                                    string s = Convert.ToString(ValueAuxiliary.ToInt64(arguments[solt++]), radix);
                                    if (flags == '+')
                                    {
                                        s = s.PadLeft(width, pending);
                                    }
                                    else
                                    {
                                        s = s.PadRight(width, pending);
                                    }
                                    if (radix == 0x10)
                                    {
                                        s = "0x" + (character == 'X' ? s.ToUpper() : s);
                                    }
                                    contents += s;
                                    index = i;
                                }
                                break;
                            }
                            else if (character == 'p' || character == 'P')
                            {
                                if (sprint_get_format_features(format, ofs, i, ref flags, ref pending, ref width))
                                {
                                    if (solt >= arguments.Length)
                                    {
                                        break;
                                    }
                                    int count = (index - previous);
                                    count = count < 0 ? 0 : count;
                                    string g = format.Substring(previous, count);
                                    if (!string.IsNullOrEmpty(g))
                                    {
                                        contents += g;
                                    }
                                    previous = i + 1;
                                    long ptr = Environment.Is64BitProcess ? ValueAuxiliary.ToInt64(arguments[solt++]) : ValueAuxiliary.ToInt32(arguments[solt++]);
                                    string s = ptr.ToString(character == 'p' ? "x2" : "X2");
                                    if (flags == '+')
                                    {
                                        s = s.PadLeft(width, pending);
                                    }
                                    else
                                    {
                                        s = s.PadRight(width, pending);
                                    }
                                    s = "0x" + s;
                                    contents += s;
                                    index = i;
                                }
                                break;
                            }
                            else if (character == 'f' || character == 'l' ||
                                character == 'e' || character == 'g' ||
                                character == 'E' || character == 'G') // lf
                            {
                                int mode = 0; // 0: 单精度浮点数，1：双精度浮点数，2：科学型浮点数数，3：常规型浮点数
                                int n = i + 1;
                                if (n < format.Length && format[n] == 'f') // lf
                                {
                                    mode = 1;
                                    i++;
                                }
                                else if (character == 'e' || character == 'E')
                                {
                                    mode = 2;
                                }
                                else if (character == 'g' || character == 'G')
                                {
                                    mode = 3;
                                }
                                if (sprint_get_format_features(format, ofs, i, ref flags, ref pending, ref width))
                                {
                                    if (solt >= arguments.Length)
                                    {
                                        break;
                                    }
                                    int count = (index - previous);
                                    count = count < 0 ? 0 : count;
                                    string g = format.Substring(previous, count);
                                    if (!string.IsNullOrEmpty(g))
                                    {
                                        contents += g;
                                    }
                                    previous = i + 1;
                                    double num = ValueAuxiliary.ToDouble(arguments[solt++]);
                                    string s = mode == 2 || mode == 3 ? num.ToString(character.ToString()) : num.ToString();
                                    if (flags == '+')
                                    {
                                        s = s.PadLeft(width, pending);
                                    }
                                    else
                                    {
                                        s = s.PadRight(width, pending);
                                    }
                                    contents += s;
                                    index = i;
                                }
                                break;
                            }
                        }
                    }
                } while (index >= 0);
                if (format.Length > previous)
                {
                    string s = format.Substring(previous);
                    if (!string.IsNullOrEmpty(s))
                    {
                        contents += s;
                    }
                }
            }
            catch (Exception)
            {
                contents = null;
            }
            return contents;
        }

        private static void print(NSJSFunctionCallbackInfo arguments, NSJSMessage.OriginBehavior behavior)
        {
            string contents = sprint(arguments, behavior == NSJSMessage.OriginBehavior.kPrintln ? true : false);
            outputmesage(arguments, behavior, contents);
        }

        private static void sprintf(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string contents = sprint(arguments, false);
            if (contents != null)
            {
                arguments.SetReturnValue(contents);
            }
            else
            {
                arguments.SetReturnValue(NSJSValue.Null(arguments.VirtualMachine));
            }
        }

        private static void sprintln(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string contents = sprint(arguments, true);
            if (contents != null)
            {
                arguments.SetReturnValue(contents);
            }
            else
            {
                arguments.SetReturnValue(NSJSValue.Null(arguments.VirtualMachine));
            }
        }

        private static void log(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            string contents = sprint(arguments, true);
            outputmesage(arguments, NSJSMessage.OriginBehavior.kLog, contents);
        }

        private static void println(IntPtr info)
        {
            print(NSJSFunctionCallbackInfo.From(info), NSJSMessage.OriginBehavior.kPrintln);
        }

        private static void printf(IntPtr info)
        {
            print(NSJSFunctionCallbackInfo.From(info), NSJSMessage.OriginBehavior.kPrintf);
        }

        private static void system(IntPtr info)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            NSJSString cmd = arguments.Length > 0 ? arguments[0] as NSJSString : null;
            string message = cmd.Value;
            bool success = false;
            if (!domessage(arguments, NSJSMessage.OriginBehavior.kSystem, message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    system(message);
                }
            }
            arguments.SetReturnValue(success);
        }
    }
}
