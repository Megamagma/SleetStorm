﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectApplyRandomInstant : StatusEffectApplyXInstant
    {
        public StatusEffectData[] effectsToApply;

        public override void Init()
        {
            base.Init();
            Events.OnActionQueued += DetermineEffect;
        }

        private void DetermineEffect(PlayAction arg)
        {
            int r = UnityEngine.Random.Range(0, effectsToApply.Length);
            effectToApply = effectsToApply[r];
        }

    }
}
