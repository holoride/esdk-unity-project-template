// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDK.FadeToBackground.BuildInRenderPipeline.Editor
{
    using UnityEditor;
    using UnityEngine;
    
    [CustomEditor(typeof(FadeToBackgroundManager))]
    internal class FadeToBackgroundManagerEditor : Editor
    {
        private FadeToBackgroundManager Target => (FadeToBackgroundManager)this.target;
        
        public override void OnInspectorGUI()
        {
            this.DrawDefaultInspector();
            
            using (new EditorGUI.DisabledScope(!Application.isPlaying || !this.Target.enabled))
            {
                using (new EditorGUI.DisabledScope(this.Target.IsPlayingUninterruptibleDisappearAnimation))
                {
                    if (this.Target.IsPlayingForward)
                    {
                        if (GUILayout.Button("Play Disappear Animation"))
                        {
                            this.Target.PlayDisappearAnimation();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Play Appear Animation"))
                        {
                            this.Target.PlayAppearAnimation();
                        }
                    }
                }
                
                if (GUILayout.Button("Play Uninterruptible Disappear Animation"))
                {
                    this.Target.PlayUninterruptibleDisappearAnimation();
                }
            }
        }
    }
}