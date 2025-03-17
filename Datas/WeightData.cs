using System;
using MyBox;
using UnityEngine;

namespace BirdCase
{
    [CreateAssetMenu(menuName = "Datas/BossData/BossOneWeightData")]
    public class BossOneWeightData : ScriptableObject
    {
        [Serializable]
        public class WeightAreaData
        {
            [OverrideLabel("좌측 레이저 이동 가중치")]
            [SerializeField] private int leftLaserWeight;
            public int LeftLaserWeight => leftLaserWeight;

            [OverrideLabel("우측 레이저 이동 가중치")]
            [SerializeField] private int rightLaserWeight;
            public int RightLaserWeight => rightLaserWeight;

            [OverrideLabel("왼손 찍기 가중치")]
            [SerializeField] private int leftDownWeight;
            public int LeftDownWeight => leftDownWeight;

            [OverrideLabel("오른손 찍기 가중치")]
            [SerializeField] private int rightDownWeight;
            public int RightDownWeight => rightDownWeight;

            [OverrideLabel("어글자 찍기 가중치")]
            [SerializeField] private int targetDownWeight;
            public int TargetDownWeight => targetDownWeight;

            [OverrideLabel("왼쪽 잡기 가중치")]
            [SerializeField] private int leftGrabWeight;
            public int LeftGrabWeight => leftGrabWeight;

            [OverrideLabel("오른쪽 잡기 가중치")]
            [SerializeField] private int rightGrabWeight;
            public int RightGrabWeight => rightGrabWeight;

            [OverrideLabel("왼손 타격 가중치")]
            [SerializeField] private int leftSwingWeight;
            public int LeftSwingWeight => leftSwingWeight;

            [OverrideLabel("오른손 타격 가중치")]
            [SerializeField] private int rightSwingWeight;
            public int RightSwingWeight => rightSwingWeight;

            [OverrideLabel("전기 방출 가중치")]
            [SerializeField] private int electronicWeight;
            public int ElectronicWeight => electronicWeight;

            [OverrideLabel("전선비 가중치")]
            [SerializeField] private int electronicWireWeight;
            public int ElectronicWireWeight => electronicWireWeight;

            [ReadOnly, OverrideLabel("총 가중치")]
            [SerializeField] private int totalWeight;
            public int TotalWeight => totalWeight;

            public void TotalWeightCalculate()
            {
                totalWeight = leftLaserWeight + rightLaserWeight + leftDownWeight + rightDownWeight + targetDownWeight +
                              leftGrabWeight + rightGrabWeight + leftSwingWeight + rightSwingWeight + electronicWeight + electronicWireWeight;
            }
        }

        [Serializable]
        public class WeightData
        {
            [OverrideLabel("1구역 가중치")]
            [SerializeField] private WeightAreaData areaOneWeightAreaData;
            public WeightAreaData AreaOneWeightAreaData => areaOneWeightAreaData;

            [OverrideLabel("2구역 가중치")]
            [SerializeField] private WeightAreaData areaTwoWeightAreaData;
            public WeightAreaData AreaTwoWeightAreaData => areaTwoWeightAreaData;

            [OverrideLabel("3구역 가중치")]
            [SerializeField] private WeightAreaData areaThreeWeightAreaData;
            public WeightAreaData AreaThreeWeightAreaData => areaThreeWeightAreaData;

            [OverrideLabel("4구역 가중치")]
            [SerializeField] private WeightAreaData areaFourWeightAreaData;
            public WeightAreaData AreaFourWeightAreaData => areaFourWeightAreaData;

            [OverrideLabel("5구역 가중치")]
            [SerializeField] private WeightAreaData areaFiveWeightAreaData;
            public WeightAreaData AreaFiveWeightAreaData => areaFiveWeightAreaData;

            [OverrideLabel("6구역 가중치")]
            [SerializeField] private WeightAreaData areaSixWeightAreaData;
            public WeightAreaData AreaSixWeightAreaData => areaSixWeightAreaData;
        }

        [OverrideLabel("보스 조우시 가중치")]
        [SerializeField] private WeightAreaData firstWeightData;
        public WeightAreaData FirstWeightData => firstWeightData;

        [OverrideLabel("1페이지 100 ~ 51% 가중치")]
        [SerializeField] private WeightData oneWeightData;
        public WeightData OneWeightData => oneWeightData;

        [OverrideLabel("1페이지 50 ~ 0% 가중치")]
        [SerializeField] private WeightData twoWeightData;
        public WeightData TwoWeightData => twoWeightData;

        [OverrideLabel("2페이지 100 ~ 51% 가중치")]
        [SerializeField] private WeightData threeWeightData;
        public WeightData ThreeWeightData => threeWeightData;

        [OverrideLabel("2페이지 50 ~ % 가중치")]
        [SerializeField] private WeightData fourWeightData;
        public WeightData FourWeightData => fourWeightData;

        private void OnValidate()
        {
            firstWeightData.TotalWeightCalculate();
            WeightData[] weightData = new WeightData[4];
            weightData[0] = oneWeightData;
            weightData[1] = twoWeightData;
            weightData[2] = threeWeightData;
            weightData[3] = fourWeightData;

            foreach (var data in weightData)
            {
                data.AreaOneWeightAreaData.TotalWeightCalculate();
                data.AreaTwoWeightAreaData.TotalWeightCalculate();
                data.AreaThreeWeightAreaData.TotalWeightCalculate();
                data.AreaFourWeightAreaData.TotalWeightCalculate();
                data.AreaFiveWeightAreaData.TotalWeightCalculate();
                data.AreaSixWeightAreaData.TotalWeightCalculate();
            }
        }
    }
}
