using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class AnimationSequence : List<AnimationCel>
    {
        public string Label = "";
        public int LoopToFrame = 0;
        public int SelectedCel = 0;

        public AnimationSequence(string label)
        {
            this.Label = label;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
