﻿using RPG.Control;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Abilities.Targeting
{

    [CreateAssetMenu(fileName = "Delayed Click Targeting", menuName = "Abilities/Targeting/Delayed Click", order = 1)]
    public class DelayedClickTargeting : TargetingStrategy
    {
        // configs
        [SerializeField] Texture2D cursorTexture;
        [SerializeField] Vector2 cursorHotspot;
        [SerializeField] private float areaEffectRadius = 5f;
        [SerializeField] private GameObject areaEffectIndicator;
        [SerializeField] private LayerMask layerMask;

        // state
        GameObject indicatorInstance;

        public override void StartTargeting(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.StartCoroutine(Targeting(data, playerController, finished));
        }

        private IEnumerator Targeting(AbilityData data, PlayerController playerController, Action finished)
        {
            playerController.enabled = false;

            if (indicatorInstance == null)
            {
                indicatorInstance = GameObject.Instantiate(areaEffectIndicator);
            }
            else
            {
                indicatorInstance.SetActive(true);
            }

            indicatorInstance.transform.localScale = new Vector3(areaEffectRadius * 2, 1, areaEffectRadius * 2);

            while (true)
            {
                // Run every frame;
                Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
                RaycastHit raycastHit;
                if (Physics.Raycast(PlayerController.GetMouseRay(), out raycastHit, 1000, layerMask))
                {
                    indicatorInstance.transform.position = raycastHit.point;
                    if (Input.GetMouseButtonDown(0))
                    {
                        yield return new WaitWhile(() => Input.GetMouseButton(0)); // Don't proceed until mouse button has been released.
                        indicatorInstance.SetActive(false);
                        playerController.enabled = true;
                        data.SetTargetedPoint(raycastHit.point);
                        data.SetTargets(GetGameObjectsinRadius(raycastHit.point));
                        finished();
                        yield break;
                    }
                }
                yield return null;
            }            
        }

        private IEnumerable<GameObject> GetGameObjectsinRadius(Vector3 point)
        {
            RaycastHit[] hits = Physics.SphereCastAll(point, areaEffectRadius, Vector3.up, 0);

            foreach (var hit in hits)
            {
                yield return hit.collider.gameObject;
            }
        }
    }
}