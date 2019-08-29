﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Linq;
using System.Windows.Forms;

namespace Yuzu.UI
{
    internal static class ControlExtensions
    {
        public static IObservable<MouseEventArgs> MouseDownAsObservable(this Control control)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (o, e) => h(e),
                h => control.MouseDown += h,
                h => control.MouseDown -= h);
        }

        public static IObservable<MouseEventArgs> MouseMoveAsObservable(this Control control)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (o, e) => h(e),
                h => control.MouseMove += h,
                h => control.MouseMove -= h);
        }

        public static IObservable<MouseEventArgs> MouseUpAsObservable(this Control control)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (o, e) => h(e),
                h => control.MouseUp += h,
                h => control.MouseUp -= h);
        }
    }
}
