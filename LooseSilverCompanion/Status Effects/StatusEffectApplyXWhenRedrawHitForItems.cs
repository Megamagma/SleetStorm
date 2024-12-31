﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectApplyXWhenRedrawHitForItems : StatusEffectApplyX
    {
        public override void Init()
        {
            Events.OnRedrawBellHit += RedrawBellHit;
        }

        public void OnDestroy()
        {
            Events.OnRedrawBellHit -= RedrawBellHit;
        }

        public void RedrawBellHit(RedrawBellSystem redrawBellSystem)
        {
            ActionQueue.Stack(new ActionSequence(Run(GetTargets())), fixedPosition: true);
        }
    }
}
