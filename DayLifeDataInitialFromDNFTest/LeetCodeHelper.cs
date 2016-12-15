using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Helpers
{

    public class LeetCodeHelper
    {
        /// <summary>
        /// Given input array nums = [1,1,2],
        /// Your function should return length = 2, with the first two elements
        /// of nums being 1 and 2 respectively.
        /// It doesn't matter what you leave beyond the new length.
        /// </summary>
        /// <param name="nums"></param>
        /// <returns></returns>
        public int RemoveDuplicates(int[] nums)
        {
            if (nums.Count() <= 1) return nums.Count();
            var lastVal = nums[0];
            var len = 1;
            for (var i = 1; i <= nums.Length-1; i++)
            {
                var curVal = nums[i];
                if (curVal != lastVal)
                {
                    lastVal = curVal;
                    len++;
                }
               
            }
             return nums.Count();
        }

        public int RemoveElement(int[] nums, int val)
        {
            int len = nums.Length;
            int index = 0;
            while (index < len)
            {
                if (nums[index] == val)
                {
                    nums[index] = nums[len - 1];
                    len--;
                }
                else
                {
                    index++;
                }
            }

            return len;
        }
    }
}
