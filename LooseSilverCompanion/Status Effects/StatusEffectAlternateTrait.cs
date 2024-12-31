using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectAlternateTrait : StatusEffectTemporaryTrait
    {

        public TraitData[] traits;

        public int index = 0;
        public override void Init()
        {
            OnCardPlayed += (_, __) => { return Cycle(); };
            base.Init();
        }

        public IEnumerator Cycle()
        {
                yield return base.EndRoutine();

                index++; //Move to the next trait.
                index %= traits.Length; //Wrap-around if necessary
                trait = traits[index]; //Replace the current trait with the new one

                yield return base.StackRoutine(count);
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            return entity?.data?.cardType?.name == "Leader";
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            return (entity == target && !target.silenced);
        }

    }
}
