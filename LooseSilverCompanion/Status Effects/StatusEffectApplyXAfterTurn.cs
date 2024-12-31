using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectApplyXAfterTurn : StatusEffectApplyX
    {
        private bool primed;

        public override void Init()
        {
            OnActionPerformed += ActionPerformed;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (action is ActionTrigger triggerAction && triggerAction.entity == target && !target.silenced) primed = true;
            return primed && ActionQueue.Empty;
        }

        private IEnumerator ActionPerformed(PlayAction action)
        {
            primed = false;
            yield return Run(GetTargets());
        }
    }
}
