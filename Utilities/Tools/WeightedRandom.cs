using UnityEngine;
using System.Collections.Generic;

namespace BirdCase
{
    /// <summary>
    /// 가중치 랜덤 뽑기 시스템 클래스
    /// </summary>
    public class WeightedRandom<T>
    {
        public WeightedRandom()
        {
            _dic = new Dictionary<T, int>();
        }
        
        private Dictionary<T, int> _dic;

        /// <summary>
        /// 가중치 리스트에 아이템과 수량을 추가함
        /// </summary>
        public void Add(T item, int value)
        {
            //음수가 들어오면 리턴한다.
            if (value < 0)
            {
                Debug.LogError("음수는 들어갈 수 없습니다.");
                return;
            }

            if (value == 0)
            {
                //Debug.LogWarning(item + "의 가중치가 0입니다.");
                return;
            }

            //만약 딕셔너리에 키가 존재하면?
            if (_dic.ContainsKey(item))
            {
                //해당 키의 값의 수치를 변경한다.
                _dic[item] += value;
            }
            //존재하지 않으면?
            else
            {
                //아이템을 추가한다.
                _dic.Add(item, value);
            }
        }

        /// <summary>
        /// 가중치 리스트에 아이템이 있으면 지정 수량을 빼고, 지정 수량이 더 크면 리스트에서 아이템을 뺌
        /// </summary>
        public void Sub(T item, int value)
        {
            //음수가 들어오면 리턴한다.
            if (value < 0)
            {
                Debug.LogError("음수는 들어갈 수 없습니다.");
                return;
            }

            //만약 딕셔너리에 키가 존재하면?
            if (_dic.ContainsKey(item))
            {
                //키의 값의 크기가 더 크면?
                if (_dic[item] > value)
                {
                    //해당 키의 값의 수치를 변경한다.
                    _dic[item] -= value;
                }
                //같거나 작으면?
                else
                {
                    //삭제한다.
                    Remove(item);
                }
            }
            else
            {
                Debug.LogError("아이템이 존재하지 않습니다.");
            }
        }

        /// <summary>
        /// 리스트에서 아이템을 삭제
        /// </summary>
        public void Remove(T item)
        {
            //만약 딕셔너리에 키가 존재하면?
            if (_dic.ContainsKey(item))
            {
                //해당 키의 데이터를 삭제한다.
                _dic.Remove(item);
            }
            else
            {
                Debug.LogError("아이템이 존재하지 않습니다.");
            }
        }

        /// <summary>
        /// 현재 리스트에 있는 아이템의 가중치를 모두 더해 반환
        /// </summary>
        public int GetTotalWeight()
        {
            int totalWeight = 0;

            //딕셔너리에 입력된 모든 아이템 가중치 값을 더한다.
            foreach (int value in _dic.Values)
            {
                totalWeight += value;
            }

            return totalWeight;
        }


        /// <summary>
        /// 아이템 리스트에 있는 모든 아이템의 가중치를 비율로 변환하여 반환 (0, 1 사이)
        /// </summary>
        public Dictionary<T, float> GetPercent()
        {
            Dictionary<T, float> _tempDic = new Dictionary<T, float>();
            float totalWeight = GetTotalWeight();

            foreach (var item in _dic)
            {
                _tempDic.Add(item.Key, item.Value / totalWeight);
            }

            return _tempDic;
        }

        /// <summary>
        /// 아이템 리스트에서 랜덤으로 아이템을 뽑아 반환(뽑힌 아이템의 갯수 -1)
        /// </summary>
        public T GetRamdomItemBySub()
        {
            //딕셔너리에 들어있는 아이템 갯수가 0이하면
            if (_dic.Count <= 0)
            {
                Debug.LogError("리스트에 아이템이 없습니다. 뽑기 불가능");
                return default;
            }

            //총 가중치를 가져온다.
            int weight = 0;
            int totalWeight = GetTotalWeight();

            //총 가중치 값에 0~1f의 랜덤 값을 곱해 기준점을 구한다.
            int pivot = Mathf.RoundToInt(totalWeight * Random.Range(0.0f, 1.0f));

            //딕셔너리를 순회하며 가중치를 더하다 기준점 이상이 되면 그 아이템을 반환한다.
            foreach (var item in _dic)
            {
                weight += item.Value;
                if (pivot <= weight)
                {
                    _dic[item.Key] -= 1;
                    return item.Key;
                }
            }

            return default;
        }

        /// <summary>
        /// 아이템 리스트에서 랜덤으로 아이템을 뽑아 반환
        /// </summary>
        public T GetRandomItem()
        {
            //딕셔너리에 들어있는 아이템 갯수가 0이하면 리턴
            if (_dic.Count <= 0)
            {
                Debug.LogError("리스트에 아이템이 없습니다. 뽑기 불가능");
                return default;
            }

            //총 가중치를 가져온다.
            int totalWeight = GetTotalWeight();
            int weight = 0;

            //총 가중치 값에 0~1f의 랜덤 값을 곱해 기준점을 구한다.
            int pivot = Mathf.RoundToInt(totalWeight * Random.Range(0.0f, 1.0f));

            //딕셔너리를 순회하며 가중치를 더하다 기준점 이상이 되면 그 아이템을 반환한다.
            foreach (var item in _dic)
            {
                weight += item.Value;
                if (pivot <= weight)
                {
                    return item.Key;
                }
            }

            return default;
        }

        /// <summary>
        /// 아이템 리스트를 반환
        /// </summary>
        public Dictionary<T, int> GetList()
        {
            return _dic;
        }
    }
}