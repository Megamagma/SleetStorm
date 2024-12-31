using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace sleetstorm
{
    public class StatusEffectTrueImmuneToX : StatusEffectImmuneToX
    {
        public string immunityType2 = "snow";

        public const int max2 = 1;

        public override void Init()
        {
            base.OnBegin += Begin;
        }

        public IEnumerator Begin()
        {
            StatusEffectData statusEffectData = target.FindStatus(immunityType2);
            if ((bool)statusEffectData && statusEffectData.count > 0)
            {
                yield return statusEffectData.RemoveStacks(statusEffectData.count, removeTemporary: false);
            }
        }

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if (apply.target == target && (bool)apply.effectData && apply.effectData.type == immunityType2)
            {
                apply.count = 0;
            }

            return false;
        }
    }
}
