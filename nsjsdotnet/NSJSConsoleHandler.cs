namespace nsjsdotnet
{
    using nsjsdotnet.Core;
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    public abstract class NSJSConsoleHandler
    {
        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void system([MarshalAs(UnmanagedType.LPStr)]string command);

        private static readonly Action<bool, string, string> __assert;

        public virtual event EventHandler<NSJSMessage> Message = null;

        static NSJSConsoleHandler()
        {
            __assert = compliedassertbridging();
        }

        public static NSJSConsoleHandler DefaultHandler
        {
            get
            {
                return nsjsdotnet.Runtime.Console.DefaultHandler;
            }
        }

        public virtual void SetTitle(NSJSFunctionCallbackInfo arguments, string caption)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            Console.Title = caption ?? string.Empty;
        }

        public virtual string GetTitle(NSJSFunctionCallbackInfo arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            return Console.Title;
        }

        public virtual void Clear(NSJSFunctionCallbackInfo arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            Console.Clear();
        }

        protected internal virtual void OnMessage(NSJSMessage e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            EventHandler<NSJSMessage> handler = this.Message;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public virtual void SystemCall(NSJSFunctionCallbackInfo arguments, string cmd)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (!string.IsNullOrEmpty(cmd))
            {
                system(cmd);
            }
        }

        public virtual void PrintLine(NSJSFunctionCallbackInfo arguments)
        {
            wvsprintln(arguments, true);
        }

        public virtual void Log(NSJSFunctionCallbackInfo arguments)
        {
            wvsprintln(arguments, true);
        }

        private void wvsprintln(NSJSFunctionCallbackInfo arguments, bool newline)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (arguments.Length > 0)
            {
                string contents = null;
                if (newline)
                {
                    contents = FormatLine(arguments);
                }
                else
                {
                    contents = FormatString(arguments);
                }
                if (contents != null)
                {
                    OutputMesage(arguments, contents);
                }
            }
        }

        public virtual void PrintString(NSJSFunctionCallbackInfo arguments)
        {
            wvsprintln(arguments, false);
        }

        public virtual void Throw(NSJSFunctionCallbackInfo arguments, NSJSValue exception)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            NSJSException.Throw(exception);
        }

        public virtual void Assert(NSJSFunctionCallbackInfo arguments, bool condition, string message, string detailMessage)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            NSJSException.Throw(arguments.VirtualMachine, string.Format("assert failed\r\n{0}\r\n{1}", message, detailMessage));
            Assert(condition, message, detailMessage);
        }

        protected virtual void Assert(bool condition, string message, string detailMessage)
        {
            __assert(condition, message, detailMessage);
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

        public virtual string FormatLine(NSJSFunctionCallbackInfo arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            return sprint(arguments, true);
        }

        public virtual string FormatString(NSJSFunctionCallbackInfo arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            return sprint(arguments, false);
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
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            string contents = null;
            if (arguments.Length <= 0)
            {
                return contents;
            }
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

        protected virtual void OutputMesage(NSJSFunctionCallbackInfo arguments, string message)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (!string.IsNullOrEmpty(message))
            {
                Console.Write(message);
            }
        }
    }
}
