using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class AnimationSequence : List<AnimationCel>
    {
        private int loopToFrame = 0;

        public string Label = "";
        public int SelectedCel = 0;

        public int LoopToFrame
        {
            get
            {
                return loopToFrame;
            }

            set
            {
                loopToFrame = value;
            }
                
        }


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
