using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sleetstorm
{
    internal class StatusEffectTriggerWhenAllyBehindAttacks : StatusEffectReaction
    {
        [SerializeField]
        public bool againstTarget;
        public readonly HashSet<Entity> prime = new HashSet<Entity>();

        public override bool RunHitEvent(Hit hit)
        {
            if (target.enabled && Battle.IsOnBoard(target) && hit.countsAsHit && hit.Offensive && (bool)hit.target && CheckEntity(hit.attacker))
                prime.Add(hit.attacker);
            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (prime.Count > 0 && prime.Contains(entity) && targets != null && targets.Length > 0)
            {
                prime.Remove(entity);
                if (Battle.IsOnBoard(target) && CanTrigger())
                    Run(entity, targets);
            }
            return false;
        }

        public void Run(Entity attacker, Entity[] targets)
        {
            if (againstTarget)
            {
                foreach (Entity target in targets)
                    ActionQueue.Stack(new ActionTriggerAgainst(this.target, attacker, target, null), true);
            }
            else
                ActionQueue.Stack(new ActionTrigger(this.target, attacker), true);
        }

        public bool CheckEntity(Entity entity) => (bool)entity && entity.owner.team == target.owner.team && entity != target && CheckBehind(entity) && Battle.IsOnBoard(entity) && CheckDuplicate(entity) && CheckDuplicate(entity.triggeredBy);

        public bool CheckBehind(Entity entity)
        {
            foreach (var cardContainer in target.actualContainers.ToArray())
            {
                if (!(cardContainer is CardSlot cardSlot) || !(cardContainer.Group is CardSlotLane group))
                {
                    continue;
                }

                for (var index = group.slots.IndexOf(cardSlot) + 1; index < group.slots.Count; ++index)
                {
                    var rowEntity = group.slots[index].GetTop();

                    if ((bool)rowEntity)
                    {
                        return entity == rowEntity;
                    }
                }
            }

            return false;
        }

        public bool CheckDuplicate(Entity entity)
        {
            return !entity.IsAliveAndExists() ||
                   entity.statusEffects.All(statusEffect => statusEffect.name != name);
        }
    }
}
