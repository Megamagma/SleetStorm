using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class TargetConstraintIsMiddleInRow : TargetConstraint
    {
        public TargetConstraint[] targetConstraints;
        public override bool Check(Entity target)
        {
            var targetConstraint = new TargetConstraintAnd() { constraints = targetConstraints };
            if (Battle.IsOnBoard(target))
            {
                if (target.GetAlliesInRow().ToArray().RemoveFromArray(item => targetConstraint.Check(item)).Count() == 2)
                {
                    if (target.InContainer((References.Battle.GetRows(target.owner)[0] as CardSlotLane).slots[1]) || target.InContainer((References.Battle.GetRows(target.owner)[1] as CardSlotLane).slots[1]))
                        return !not;
                }
            }

            return not;
        }

        public override bool Check(CardData targetData)
        {
            return not;
        }
    }
}
