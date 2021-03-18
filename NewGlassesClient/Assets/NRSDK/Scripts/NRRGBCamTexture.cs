/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using UnityEngine;

    public class RGBCameraTextureBase
    {
        public int Height
        {
            get
            {
                return NRRgbCamera.Resolution.height;
            }
        }

        public int Width
        {
            get
            {
                return NRRgbCamera.Resolution.width;
            }
        }

        private bool m_IsPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return m_IsPlaying;
            }
        }

        public bool DidUpdateThisFrame
        {
            get
            {
                return NRRgbCamera.HasFrame();
            }
        }

        public int FrameCount = 0;
        private bool m_IsInitilized = false;

        private void Initilize()
        {
            if (m_IsInitilized)
            {
                return;
            }

            OnCreated();
            NRRgbCamera.Regist(this);
            m_IsInitilized = true;
        }

        /// <summary>
        /// On texture created.
        /// </summary>
        protected virtual void OnCreated() { }

        public void Play()
        {
            if (m_IsPlaying)
            {
                return;
            }
            this.Initilize();
            NRKernalUpdater.Instance.OnUpdate += UpdateTexture;
            NRRgbCamera.Play();
            m_IsPlaying = true;
        }

        public void Pause()
        {
            if (!m_IsPlaying)
            {
                return;
            }
            NRKernalUpdater.Instance.OnUpdate -= UpdateTexture;
            m_IsPlaying = false;
        }

        private void UpdateTexture()
        {
            if (!DidUpdateThisFrame || !IsPlaying)
            {
                return;
            }

            RGBRawDataFrame rgbRawDataFrame = NRRgbCamera.GetRGBFrame();
            if (rgbRawDataFrame.data == null)
            {
                Debug.LogError("Get Rgb camera data faild...");
                return;
            }

            OnLoadRawTextureData(rgbRawDataFrame);
        }

        /// <summary>
        /// Load raw texture data.
        /// </summary>
        /// <param name="rgbRawDataFrame"></param>
        protected virtual void OnLoadRawTextureData(RGBRawDataFrame rgbRawDataFrame) { }

        public void Stop()
        {
            if (!m_IsInitilized)
            {
                return;
            }

            NRRgbCamera.UnRegist(this);
            NRRgbCamera.Stop();

            this.Pause();
            this.OnStopped();

            FrameCount = 0;
            m_IsPlaying = false;
            m_IsInitilized = false;
        }

        /// <summary>
        /// On texture stopped.
        /// </summary>
        protected virtual void OnStopped() { }
    }

    /// <summary>
    /// Create a rgb camera texture.
    /// </summary>
    public class NRRGBCamTexture : RGBCameraTextureBase
    {
        public Action<RGBTextureFrame> OnUpdate;
        public RGBTextureFrame CurrentFrame;
        private Texture2D m_Texture;

        public Texture2D GetTexture()
        {
            if (m_Texture == null)
            {
                m_Texture = CreateTex();
            }
            return m_Texture;
        }

        private Texture2D CreateTex()
        {
            return new Texture2D(Width, Height, TextureFormat.RGB24, false);
        }

        protected override void OnCreated()
        {
            if (m_Texture == null)
            {
                m_Texture = CreateTex();
            }
        }

        protected override void OnLoadRawTextureData(RGBRawDataFrame rgbRawDataFrame)
        {
            m_Texture.LoadRawTextureData(rgbRawDataFrame.data);
            m_Texture.Apply();

            CurrentFrame.timeStamp = rgbRawDataFrame.timeStamp;
            CurrentFrame.texture = m_Texture;
            FrameCount++;

            OnUpdate?.Invoke(CurrentFrame);
        }

        protected override void OnStopped()
        {
            GameObject.Destroy(m_Texture);
            m_Texture = null;
        }
    }
}
