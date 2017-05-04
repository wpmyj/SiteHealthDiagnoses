using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;

namespace DayLifeDataInitialFromDNFTest
{
    public class MyStringComparer<T> : IComparer<T>
    {
        private CompareInfo myComp = CultureInfo.InvariantCulture.CompareInfo;
        private CompareOptions myOptions = CompareOptions.Ordinal;
        public MyStringComparer()
        {

        }

        public int Compare(T xT, T yT)
        {

            if (xT == null) return -1;
            if (yT == null) return 1;
            var x = xT.ToString();
            var y = yT.ToString();
            if (x == y) return 0;
            String sa = x as String;
            String sb = y as String;

            if (sa != null && sb != null)
                return myComp.Compare(sa, sb, myOptions);
            throw new ArgumentException("x and y should be strings.");
        }
    }
}
