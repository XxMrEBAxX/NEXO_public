using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class MeshDemolisherTool : NetworkBehaviour, IGetOffLauncher
    {
        public GameObject mesh;
        public Transform parentTransform;
        public GameObject maskObject;
        public Ease maskEase = Ease.Linear;
        private float originMaskLocalZ = 0;
        private List<Vector3> originalPositions = new List<Vector3>();
        public event Action<IGetOffLauncher> GetOffLauncher;
        public bool isKinematic { get; private set; } = true;
        private Material childMaterial;
        public Material dissolveMaterial;
        public ParticleSystem dustParticle;
        public Image fillImage;
        public EventReference demolishSound;

        private float defaultDissolveOffsetZ = -0.4f;

        private void Start()
        {
            isHelpFirstDemolish = false;
            childMaterial = new Material(dissolveMaterial.shader);
            childMaterial.CopyPropertiesFromMaterial(dissolveMaterial);
            List<Material> materials = new List<Material>
            {
                childMaterial
            };

            childMaterial.SetVector("_DissolveOffset", new Vector3(0, 0, defaultDissolveOffsetZ));
            originalPositions.Clear();
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                child.GetComponent<MeshRenderer>().SetSharedMaterials(materials);
                originalPositions.Add(child.position);
                child.GetComponent<Rigidbody>().isKinematic = true;
            }
            isKinematic = true;
            mesh.SetActive(true);
            parentTransform.gameObject.SetActive(false);
            originMaskLocalZ = maskObject.transform.localPosition.z;
            fillImage.fillAmount = 0;
            fillImage.transform.parent.gameObject.SetActive(false);
        }

        [ContextMenu("Reset")]
        public void Reset()
        {
            fillImage.transform.parent.gameObject.SetActive(false);
            childMaterial.SetVector("_DissolveOffset", new Vector3(0, 0, defaultDissolveOffsetZ));
            if (originalPositions.Count == 0)
                return;

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                child.position = originalPositions[i];
                child.rotation = Quaternion.identity;
                child.GetComponent<Rigidbody>().isKinematic = true;
            }
            isKinematic = true;
            mesh.SetActive(true);
            parentTransform.gameObject.SetActive(false);

            maskObject.transform.localPosition = new Vector3(maskObject.transform.localPosition.x, maskObject.transform.localPosition.y, 0.8f);
            maskObject.transform.DOLocalMoveZ(originMaskLocalZ, 1.2f).SetEase(maskEase);
        }

        [ContextMenu("Demolish")]
        public void Demolish()
        {
            ShowHelpFirstDemolish();
            maskObject.transform.DOKill();
            dustParticle.Play();
            SoundManager.Instance.Play(demolishSound, SoundManager.Banks.SFX, 1, transform.position);

            if (originalPositions.Count == 0 || !isKinematic)
                return;

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                Rigidbody rb = child.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                Vector3 dir = new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 1.5f), UnityEngine.Random.Range(-1, 1));
                StartCoroutine(AddForceInFixedTime(rb, dir));
            }
            isKinematic = false;
            mesh.SetActive(false);
            parentTransform.gameObject.SetActive(true);
            GetOffLauncher?.Invoke(this);

            //StartCoroutine(DisappearPlatformCoroutine(1));
        }

        private IEnumerator AddForceInFixedTime(Rigidbody rb, Vector3 dir)
        {
            yield return new WaitForFixedUpdate();
            rb.AddForce(dir * 2, ForceMode.Impulse);
            rb.AddTorque(-dir * 2, ForceMode.Impulse);
        }

        private Coroutine platformCoroutine;

        public void Appear(float time)
        {
            if (platformCoroutine != null)
                StopCoroutine(platformCoroutine);
            platformCoroutine = StartCoroutine(AppearPlatformCoroutine(time));
        }


        public void Disappear(float time, float appearTime = 0)
        {
            DemolishClientRpc();
            if (platformCoroutine != null)
                StopCoroutine(platformCoroutine);
            platformCoroutine = StartCoroutine(DisappearPlatformCoroutine(time, appearTime));
        }

        private float curTime = 0;
        private IEnumerator AppearPlatformCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
            ResetClientRpc();
        }

        [ClientRpc]
        private void SetFillAmountClientRPC(float time)
        {
            fillImage.fillAmount = 0;
            fillImage.transform.parent.gameObject.SetActive(true);
            StartCoroutine(SetFillAmountCoroutine(time));
        }

        private IEnumerator SetFillAmountCoroutine(float time)
        {
            curTime = Time.time;
            while (curTime + time > Time.time)
            {
                if (isKinematic)
                    break;

                float t = (Time.time - curTime) / time;
                fillImage.fillAmount = t;
                yield return null;
            }
            fillImage.fillAmount = 0;
        }

        private IEnumerator DisappearPlatformCoroutine(float time, float appearTime = 0)
        {
            yield return new WaitForSeconds(time);
            DisappearMaterialClientRpc();
            if (appearTime != 0)
            {
                SetFillAmountClientRPC(appearTime);
                Appear(appearTime);
            }
        }

        [ClientRpc]
        public void DemolishClientRpc()
        {
            Demolish();
        }

        [ClientRpc]
        public void ResetClientRpc()
        {
            Reset();
        }

        [ClientRpc]
        private void DisappearMaterialClientRpc()
        {
            StartCoroutine(DisappearMaterialCoroutine(2.5f));
        }

        private IEnumerator DisappearMaterialCoroutine(float time)
        {
            float curTime = 0;
            while (curTime < time)
            {
                curTime += Time.deltaTime;
                childMaterial.SetVector("_DissolveOffset", new Vector3(0, 0, Mathf.Lerp(defaultDissolveOffsetZ, -defaultDissolveOffsetZ, curTime / time)));
                yield return null;
            }
            parentTransform.gameObject.SetActive(false);
        }

        static bool isHelpFirstDemolish = false;
        public void ShowHelpFirstDemolish()
        {
            if (!isHelpFirstDemolish)
            {
                isHelpFirstDemolish = true;
                HelpWindowUI.Instance.RequestHelpText(HelpWindowUI.HelpType.PLATFORM_DESTROY);
            }
        }
    }
}
