namespace nsjsdotnet.Core.Sorting
{
    using System;
    using System.Collections.Generic;

    public static class Sortable
    {
        private static class SelectSorting
        {
            public static void Sort<T>(IList<T> nums, int low, int high, Func<T, T, bool> max)
            {
                if (nums == null || max == null)
                {
                    throw new ArgumentNullException();
                }
                if (low < 0 || high >= nums.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                for (int i = low; i <= high; i++)
                {
                    for (int j = i; j <= high; j++)
                    {
                        if (max(nums[i], nums[j]))
                        {
                            T num = nums[i];
                            nums[i] = nums[j];
                            nums[j] = num;
                        }
                    }
                }
            }

            public static void Sort<T>(IList<T> nums, Func<T, T, bool> max)
            {
                SelectSorting.Sort(nums, 0, nums.Count - 1, max);
            }
        }

        private static class QuickSorting
        {
            private static int Middle<T>(IList<T> nums, int low, int high, Func<T, T, bool> max)
            {
                T key = nums[low];
                while (low < high)
                {
                    while (max(nums[high], key) && high > low)
                    {
                        high--;
                    }
                    nums[low] = nums[high];
                    while (!max(nums[low], key) && high > low)
                    {
                        low++;
                    }
                    nums[high] = nums[low];
                }
                nums[low] = key;
                return high;
            }

            public static void Sort<T>(IList<T> nums, int low, int high, Func<T, T, bool> max)
            {
                if (nums == null || max == null)
                {
                    throw new ArgumentNullException();
                }
                if (nums.Count <= 1 || low >= high)
                {
                    return;
                }
                int middle = Middle(nums, low, high, max);
                QuickSorting.Sort(nums, low, middle - 1, max);
                QuickSorting.Sort(nums, middle + 1, high, max);
            }

            public static void Sort<T>(IList<T> nums, Func<T, T, bool> max)
            {
                QuickSorting.Sort<T>(nums, 0, nums.Count - 1, max);
            }
        }

        public static void Select<T>(IList<T> nums, int low, int high, Func<T, T, bool> max)
        {
            SelectSorting.Sort(nums, low, high, max);
        }

        public static void Select<T>(IList<T> nums, Func<T, T, bool> max)
        {
            SelectSorting.Sort(nums, max);
        }

        public static void Quick<T>(IList<T> nums, int low, int high, Func<T, T, bool> max)
        {
            QuickSorting.Sort(nums, low, high, max);
        }

        public static void Quick<T>(IList<T> nums, Func<T, T, bool> max)
        {
            QuickSorting.Sort(nums, max);
        }
    }
}
