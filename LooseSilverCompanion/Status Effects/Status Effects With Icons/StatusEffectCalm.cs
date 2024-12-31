using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class StatusEffectCalm : StatusEffectApplyX
    {
        public StatusEffectData counterIncreaseEffect;
        public StatusEffectData countDownEffect;

        private int _currentReduction;
        public override object GetMidBattleData()
        {
            return _currentReduction;
        }

        public override void RestoreMidBattleData(object data)
        {
            _currentReduction = (int)data;
        }

        public override void Init()
        {
            var previousCalm = target.FindStatus(type);
            if (previousCalm is not null && previousCalm.name != name)
            {
                target.statusEffects.Remove(this);
                ActionQueue.Stack(new ActionApplyStatus(target, applier, previousCalm, count));
                return;
            }

            OnStack += Stack;
            OnHit += HitEvent;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.target != target) return false;
            if (!hit.Offensive) return false;

            return hit.damage > 0;
        }

        private IEnumerator HitEvent(Hit hit)
        {
            var halved = Math.Max(count / 2, 1);
            var leftover = count - halved;
            var increase = count / 3 - leftover / 3;

            increase = Math.Min(increase, Math.Max(_currentReduction - leftover / 3, 0));
            _currentReduction -= increase;

            yield return StatusEffectSystem.Apply(target, target, counterIncreaseEffect, increase);
            target.display.promptUpdateDescription = true;
            yield return RemoveStacks(halved, false);
        }

        private IEnumerator Stack(int stacks)
        {
            var oldCount = count - stacks;
            var canReduceBy = target.counter.max - 1;
            var decreaseCounterBy = count / 3 - oldCount / 3;

            var actualDecrease = Math.Min(decreaseCounterBy, canReduceBy);

            if (actualDecrease > 0)
            {
                _currentReduction += actualDecrease;
                yield return Run(GetTargets(), actualDecrease);
            }
            else if (decreaseCounterBy > 0)
            {
                yield return StatusEffectSystem.Apply(target, target, countDownEffect, 1);
            }

            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }
    }
}
