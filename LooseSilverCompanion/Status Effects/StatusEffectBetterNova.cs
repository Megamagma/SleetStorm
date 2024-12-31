using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    public class StatusEffectBetterNova : StatusEffectApplyXWhenUnitLosesY
    {
        public override bool RunCardMoveEvent(Entity entity)
        {
            StoreCurrentAmount(entity);
            return base.RunCardMoveEvent(entity);
        }

    }
}
