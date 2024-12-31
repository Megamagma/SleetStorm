using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectBonusDamageEqualToXBoostable : StatusEffectBonusDamageEqualToX
    {
        public override void Init()
        {
            PreCardPlayed += Gain2;
        }
        public IEnumerator Gain2(Entity entity, Entity[] targets)
        {
            int num = Find() * GetAmount();
            if (!toReset || num != currentAmount)
            {
                if (toReset)
                {
                    LoseCurrentAmount();
                }

                if (num > 0)
                {
                    yield return GainAmount(num);
                }
            }
        }
    }
}
