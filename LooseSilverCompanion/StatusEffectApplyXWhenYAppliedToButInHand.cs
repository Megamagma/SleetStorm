using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class StatusEffectApplyXWhenYAppliedToButInHand : StatusEffectApplyXWhenYAppliedTo
    {
        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if ((adjustAmount || instead) && target.enabled && !TargetSilenced() && (target.alive || !targetMustBeAlive) && (bool)apply.effectData && apply.count > 0 && CheckType(apply.effectData) && CheckTarget(apply.target))
            {
                if (instead)
                {
                    apply.effectData = effectToApply;
                }

                if (adjustAmount)
                {
                    apply.count += addAmount;
                    apply.count = Mathf.RoundToInt((float)apply.count * multiplyAmount);
                }
            }

            return false;
        }
        public new bool CheckTarget(Entity entity)
        {

            if (entity == target)
            {
                return CheckFlag(ApplyToFlags.Self);
            }

            if (entity.owner == target.owner)
            {
                return CheckFlag(ApplyToFlags.Allies);
            }

            if (entity.owner != target.owner)
            {
                return CheckFlag(ApplyToFlags.Enemies);
            }

            return false;
        }
    }
}
