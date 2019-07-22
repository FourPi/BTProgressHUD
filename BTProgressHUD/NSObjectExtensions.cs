using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Foundation;
using UIKit;

namespace BigTed
{
    /// <summary>
    /// Contains several extensions for NSObject and derived classes.
    /// </summary>
    public static class NSObjectExtensions
    {

        [DllImport("/usr/lib/libobjc.dylib")]
        private static extern IntPtr object_getClassName(IntPtr obj);

        /// <summary>
        /// Returns the Objective-C class name of the NSObject instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetClassName(this NSObject obj)
        {
            return Marshal.PtrToStringAuto(object_getClassName(obj.Handle));
        }

    }
}