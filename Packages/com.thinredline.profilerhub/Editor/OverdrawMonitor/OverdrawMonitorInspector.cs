using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ThinRL.ProfilerHub.OverdrawMonitor;

namespace ThinRL.ProfilerHub.Editor.OverdrawMonitor
{
    [CustomEditor(typeof(ThinRL.ProfilerHub.OverdrawMonitor.OverdrawMonitor), true)]
    public class OverdrawMonitorInspector : UnityEditor.Editor
    {
		bool m_ShowTextureToggle = false;
		GUIStyle m_TextureStyle = null;
        public override void OnInspectorGUI()
		{
			if (null == m_TextureStyle)
			{
				m_TextureStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
				m_TextureStyle.alignment = TextAnchor.MiddleCenter;
			}			

			DrawDefaultInspector();
			
			ThinRL.ProfilerHub.OverdrawMonitor.OverdrawMonitor overdrawMonitor = this.target as ThinRL.ProfilerHub.OverdrawMonitor.OverdrawMonitor;
			string toggleText0 = "‘§¿¿Overdraw";
			string toggleText1 = (overdrawMonitor.overdrawTexture == null) ? "‘§¿¿Overdraw(Œﬁ)" : $"‘§¿¿Overdraw({overdrawMonitor.overdrawTexture.width}x{overdrawMonitor.overdrawTexture.height})";
			m_ShowTextureToggle = GUILayout.Toggle(m_ShowTextureToggle, m_ShowTextureToggle ? toggleText1 : toggleText0);

			if (m_ShowTextureToggle)
			{
				if (overdrawMonitor.overdrawTexture != null)
				{
					GUILayout.Label(overdrawMonitor.overdrawTexture, m_TextureStyle);
				}
			}
		}
	}
}
