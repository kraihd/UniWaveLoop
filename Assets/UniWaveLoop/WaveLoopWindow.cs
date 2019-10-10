using System;
using UnityEditor;
using UnityEngine;

namespace UniWaveLoop {
	public class WaveLoopWindow : ScriptableWizard {
		string path;
		WaveFile wave;

		[MenuItem("Assets/Open WaveLoop Editor", true)]
		static bool CanOpen() {
			var activeObject = Selection.activeObject as AudioClip;
			if (activeObject == null) { return false; }

			var path = AssetDatabase.GetAssetPath(activeObject);
			return path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
		}

		[MenuItem("Assets/Open WaveLoop Editor")]
		static void Open() {
			Open(Selection.activeObject as AudioClip);
		}

		public static void Open(AudioClip clip) {
			var wizard = DisplayWizard<WaveLoopWindow>("Wave Loop Editor", "Save");
			wizard.path = AssetDatabase.GetAssetPath(clip);
			wizard.wave = new WaveFile(wizard.path);
			wizard.minSize = new Vector2(480, 240);
		}

		protected override bool DrawWizardGUI() {
			var flag = base.DrawWizardGUI();

			EditorGUILayout.LabelField("Path: " + path);

			if (wave == null || !wave.IsValid) {
				return flag;
			}

			EditorGUILayout.BeginHorizontal();
			var loopPoint = wave.LoopPoint;
			uint newLoopPoint = (uint)EditorGUILayout.IntSlider("Loop Point", (int)loopPoint, 0, (int)wave.SampleCount - 1);
			flag |= (newLoopPoint == loopPoint);
			wave.LoopPoint = newLoopPoint;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Loop Point (sec): " + loopPoint / (float)wave.SamplingRate);

			return flag;
		}

		void OnWizardCreate() {
			if (wave != null) {
				wave.Publish(path);
				AudioImporter.GetAtPath(path).SaveAndReimport();
			}
		}

		void OnDestroy() {
			wave?.Dispose();
		}
	}
}
