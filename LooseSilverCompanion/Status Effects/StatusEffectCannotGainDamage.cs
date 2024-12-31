using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    public class StatusEffectCannotGainDamage : StatusEffectData
    {
        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if (apply.target == this.target && !this.target.silenced && CheckEffectType(apply.effectData))
            {
                apply.count = 0;
            }
            return false;
        }

        public static bool CheckEffectType(StatusEffectData effectData)
        {
            if (effectData)
            {
                string type = effectData.type;
                if (effectData is StatusEffectApplyX) return ((StatusEffectApplyX)effectData).effectToApply.type == "damage up";
                return type == "damage up";
            }
            return false;
        }
    }
}
