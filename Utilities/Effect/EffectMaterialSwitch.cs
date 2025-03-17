using UnityEngine;

namespace BirdCase
{
    public class EffectMaterialSwitch : MonoBehaviour
    {
        public Material material;
        private void Start()
        {
            GetComponent<ParticleSystem>().GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
