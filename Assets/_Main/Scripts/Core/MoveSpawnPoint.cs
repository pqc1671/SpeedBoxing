using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPointleft;
    [SerializeField] private Transform spawnPointright;
    [SerializeField]List<Transform> pos;


    void ChangePosition()
    {
        HashSet<Transform> usedPositions = new HashSet<Transform>();

        if (pos.Count > 0)
        {
            Transform leftPosition;
            do
            {
                leftPosition = pos[UnityEngine.Random.Range(0, pos.Count)];
            } while (usedPositions.Contains(leftPosition));

            spawnPointleft = leftPosition;
            usedPositions.Add(leftPosition);
        }

        if (pos.Count > 0)
        {
            Transform rightPosition;
            do
            {
                rightPosition = pos[UnityEngine.Random.Range(0, pos.Count)];
            } while (usedPositions.Contains(rightPosition));

            spawnPointright = rightPosition;
            usedPositions.Add(rightPosition);
        }
    }
}
