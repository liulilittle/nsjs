namespace nsjsdotnet.Runtime.Systematic.Sorting
{
    using System;

    static class Sortable
    {
        public static NSJSVirtualMachine.ExtensionObjectTemplate ClassTemplate
        {
            get;
            private set;
        }

        static Sortable()
        {
            NSJSVirtualMachine.ExtensionObjectTemplate owner = new NSJSVirtualMachine.ExtensionObjectTemplate();
            ClassTemplate = owner;
            owner.AddFunction("Quick", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Quick));
            owner.AddFunction("Select", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Select));
            owner.AddFunction("Bubble", NSJSPinnedCollection.Pinned<NSJSFunctionCallback>(Bubble));
        }

        private static void Sort(IntPtr info, Action<NSJSArray, int, int, Func<NSJSValue, NSJSValue, bool>> sorting)
        {
            NSJSFunctionCallbackInfo arguments = NSJSFunctionCallbackInfo.From(info);
            bool success = false;
            if (arguments.Length > 1)
            {
                NSJSArray s = arguments[0] as NSJSArray;
                NSJSFunction max = arguments[1] as NSJSFunction;
                int low = 0;
                int high = -1;
                if (arguments.Length > 3 && max == null)
                {
                    max = arguments[3] as NSJSFunction;
                    NSJSInt32 i32 = arguments[1] as NSJSInt32;
                    if (i32 != null)
                    {
                        low = i32.Value;
                    }
                    i32 = arguments[2] as NSJSInt32;
                    if (i32 != null)
                    {
                        high = i32.Value;
                    }
                }
                if (max != null && s != null)
                {
                    if (high < 0)
                    {
                        high = s.Length - 1;
                    }
                    sorting(s, low, high, (x, y) => ((max.Call(x, y) as NSJSBoolean)?.Value).GetValueOrDefault());
                }
            }
            arguments.SetReturnValue(success);
        }

        private static void Select(IntPtr info)
        {
            Sortable.Sort(info, (s, low, high, max) =>
            {
                for (int i = low; i <= high; i++)
                {
                    for (int j = i; j <= high; j++)
                    {
                        if (max(s[i], s[j]))
                        {
                            NSJSValue n = s[i];
                            s[i] = s[j];
                            s[j] = n;
                        }
                    }
                }
            });
        }

        private static void Bubble(IntPtr info)
        {
            Sortable.Sort(info, (s, low, high, max) =>
            {
                for (int i = low; i <= high; i++)
                {
                    for (int j = low; j <= high; j++)
                    {
                        if (max(s[j], s[i]))
                        {
                            NSJSValue n = s[i];
                            s[i] = s[j];
                            s[j] = n;
                        }
                    }
                }
            });
        }

        private class QuickSorting
        {
            private static int Middle(NSJSArray s, int low, int high, Func<NSJSValue, NSJSValue, bool> max)
            {
                NSJSValue key = s[low];
                while (low < high)
                {
                    while (max(s[high], key) && high > low) // R轴
                    {
                        high--;
                    }
                    s[low] = s[high];
                    while (!max(s[low], key) && high > low) // L轴
                    {
                        low--;
                    }
                    s[high] = s[low];
                }
                s[low] = key;
                return high;
            }

            public static void Sort(NSJSArray s, int low, int high, Func<NSJSValue, NSJSValue, bool> max)
            {
                if (!(s.Length <= 1 || low >= high))
                {
                    int middle = QuickSorting.Middle(s, low, high, max);
                    QuickSorting.Sort(s, low, middle - 1, max); // 向左向中心扩散
                    QuickSorting.Sort(s, middle + 1, high, max); // 从中心向右扩散
                }
            }
        }

        private static void Quick(IntPtr info)
        {
            Sortable.Sort(info, (s, low, high, max) => QuickSorting.Sort(s, low, high, max));
        }
    }
}
