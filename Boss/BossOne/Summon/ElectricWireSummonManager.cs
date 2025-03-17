using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BirdCase
{
    public class ElectricWireSummonManager : NetworkBehaviour
    {
        public BossOne BossOne;
        [SerializeField] private Collider summonCreateArea;
        private ElectricWireSummon[] electricWireSummon;
        private List<Vector3> spawnPositions = new List<Vector3>();
        private float colliderXMin;
        private float colliderXMax;
        private int gridRows;
        private int deadCount = 0;
        public bool IsSummoning { get; private set; }
        private CancellationTokenSource cancel;

        private void Start()
        {
            cancel = new CancellationTokenSource();
            BossOne = FindFirstObjectByType<BossOne>();
            electricWireSummon = gameObject.GetComponentsInChildren<ElectricWireSummon>();
            for (int i = 0; i < electricWireSummon.Length; i++)
            {
                electricWireSummon[i].DeadAction += OnSummonDied;
                electricWireSummon[i].SetActiveAndPosWireClientRpc(false, new Vector3(0, BossOne.SummonData.ElectricWireSetCreateHeight, 0));
            }
            deadCount = 0;

            gridRows = electricWireSummon.Length;
            colliderXMin = summonCreateArea.bounds.min.x;
            colliderXMax = summonCreateArea.bounds.max.x;
        }

        private void Init()
        {
            for (int i = 0; i < electricWireSummon.Length; i++)
            {
                electricWireSummon[i].SetActiveAndPosWireClientRpc(false, new Vector3(0, BossOne.SummonData.ElectricWireSetCreateHeight, 0));
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnDisable();
        }

        private void OnDisable()
        {
            if(cancel != null)
            {
                cancel.Cancel();
                cancel.Dispose();
                cancel = null;
            }
        }

        public async UniTaskVoid Summon()
        {
            OnDisable();
            cancel = new CancellationTokenSource();
            Init();
            GetSummonPosition();
            Shuffle(spawnPositions);
            IsSummoning = true;

            try
            {
                await UniTask.Delay(500);
                cancel.Token.ThrowIfCancellationRequested();

                for (int i = 0; i < electricWireSummon.Length; i++)
                {
                    //Debug.Log(i + "번째 : " + spawnPositions[i]);
                    electricWireSummon[i].SetActiveAndPosWireClientRpc(true, spawnPositions[i]);
                    electricWireSummon[i].WarningEffectDownRayCasting(spawnPositions[i]);
                    await UniTask.Delay(TimeSpan.FromSeconds(BossOne.SummonData.ElectricWireSetInterval));
                    cancel.Token.ThrowIfCancellationRequested();
                }
            }
            catch { IsSummoning = false; }
            spawnPositions.Clear();
            IsSummoning = false;
        }

        public void Unsummon()
        {
            OnDisable();
            for (int i = 0; i < electricWireSummon.Length; i++)
            {
                electricWireSummon[i].SetActiveAndPosWireClientRpc(false, new Vector3(0, BossOne.SummonData.ElectricWireSetCreateHeight, 0));
            }
            deadCount = 0;
            IsSummoning = false;
        }

        /// <summary>
        /// 영역을 균등하게 나누어 소환 위치를 설정
        /// </summary>
        private void GetSummonPosition()
        {
            float cellWidth = (colliderXMax - colliderXMin) / gridRows;

            for (int row = 0; row < gridRows; row++)
            {
                float xPosition = colliderXMin + cellWidth * (row + 0.5f);
                spawnPositions.Add(new Vector3(xPosition, BossOne.SummonData.ElectricWireSetCreateHeight, 0));
            }
        }

        /// <summary>
        /// 리스트를 셔플하여 무작위로 섞음
        /// </summary>
        private void Shuffle(List<Vector3> list)
        {
            List<Vector3> temp = new List<Vector3>(new Vector3[list.Count]);
            int halfCount = Mathf.FloorToInt(list.Count * 0.5f);

            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex;

                if (i % 2 == 0)
                {
                    randomIndex = UnityEngine.Random.Range(0, halfCount);
                }
                else
                {
                    randomIndex = UnityEngine.Random.Range(halfCount + 1, list.Count);
                }

                temp[i] = list[randomIndex];
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = temp[i];
            }
        }

        private void OnSummonDied()
        {
            deadCount++;
            if (deadCount >= electricWireSummon.Length)
            {
                deadCount = 0;
            }
        }
    }
}
