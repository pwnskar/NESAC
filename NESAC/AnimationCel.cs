using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class AnimationCel
    {
        //public MetaSprite MetaSprite;// = new MetaSprite();
        public int MetaSpriteIndex = 0;
        public int TimeDelay = 0;
        public int CountDown = 0;

        public AnimationCel()
        {

        }

        public AnimationCel(int metaSpriteIndex, int timeDelay)
        {
            //this.MetaSprite = metaSprite;
            this.MetaSpriteIndex = metaSpriteIndex;
            this.TimeDelay = timeDelay;
            this.CountDown = timeDelay;
        }
    }
}
