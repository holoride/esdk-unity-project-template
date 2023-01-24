// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDK.FadeToBackground.BuildInRenderPipeline
{
    using System;
    using UnityEngine.Events;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// The behavior on every <see cref="OnEnable"/> call when the <see cref="FadeToBackgroundManager"/> gets active.
    /// </summary>
    public enum BehaviorOnEnable
    {
        /// <summary>
        /// Plays the appear animation to reveal all FadeToBackground materials.
        /// </summary>
        PlayAppearAnimation,
        
        /// <summary>
        /// FadeToBackground materials are invisible and are revealed when the <see cref="PlayAppearAnimation"/>
        /// function is called.
        /// </summary>
        Invisible,
        
        /// <summary>
        /// FadeToBackground materials are visible.
        /// </summary>
        Visible,
        
        /// <summary>
        /// Plays the appear animation after the ElasticSceneGenerator has finished its generation process.
        /// </summary>
        AppearAfterElasticSceneGenerationFinished,
    }
    
    /// <summary>
    /// The kind of animation to reveal or hide all FadeToBackground materials.
    /// </summary>
    public enum AnimationType
    {
        /// <summary>
        /// The render distance starts narrow and gets wider.
        /// </summary>
        Swipe,
        
        /// <summary>
        /// The render opacity starts transparent and gets more opaque.
        /// </summary>
        Fade,
    }

    /// <summary>
    /// The component to control all FadeToBackground materials and their fadeout in the distance. Furthermore, it
    /// handles the appear and disappear animations:
    /// <ul>
    /// <li>Call <see cref="PlayAppearAnimation"/> to reveal the materials.</li>
    /// <li>Call <see cref="PlayDisappearAnimation"/> to hide the materials.</li>
    /// </ul>
    /// </summary>
    [ExecuteInEditMode]
    public class FadeToBackgroundManager : MonoBehaviour
    {
        [Header("Dependencies")]
        
        [Tooltip("The camera to draw the fading materials.")]
        [SerializeField]
        private Camera camera;
                
        [Header("View Distance Cutoff (optional)")]
        
        [Tooltip("The ElasticSceneGenerator to cutoff the maximum view distance depending on its bounds. Leave empty to disable cutoff.")]
        [SerializeField]
        private ElasticSceneGenerator elasticSceneGenerator;
        
        [Tooltip("The cutoff distance to the bounds of the ElasticSceneGenerator.")]
        [SerializeField]
        private float cutoffDistanceToBounds = 50;
        
        [Tooltip("The time in seconds it takes to adapt to new bounds after an ElasticSceneGenerator update.")]
        [SerializeField]
        private float cutoffApplySeconds = 1.0f;
        
        [Header("Maximum Distances and Opacity")]
        
        [Tooltip("The maximum opacity of any FadeToBackground material in any state.")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float maxOpacity = 1.0f;
        
        [Tooltip("The maximum distance at which FadeToBackground materials start fading out.")]
        [SerializeField]
        [Min(0)]
        private float maxOpaqueDistance = 100;
        
        [Tooltip("The maximum distance at which FadeToBackground materials lose sight.")]
        [SerializeField]
        [Min(0)]
        private float maxTransparentDistance = 200;

        [Header("Appear / Disappear Animation")]
        
        [Tooltip("The behavior on every OnEnable call when the FadeToBackgroundManager gets active.")]
        [SerializeField]
        private BehaviorOnEnable behaviorOnEnable;
        
        [Tooltip("The kind of animation to reveal or hide all FadeToBackground materials.")]
        [SerializeField]
        private AnimationType animationType;
        
        [Tooltip("Maps the current animation time to a ratio of the (dis)appear effect. The curve is played backwards during the disappear animation. While the time can be scaled arbitrarily, values are clamped between 0 and 1 when evaluated. Animations will always start at 0 time (or less, if negative keyframes times are set).")]
        [SerializeField]
        private AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(3, 1));

        [Space]
        
        [Tooltip("Gets invoked when the appear animation ends.")]
        [SerializeField]
        private UnityEvent onAppearAnimationFinished;
            
        [Tooltip("Gets invoked when the disappear animation ends.")]
        [SerializeField]
        private UnityEvent onDisappearAnimationFinished;
        
        [Tooltip("Gets invoked when a uninterruptibled disappear animation ends.")]
        [SerializeField]
        private UnityEvent onUninterruptibleDisappearAnimationFinished;

        // Shader parameter IDs
        private int cameraPositionID;
        private int opaqueDistanceSquaredID;
        private int transparentDistanceSquaredID;
        private int opacityID;

        // Required for clearing the alpha channel
        private bool alphaClearRequired;
        private CommandBuffer commandBuffer;
        
        // Animation state variables
        private bool isPlayingForward = true;
        private float currentAnimationSecond = 0;
        private float currentOpaqueDistance;
        private float currentTransparentDistance;
        private float currentOpacity;
        
        // ElasticScene bound cutoff state variables
        private float distanceAdjustmentVelocity;
        private float targetCutoffDistanceRatio = 1.0f;
        private float currentCutoffDistanceRatio = 1.0f;
        private bool isPlayingIsPlayingUninterruptibleDisappearAnimation = false;

        /// <summary>
        /// The camera to draw the fading materials.
        /// </summary>
        public Camera Camera
        {
            get => this.camera;
            set
            {
                if (this.camera != null && this.commandBuffer != null)
                {
                    this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
                }

                this.camera = value;

                if (this.camera != null && this.commandBuffer != null)
                {
                    this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
                    this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
                }
            }
        }
        
        /// <summary>
        /// The maximum opacity of any FadeToBackground material in any state.
        /// </summary>
        public float MaxOpacity
        {
            get => this.maxOpacity;
            set => this.maxOpacity = value;
        }

        /// <summary>
        /// The maximum distance at which FadeToBackground materials start fading out.
        /// </summary>
        public float MaxOpaqueDistance
        {
            get => this.maxOpaqueDistance;
            set => this.maxOpaqueDistance = value;
        }

        /// <summary>
        /// The maximum distance at which FadeToBackground materials lose sight.
        /// </summary>
        public float MaxTransparentDistance
        {
            get => this.maxTransparentDistance;
            set => this.maxTransparentDistance = value;
        }

        /// <summary>
        /// Maps the current animation time to a ratio of the (dis)appear effect. The curve is played backwards during
        /// the disappear animation. While the time can be scaled arbitrarily, values are clamped between 0 and 1 when
        /// evaluated. Animations will always start at 0 time (or less, if negative keyframes times are set). 
        /// </summary>
        public AnimationCurve AnimationCurve
        {
            get => this.animationCurve;
            set => this.animationCurve = value;
        }
        
        /// <summary>
        /// The kind of animation to reveal or hide all FadeToBackground materials.
        /// </summary>
        public AnimationType AnimationType
        {
            get => this.animationType;
            set => this.animationType = value;
        }
        
        /// <summary>
        /// The behavior on every <see cref="OnEnable"/> call when the FadeToBackgroundManager gets active.
        /// </summary>
        public BehaviorOnEnable BehaviorOnEnable
        {
            get => this.behaviorOnEnable;
            set => this.behaviorOnEnable = value;
        }

        /// <summary>
        /// States whether and uninterruptible disappear animation is currently playing.
        /// </summary>
        public bool IsPlayingUninterruptibleDisappearAnimation => isPlayingIsPlayingUninterruptibleDisappearAnimation;

        /// <summary>
        /// Gets invoked when the appear animation ends.
        /// </summary>
        public UnityEvent OnAppearAnimationFinished => this.onAppearAnimationFinished;
        
        /// <summary>
        /// Gets invoked when the disappear animation ends.
        /// </summary>
        public UnityEvent OnDisappearAnimationFinished => this.onDisappearAnimationFinished;
        
        /// <summary>
        /// Gets invoked when a uninterruptibled disappear animation ends.
        /// </summary>
        public UnityEvent OnUninterruptibleDisappearAnimationFinished => this.onUninterruptibleDisappearAnimationFinished;

        /// <summary>
        /// States whether an animation is currently playing.
        /// </summary>
        public bool IsAnimating => (this.isPlayingForward && this.CurrentAnimationSecond != this.AnimationEndSecond) || (!this.isPlayingForward && this.CurrentAnimationSecond != this.AnimationBeginSecond);

        /// <summary>
        /// States whether the animation is playing forward or backwards. 
        /// </summary>
        public bool IsPlayingForward => this.isPlayingForward;
        
        /// <summary>
        /// The current animation time measured in seconds.
        /// </summary>
        public float CurrentAnimationSecond => this.currentAnimationSecond;
        
        /// <summary>
        /// Usually returns 0. If keyframes with negative times are set, the time of the left-most keyframe is returned.
        /// Returned values are measured in seconds.
        /// </summary>
        public float AnimationBeginSecond => Mathf.Min(0, this.AnimationCurve.keys[0].time);
        
        /// <summary>
        /// The time of the right-most keyframe measured in seconds.
        /// </summary>
        public float AnimationEndSecond => this.AnimationCurve.keys[this.AnimationCurve.keys.Length - 1].time;
        
        /// <summary>
        /// Returns the total duration of the animation measured in seconds.
        /// </summary>
        public float AnimationDuration => this.AnimationEndSecond - this.AnimationBeginSecond;
        
                
        /// <summary>
        /// The ElasticSceneGenerator to cutoff the maximum view distance. Leave empty if no bounds cutoff should be
        /// applied.
        /// </summary>
        public ElasticSceneGenerator ElasticSceneGenerator
        {
            get => this.elasticSceneGenerator;
            set => this.elasticSceneGenerator = value;
        }

        /// <summary>
        /// The time in seconds it takes to adapt to new bounds after an ElasticSceneGenerator update.
        /// </summary>
        public float CutoffApplySeconds
        {
            get => this.cutoffApplySeconds;
            set => this.cutoffApplySeconds = value;
        }

        /// <summary>
        /// The cutoff distance to the bounds of the ElasticSceneGenerator.
        /// </summary>
        public float CutoffDistanceToBounds
        {
            get => this.cutoffDistanceToBounds;
            set => this.cutoffDistanceToBounds = value;
        }

        /// <summary>
        /// Plays the animation to make all FadeToBackground materials appear due to the chosen animation curve. Has no
        /// effect if the animation is currently playing forward or if <see cref="CurrentAnimationSecond"/> is at the end.
        /// </summary>
        [ContextMenu("PlayAppearAnimation")]
        public void PlayAppearAnimation()
        {
            if (!this.isPlayingIsPlayingUninterruptibleDisappearAnimation)
            {
                this.isPlayingForward = true;
            }
        }

        /// <summary>
        /// Plays the animation to make all FadeToBackground materials disappear due to the chosen animation
        /// curve that's played backwards. Has no effect if the animation is currently playing backward or if
        /// <see cref="CurrentAnimationSecond"/> is at the beginning.
        /// </summary>
        [ContextMenu("PlayDisappearAnimation")]
        public void PlayDisappearAnimation()
        {
            this.isPlayingForward = false;
        }
        
        /// <summary>
        /// Plays the animation to make all FadeToBackground materials disappear due to the chosen animation
        /// curve that's played backwards. Has no effect if the animation is currently playing backward or if
        /// <see cref="CurrentAnimationSecond"/> is at the beginning.
        /// </summary>
        [ContextMenu("PlayUninterruptibleDisappearAnimation")]
        public void PlayUninterruptibleDisappearAnimation()
        {
            this.isPlayingIsPlayingUninterruptibleDisappearAnimation = true;
            
            if (this.currentAnimationSecond == this.AnimationBeginSecond)
            {
                this.OnUninterruptibleDisappearAnimationFinished.Invoke();
                this.isPlayingIsPlayingUninterruptibleDisappearAnimation = false;
            }
            else
            {
                this.PlayDisappearAnimation();
            }
        }

        private void Awake()
        {
            this.alphaClearRequired =
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;

            this.opacityID = Shader.PropertyToID("_FadeToBackground_Opacity");
            this.opaqueDistanceSquaredID = Shader.PropertyToID("_FadeToBackground_OpaqueDistanceSquared");
            this.transparentDistanceSquaredID = Shader.PropertyToID("_FadeToBackground_TransparentDistanceSquared");
            this.cameraPositionID = Shader.PropertyToID("_FadeToBackground_CameraPosition");

            this.MaxOpaqueDistance = this.maxOpaqueDistance;
            this.MaxTransparentDistance = this.maxTransparentDistance;
            
            this.DisableFading();

            if (this.alphaClearRequired)
            {
                this.commandBuffer = new CommandBuffer();
                this.commandBuffer.name = "Clear Alpha";
                this.commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                this.commandBuffer.ClearRenderTarget(false, true, Color.clear);
            }
                            
            if (this.Camera != null && this.commandBuffer != null)
            {
                this.Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
            }
        }

        private void OnEnable()
        {
            if (RenderSettings.skybox == null || RenderSettings.skybox.GetTag("FadeToBackgroundRole", false) != "Background")
            {
                Debug.LogError("The current Skybox Material is not tagged as \"FadeToBackgroundRole\"=\"Background\".");
            }

            switch (this.BehaviorOnEnable)
            {
                case BehaviorOnEnable.PlayAppearAnimation:
                    this.currentAnimationSecond = this.AnimationBeginSecond;
                    this.isPlayingForward = true;
                    break;

                case BehaviorOnEnable.Invisible:
                    this.currentAnimationSecond = this.AnimationBeginSecond;
                    this.isPlayingForward = false;
                    break;

                case BehaviorOnEnable.Visible:
                    this.currentAnimationSecond = this.AnimationEndSecond;
                    this.isPlayingForward = true;
                    break;

                case BehaviorOnEnable.AppearAfterElasticSceneGenerationFinished:
                    this.currentAnimationSecond = this.AnimationBeginSecond;
                    this.isPlayingForward = false;
                    if (this.ElasticSceneGenerator != null)
                    {
                        this.ElasticSceneGenerator.OnGenerationFinished.AddListener(PlayAppearAnimationOnElasticSceneGenerationFinished);
                    }
                    else
                    {
                        Debug.LogError("No ElasticSceneGenerator has been selected to cutoff the Fade-To-Background distance. Please add one to the corresponding field or select a different BehaviorOnEnable.");
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            this.SeekAnimation(this.currentAnimationSecond);
            this.ComputeBoundCutoff();
            this.UpdateShaderValues();
        }

        private void PlayAppearAnimationOnElasticSceneGenerationFinished(GenerationContext context)
        {
            this.PlayAppearAnimation();
        }
        
        private void OnDisable()
        {
            this.isPlayingForward = false;
            this.SeekAnimation(this.AnimationBeginSecond);
            this.DisableFading();
            
            if (this.ElasticSceneGenerator != null)
            {
                this.ElasticSceneGenerator.OnGenerationFinished.RemoveListener(PlayAppearAnimationOnElasticSceneGenerationFinished);
            }
        }

        private void OnValidate()
        {
            if (this.Camera == null)
            {
                this.camera = Camera.main;
            }
            
            if (this.Camera != null && this.commandBuffer != null)
            {
                this.Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
                this.Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
            }

            this.MaxOpacity = this.maxOpacity;
            this.MaxOpaqueDistance = this.maxOpaqueDistance;
            this.MaxTransparentDistance = this.maxTransparentDistance;
            this.SeekAnimation(this.currentAnimationSecond);
            this.ComputeBoundCutoff();
            this.UpdateShaderValues();
        }

        private void OnDestroy()
        {
            if (this.Camera != null && this.commandBuffer != null)
            {
                this.Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, this.commandBuffer);
            }

            this.commandBuffer?.Release();
        }

        private void Update()
        {
            if (this.Camera == null)
            {
                return;
            }

            if (this.IsAnimating)
            {
                this.SeekAnimation(this.isPlayingForward
                    ? Mathf.Min(this.AnimationEndSecond, this.CurrentAnimationSecond + Time.deltaTime)
                    : Mathf.Max(this.AnimationBeginSecond, this.CurrentAnimationSecond - Time.deltaTime));
                
                if (this.currentAnimationSecond == this.AnimationEndSecond)
                {
                    this.OnAppearAnimationFinished.Invoke();
                }
                    
                if (this.currentAnimationSecond == this.AnimationBeginSecond)
                {
                    if (this.isPlayingIsPlayingUninterruptibleDisappearAnimation)
                    {
                        this.OnUninterruptibleDisappearAnimationFinished.Invoke();
                        this.isPlayingIsPlayingUninterruptibleDisappearAnimation = false;
                    }
                    else
                    {
                        this.OnDisappearAnimationFinished.Invoke();
                    }
                }
            }

            this.ComputeBoundCutoff();
            this.UpdateShaderValues();
        }

        private void SeekAnimation(float time)
        {
            this.currentAnimationSecond = time;

            switch (AnimationType)
            {
                case AnimationType.Swipe:
                    float animationDistance = Mathf.Clamp01(this.AnimationCurve.Evaluate(this.CurrentAnimationSecond));
                    this.currentOpaqueDistance = this.maxOpaqueDistance * animationDistance;
                    this.currentTransparentDistance = this.maxTransparentDistance * animationDistance;
                    this.currentOpacity = this.maxOpacity;
                    break;

                case AnimationType.Fade:
                    float animationAlpha = Mathf.Clamp01(this.AnimationCurve.Evaluate(this.CurrentAnimationSecond));
                    this.currentOpaqueDistance = this.maxOpaqueDistance;
                    this.currentTransparentDistance = this.maxTransparentDistance;
                    this.currentOpacity = this.maxOpacity * animationAlpha;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ComputeBoundCutoff()
        {
            var cameraPosition = GlobalPosition.FromShifted((this.Camera == null) ? Vector3.zero : this.camera.transform.position);
            
            if (this.ElasticSceneGenerator != null)
            {
                if (this.ElasticSceneGenerator.TerrainGenerationState.FinalGenerationContext != null)
                {
                    var bounds = this.ElasticSceneGenerator.TerrainGenerationState.FinalGenerationContext.InnerBounds;

                    double minBorderDistance = cameraPosition.X - bounds.XMin;
                    minBorderDistance = Math.Min(minBorderDistance, bounds.XMax - cameraPosition.X);
                    minBorderDistance = Math.Min(minBorderDistance, cameraPosition.Z - bounds.ZMin);
                    minBorderDistance = Math.Min(minBorderDistance, bounds.ZMax - cameraPosition.Z);
                    minBorderDistance -= this.cutoffDistanceToBounds;
                    
                    this.targetCutoffDistanceRatio = Mathf.Clamp01((float) minBorderDistance / this.maxTransparentDistance);
                    this.currentCutoffDistanceRatio = Mathf.SmoothDamp(
                        this.currentCutoffDistanceRatio,
                        this.targetCutoffDistanceRatio,
                        ref this.distanceAdjustmentVelocity,
                        this.cutoffApplySeconds);
                }
            }
        }

        private void UpdateShaderValues()
        {
            float resultTransparentDistance = this.currentCutoffDistanceRatio * this.currentTransparentDistance;
            float resultOpaqueDistance = Mathf.Min(resultTransparentDistance, this.currentCutoffDistanceRatio * this.currentOpaqueDistance);

            Shader.SetGlobalVector(this.cameraPositionID, this.camera.transform.position);
            Shader.SetGlobalFloat(this.opaqueDistanceSquaredID, resultOpaqueDistance * resultOpaqueDistance);
            Shader.SetGlobalFloat(this.transparentDistanceSquaredID, resultTransparentDistance * resultTransparentDistance);
            Shader.SetGlobalFloat(this.opacityID, this.currentOpacity);
        }

        private void DisableFading()
        {
            Shader.SetGlobalFloat(this.opaqueDistanceSquaredID, 0.001f);
            Shader.SetGlobalFloat(this.transparentDistanceSquaredID, 0);
            Shader.SetGlobalFloat(this.opacityID, 1);
        }
    }
}