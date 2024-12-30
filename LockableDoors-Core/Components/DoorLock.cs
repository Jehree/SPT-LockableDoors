using EFT.Interactive;
using InteractableInteractionsAPI.Models;
using LockableDoors.Fika;
using LockableDoors.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LockableDoors.Components
{
    internal class DoorLock : MonoBehaviour
    {
        public Door Door { get; private set; }
        public NavMeshObstacle Obstacle { get; private set; }
        private BoxCollider _collider;
        private GameObject _visualizer = null;
        public Vector3 ObstacleSize
        {
            get
            {
                return new Vector3(
                    Settings.NavMeshObstacleXSizeMultiplier.Value,
                    Settings.NavMeshObstacleYSizeMultiplier.Value,
                    Settings.NavMeshObstacleZSizeMultiplier.Value
                );
            }
        }

        private void Awake()
        {
            Obstacle = gameObject.GetOrAddComponent<NavMeshObstacle>();
            _collider = gameObject.GetComponent<BoxCollider>();
            Obstacle.shape = NavMeshObstacleShape.Box;
            Obstacle.center = _collider.center;
            Obstacle.size = ObstacleSize;
            Obstacle.carving = true;
            Obstacle.carveOnlyStationary = false;
            Door = gameObject.GetComponent<Door>();
            ModSession.AddInitializedDoor(Door);
            Door.OnDoorStateChanged += OnDoorStateChanged;
        }

        public virtual void OnDoorStateChanged(WorldInteractiveObject subscribee, EDoorState previous, EDoorState next)
        {
            Obstacle.enabled = next == EDoorState.Locked;
        }

        public void AddLockInteractionsToActionList(List<ActionsTypesClass> vanillaActionList)
        {

            if (Door.DoorState == EDoorState.Locked)
            {
                vanillaActionList.Insert(0, GetUnlockInteraction().ActionsTypesClass);
                return;
            }

            if (Door.DoorState == EDoorState.Shut)
            {
                vanillaActionList.Insert(1, GetLockInteraction().ActionsTypesClass);
                return;
            }

            if (Door.DoorState == EDoorState.Open)
            {
                vanillaActionList.Insert(1, Interaction.GetDisabledInteraction("Lock").ActionsTypesClass);
                return;
            }
        }

        public static void AddUninitializedLockInteractionsToActionList(List<ActionsTypesClass> vanillaActionList, Door door)
        {

            if (door.DoorState == EDoorState.Shut)
            {
                vanillaActionList.Insert(1, DoorLock.GetUninitializedLockInteration(door.Id).ActionsTypesClass);
                return;
            }

            if (door.DoorState == EDoorState.Open)
            {
                vanillaActionList.Insert(1, Interaction.GetDisabledInteraction("Lock").ActionsTypesClass);
                return;
            }
        }

        public void Lock()
        {
            Obstacle.enabled = true;
            Door.DoorState = EDoorState.Locked;
            Obstacle.size = ObstacleSize;
        }

        private Interaction GetLockInteraction()
        {
            string id = Door.Id;
            return new Interaction
            (
                "Lock",
                () => { return Settings.GetLockedDoorLimit() <= ModSession.Instance.LockedDoorsCount; },
                () =>
                {
                    DoorLock doorLock = GetLock(id);
                    doorLock.Lock();
                    FikaInterface.SendDoorLockedStatePacket(id, true);
                    NotificationManagerClass.DisplayMessageNotification($"Locked! {ModSession.Instance.LockedDoorsCount}/{GetDoorLimitText()} doors locked");

                    if (Settings.VisualizerEnabled.Value)
                    {
                        doorLock.EnabledVisualizer();
                    }
                }
            );
        }

        public static Interaction GetUninitializedLockInteration(string doorId)
        {
            return new Interaction
            (
                "Lock",
                () => { return Settings.GetLockedDoorLimit() <= ModSession.Instance.LockedDoorsCount; },
                () =>
                {
                    DoorLock doorLock = ModSession.GetDoor(doorId).gameObject.AddComponent<DoorLock>();
                    doorLock.Lock();
                    FikaInterface.SendDoorLockedStatePacket(doorId, true);
                    NotificationManagerClass.DisplayMessageNotification($"Locked! {ModSession.Instance.LockedDoorsCount}/{GetDoorLimitText()} doors locked");

                    if (Settings.VisualizerEnabled.Value)
                    {
                        doorLock.EnabledVisualizer();
                    }
                }
            );
        }

        public void Unlock()
        {
            Obstacle.enabled = false;
            Door.DoorState= EDoorState.Shut;
        }

        private Interaction GetUnlockInteraction()
        {
            string id = Door.Id;
            return new Interaction
            (
                "Unlock",
                false,
                () =>
                {
                    DoorLock doorLock = GetLock(id);
                    doorLock.Unlock();
                    FikaInterface.SendDoorLockedStatePacket(id, false);

                    NotificationManagerClass.DisplayMessageNotification($"Unlocked! {ModSession.Instance.LockedDoorsCount}/{GetDoorLimitText()} doors locked");

                    if (Settings.VisualizerEnabled.Value)
                    {
                        doorLock.DisableVisualizer();
                    }
                }
            );
        }

        public static DoorLock GetLock(Door door)
        {
            return door.gameObject.GetComponent<DoorLock>();
        }

        public static DoorLock GetLock(string doorId)
        {
            return GetLock(ModSession.GetDoor(doorId));
        }

        public void EnabledVisualizer()
        {
            if (_visualizer == null)
            {
                _visualizer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Renderer renderer = _visualizer.GetComponent<Renderer>();
                Destroy(_visualizer.GetComponent<BoxCollider>());
                renderer.material.color = Color.magenta;
            }
            _visualizer.SetActive(true);
            _visualizer.transform.position = _collider.transform.TransformPoint(_collider.center);
            _visualizer.transform.rotation = _collider.transform.rotation;
            _visualizer.transform.localScale = ObstacleSize;
        }

        public void DisableVisualizer()
        {
            if (_visualizer == null) return;
            _visualizer.SetActive(false);
        }

        private static string GetDoorLimitText()
        {
            if (Settings.LockedDoorLimitsEnabled.Value)
            {
                return Settings.GetLockedDoorLimit().ToString();
            }
            else
            {
                return "∞";
            }
        }
    }
}
