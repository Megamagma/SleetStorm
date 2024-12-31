using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sleetstorm
{
    internal class StatusEffectInstantSwapToOtherRow : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            int[] rowIndices = Battle.instance.GetRowIndices(target);
            if (rowIndices.Length == 1)
            {
                CardContainer oldRow = Battle.instance.GetRow(target.owner, rowIndices[0]);
                int index = oldRow.IndexOf(target);

                //First, get a row that isn't the current row.
                List<CardContainer> allyRows = Battle.instance.GetRows(target.owner);
                CardContainer newRow = allyRows.Where(container => !target.containers.Contains(container)).ToArray().RandomItem();

                if (newRow != null)
                {
                    bool completed = false;
                    Entity swappy = newRow[index];
                    int swappyIndex = -1;
                    if (swappy != null) //Meet Swappy; The former owner of slot[index] of newRow.
                    {
                        swappyIndex = newRow.IndexOf(swappy);
                    }

                    oldRow.RemoveAt(index);
                    if (newRow.Count == newRow.max || swappyIndex == 0) //If the target row is full, or Swappy is in the front of the row, swap the entities and call it a day.
                    {
                        newRow.RemoveAt(index);
                        oldRow.Insert(index, swappy);
                        completed = true;
                    }
                    newRow.Insert(index, target); //Move the target to the new row.


                    if (!completed && swappy != null)
                    {
                        if (newRow.IndexOf(swappy) != swappyIndex) //If Swappy was forced to move because of the move, it should actually swap rows instead.
                        {
                            newRow.Remove(swappy);
                            oldRow.Insert(index, swappy);
                        }
                    }

                    oldRow.TweenChildPositions();
                    newRow.TweenChildPositions();
                }
            }

            yield return base.Process();
        }
    }
}
