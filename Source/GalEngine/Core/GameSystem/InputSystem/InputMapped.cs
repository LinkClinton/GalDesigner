using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class InputMapped
    {
        private readonly Dictionary<string, string> mInputMapeed;

        public InputMapped()
        {
            mInputMapeed = new Dictionary<string, string>();
        }

        public void Mapped(string input, string output)
            => mInputMapeed[input] = output;
        
        public void Remove(string input)
            => mInputMapeed.Remove(input);
        
        public bool IsSame(string input0, string input1)
        {
            if (!mInputMapeed.ContainsKey(input0)) return false;

            return input1 == mInputMapeed[input0];
        }

        public bool IsMapped(string input)
        {
            return mInputMapeed.ContainsKey(input);
        }

        public string MappedInput(string input)
        {
            //if we do not mapped the input, we only return the input self.
            if (!mInputMapeed.ContainsKey(input)) return input;

            return mInputMapeed[input];
        }
    }
}
