// SPDX-FileCopyrightText: 2026 milkydelta
// SPDX-License-Identifier: LicenseRef-AllRightsReserved
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets.Cinematography;
using Warudo.Plugins.Core.Assets.Utility;
using TapSuite.Comms;
using Warudo.Plugins.Core.Assets;
using UnityEngine;
using UniVRM10; //For Matrix4x4.ExtractPos

namespace TapSuite
{

    [AssetType(Id = "8dbd9963-6341-4674-bbf2-6af11aa7c20c", Title = "VR Camera Sync", Category ="Cinematography")]
    public class TapSuiteAsset : Asset
    {
        // I've made these Hidden because the path isn't yet configurable
        // in OAT, so having these here will just increase risk of breakage.
        [DataInput]
        [Hidden]
        public string Path = "uk.lum.vrnyan.cameradata";

        [Trigger]
        [Hidden]
        public void UpdatePath()
        {
            PathChanged(Path);
        }

        [DataInput]
        public CameraAsset Camera;

        [DataInput]
        [Label("Clip Target Location")]
        public AnchorAsset Clip;

        [DataInput]
        [Label("VR Playspace Root")]
        public GameObjectAsset PlayspaceRoot;

        [DataInput]
        public bool Enabled = false;

        [DataInput]
        public bool ModLogEnabled = true;

        [DataInput]
        [Hidden]
        public bool ModLogSpam = false;

        private AbComms com = AbComms.New();

        private void PathChanged(string to)
        {
            ShouldUpdateActive = true;
            com.Close();
            if (to == null || to == "") { return; }
            if (to.Length > 30) { return; }
            com.Open(to);
        }

        private void LogChanged(bool from, bool to)
        {
            com.SetCfg(LIVnyan_cfg.LOG_ON, to);
        }

        private void LogSpamChanged(bool from, bool to)
        {
            com.SetCfg(LIVnyan_cfg.LOGSPM, to);
        }

        private void EnableChanged(bool from, bool to)
        {
            ShouldUpdateActive = true;
        }

        private void CamChanged(CameraAsset from, CameraAsset to)
        {
            ShouldUpdateActive = true;
        }

        private void UpdateActive()
        {
            bool value = com.isOpen && Camera != null && Enabled;
            SetActive(value);
            com.SetCfg(LIVnyan_cfg.CAM_ON, value);
            ShouldUpdateActive = false;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            PathChanged(Path); //also sets shouldupdateactive

            Watch<bool>("ModLogEnabled", LogChanged);
            Watch<bool>("ModLogSpam", LogSpamChanged);
            Watch<bool>("Enabled", EnableChanged);
            Watch<CameraAsset>("Camera", CamChanged);
        }

        private bool ShouldUpdateActive = false;

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            Vector3 camPos;
            Quaternion camRot;
            Vector3 clipPos;

            if (ShouldUpdateActive) { UpdateActive(); }

            if (Active)
            {
                com.SetCfg(LIVnyan_cfg.OAT_READCLIP, Clip != null);

                if (Camera != null)
                {
                    camPos = Camera.Transform.Position;
                    camRot = Camera.Transform.RotationQuaternion;

                    if (PlayspaceRoot != null)
                    {
                        Matrix4x4 camPosMatrix = PlayspaceRoot.GameObject.transform.worldToLocalMatrix * Camera.GameObject.transform.localToWorldMatrix;
                        camPos = camPosMatrix.ExtractPosition();
                        camRot = camPosMatrix.ExtractRotation();
                    }

                    com.WriteCamPos(camPos);
                    com.WriteCamRot(camRot);
                    com.WriteCamFov(Camera.FieldOfView);

                    com.WriteRes(Camera.Camera.pixelWidth, Camera.Camera.pixelHeight);
                }

                if (Clip != null)
                {
                    clipPos = Clip.EndOfFrameWorldPosition;

                    if (PlayspaceRoot != null)
                    {
                        clipPos = PlayspaceRoot.GameObject.transform.InverseTransformPoint(clipPos);
                    }

                    com.WriteClipPos(clipPos);
                }
            }
        }

        // commented 2026-09-13
        // public override void OnLateUpdate()
        // {
        //     base.OnLateUpdate();

        //     Vector3 camPos;
        //     Quaternion camRot;
        //     Vector3 clipPos;

        //     if (ShouldUpdateActive) { UpdateActive(); }

        //     if (Active)
        //     {
        //         com.SetCfg(LIVnyan_cfg.OAT_READCLIP, Clip != null);

        //         if (Camera != null)
        //         {
        //             com.WriteCamPos(Camera.Transform.Position);
        //             com.WriteCamRot(Camera.Transform.RotationQuaternion);
        //             com.WriteCamFov(Camera.FieldOfView);

        //             com.WriteRes(Camera.Camera.pixelWidth, Camera.Camera.pixelHeight);
        //         }

        //         if (Clip != null)
        //         {
        //             com.WriteClipPos(Clip.EndOfFrameWorldPosition);
        //         }
        //     }
        // }

        protected override void OnDestroy()
        {
            com.Close();
            base.OnDestroy();
        }
    }
}