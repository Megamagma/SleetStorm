using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectInstantFillXBoardSlots : StatusEffectInstant
    {
        public bool random = false;

        public bool clearBoardFirst = false;

        public CardData[] withCards;

        public readonly List<CardData> pool = new List<CardData>();

        public override IEnumerator Process()
        {
            List<CardContainer> rows = References.Battle.GetRows(target.owner);
            if (clearBoardFirst)
            {
                foreach (CardContainer cardContainer in rows)
                {
                    CardSlotLane cardSlotLane2 = cardContainer as CardSlotLane;
                    if (!(cardSlotLane2 != null))
                    {
                        continue;
                    }

                    foreach (CardSlot slot3 in cardSlotLane2.slots)
                    {
                        if (slot3.Empty || !slot3.entities.All((Entity e) => e.name != target.name))
                        {
                            continue;
                        }

                        List<Entity> toKill = new List<Entity>(slot3.entities);
                        foreach (Entity e2 in toKill)
                        {
                            yield return e2.Kill(DeathType.Sacrifice);
                        }
                    }
                }
            }

            List<CardSlot> list = new List<CardSlot>();
            int amount = GetAmount();
            int i = 0;
            if (!random)
            {
                foreach (CardContainer cardContainer2 in rows)
                {
                    CardSlotLane cardSlotLane3 = cardContainer2 as CardSlotLane;
                    if (!(cardSlotLane3 != null))
                    {
                        continue;
                    }

                    foreach (CardSlot slot4 in cardSlotLane3.slots)
                    {
                        if (slot4.Empty && i < amount)
                        {
                            list.Add(slot4);
                            i++;
                        }
                    }
                }
            }
            else
            {
                while (i < amount && rows.SelectMany((CardContainer row) => (row as CardSlotLane).slots.Where((CardSlot slot) => slot.Empty && !list.Contains(slot))).Any())
                {
                    CardContainer[] containers = rows.ToArray();
                    CardSlotLane cardSlotLane = containers.RandomItem() as CardSlotLane;
                    if (cardSlotLane != null)
                    {
                        CardSlot slot2 = cardSlotLane.slots.RandomItem();
                        if (slot2.Empty && i < amount && !list.Contains(slot2))
                        {
                            list.Add(slot2);
                            i++;
                        }
                    }
                }
            }

            foreach (CardSlot slot5 in list)
            {
                CardData data = Pull().Clone();
                Card card = CardManager.Get(data, References.Battle.playerCardController, target.owner, inPlay: true, target.owner.team == References.Player.team);
                yield return card.UpdateData();
                CardDiscoverSystem.instance.DiscoverCard(data);
                target.owner.reserveContainer.Add(card.entity);
                target.owner.reserveContainer.SetChildPosition(card.entity);
                ActionQueue.Stack(new ActionMove(card.entity, slot5), fixedPosition: true);
                ActionQueue.Stack(new ActionRunEnableEvent(card.entity), fixedPosition: true);
            }

            list = null;
            yield return base.Process();
        }

        public CardData Pull()
        {
            if (pool.Count <= 0)
            {
                pool.AddRange(withCards);
            }

            return pool.TakeRandom();
        }
    }
}
