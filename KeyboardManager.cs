using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZVideoConverter_WPF
{
    internal class KeyboardManager
    {
        Dictionary<System.Windows.Input.Key, List<Action<System.Windows.Input.Key, KeyState>>> BoundActions = new Dictionary<System.Windows.Input.Key, List<Action<System.Windows.Input.Key, KeyState>>>();
        public enum KeyState
        {
            Up,Down,
        }
        public KeyboardManager(MainWindow window)
        {

            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            //for uwp
            //Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            //Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key;
            if (BoundActions.ContainsKey(key))
            {
                foreach (var i in BoundActions[key])
                {
                    i(key, KeyState.Up);
                }
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key;
            if (BoundActions.ContainsKey(key))
            {
                foreach (var i in BoundActions[key])
                {
                    i(key, KeyState.Down);
                }
            }
        }

        //private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        //{
        //    var key = args.VirtualKey;
        //    if (BoundActions.ContainsKey(key))
        //    {
        //        foreach(var i in BoundActions[key])
        //        {
        //            i(key, KeyState.Up);
        //        }
        //    }
        //}
        //
        //private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        //{
        //    var key = args.VirtualKey;
        //    if (BoundActions.ContainsKey(key))
        //    {
        //        foreach (var i in BoundActions[key])
        //        {
        //            i(key, KeyState.Down);
        //        }
        //    }
        //}
        
        public void BindKeyTrigger(System.Windows.Input.Key key, Action<System.Windows.Input.Key, KeyState> action)
        {
            if(BoundActions.ContainsKey(key))
            {
                BoundActions[key].Add(action);
            }
            else
            {
                var temp = new List<Action<System.Windows.Input.Key, KeyState>>();
                temp.Add(action);
                BoundActions.Add(key, temp);
            }
        }
    }
}
