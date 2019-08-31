using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static class InputStatus
    {
        private static Dictionary<string, bool> mButtonStatus { get; }
        private static Dictionary<string, float> mAxisStatus { get; }

        internal static void Update(Queue<InputAction> actions)
        {
            //input status update the button and axis status by reading action in listener
            //listener will update input status before new turn start
            foreach (var action in actions)
            {
                switch (action)
                {
                    case ButtonInputAction button:
                        mButtonStatus[button.Name] = button.Status; break;
                    case AxisInputAction axis:
                        mAxisStatus[axis.Name] = axis.Offset; break;
                    case CharInputAction charInput: break;
                    default: throw new Exception("Invalid Input Action.");
                }
            }
        }

        static InputStatus()
        {
            mButtonStatus = new Dictionary<string, bool>();
            mAxisStatus = new Dictionary<string, float>();
        }

        public static bool GetButton(string button) => mButtonStatus.ContainsKey(button) ? mButtonStatus[button] : false;

        public static float GetAxis(string axis) => mAxisStatus.ContainsKey(axis) ? mAxisStatus[axis] : 0;
    }
}
