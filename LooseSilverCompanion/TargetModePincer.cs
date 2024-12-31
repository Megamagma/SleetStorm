using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class TargetModePincer : TargetModeRow
    {
        public override bool TargetRow => true;
        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            if ((bool)targetContainer)
            {
                AddEligible(entity, hashSet, targetContainer);
            }
            else if ((bool)target)
            {
                switch (target.containers.Length)
                {
                    case 1:
                        {
                            CardContainer fromCollection = target.containers[0];
                            AddEligible(entity, hashSet, fromCollection);
                            break;
                        }
                    case 2:
                        {
                            int[] rowIndices = References.Battle.GetRowIndices(entity);
                            int[] rowIndices2 = References.Battle.GetRowIndices(target);
                            foreach (int item in rowIndices.Intersect(rowIndices2))
                            {
                                List<Entity> enemiesInRow = entity.GetEnemiesInRow(item);
                                AddEligible(entity, hashSet, enemiesInRow);
                            }

                            break;
                        }
                }
            }
            else
            {
                int[] rowIndices3 = Battle.instance.GetRowIndices(entity);
                int[] array = rowIndices3;
                foreach (int rowIndex in array)
                {
                    List<Entity> enemiesInRow2 = entity.GetEnemiesInRow(rowIndex);
                    AddEligible(entity, hashSet, enemiesInRow2);
                }

                if (hashSet.Count == 0)
                {
                    int rowCount = Battle.instance.rowCount;
                    for (int j = 0; j < rowCount; j++)
                    {
                        if (!rowIndices3.Contains(j))
                        {
                            List<Entity> enemiesInRow3 = entity.GetEnemiesInRow(j);
                            AddEligible(entity, hashSet, enemiesInRow3);
                        }
                    }
                }
            }

            if (hashSet.Count <= 0)
            {
                return null;
            }

            var hashSet2 = hashSet.ToArray();
            if (hashSet2.Count() > 2)
            {
                hashSet2 = new Entity[] { hashSet2[0], hashSet2[hashSet2.Count() - 1] };
            }
            return hashSet2;
        }


        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }
    }
}
