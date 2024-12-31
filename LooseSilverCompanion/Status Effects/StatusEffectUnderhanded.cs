using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    public class StatusEffectUnderhanded : StatusEffectInstantTakeHealth
    {

        public override IEnumerator Process()
        {
            int amount = GetAmount();
            int num = Mathf.Min(target.hp.current, amount);
            target.hp.max -= amount;
            target.hp.current -= amount;
            target.PromptUpdate();
            Hit hit = new Hit(target, applier, 0)
            {
                canRetaliate = false,
                countsAsHit = false 
            };
            yield return hit.Process();
            yield return base.Process();
        }
        
    }
}
