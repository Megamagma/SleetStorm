using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class TargetConstraintRightMostWithoutTrait : TargetConstraint
    {
        public TraitData[] withoutTraits;
        public override bool Check(Entity target)
        {
            var hand = References.Player.handContainer?.ToList();
            if (hand == null)
                return not;

            var result = hand.First(e => !withoutTraits.ToList().ContainsAny(e.traits.Select(t => t.data))) == target;
            return not ? !result : result;
        }
    }
}
