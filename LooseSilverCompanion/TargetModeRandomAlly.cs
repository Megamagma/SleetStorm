using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class TargetModeRandomAlly : TargetMode
    {
        [SerializeField]
        public TargetConstraint[] constraints;
        public override bool Random => true;
        public override bool NeedsTarget => false;

        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(from e in entity.GetAllAllies()
                             where (bool)e && e.enabled && e.alive && e.canBeHit && CheckConstraints(e)
                             select e);
            if (hashSet.Count <= 0)
            {
                return null;
            }

            return new Entity[] { hashSet.ToArray().RandomItem() };
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(Battle.GetCardsOnBoard(target.owner));
            hashSet.Remove(entity);
            if (hashSet.Count <= 0)
            {
                return null;
            }

            return new Entity[] { hashSet.ToArray().RandomItem() };
        }

        public bool CheckConstraints(Entity target)
        {
            TargetConstraint[] array = constraints;
            if (array != null && array.Length > 0)
            {
                return constraints.Any((TargetConstraint c) => c.Check(target));
            }

            return true;
        }
    }

}
