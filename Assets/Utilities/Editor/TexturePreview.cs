using System;
using UnityEditor;
using UnityEngine;
using Utilities.Extensions;

namespace Utilities.Editor {
	[Serializable]
	public class TexturePreview {
		public enum Anchor {
			Left,
			Right,
			Center
		}
		public float Width => 100 * zoom;
		public float Height => 100 * zoom;
		public Texture2D texture;
		private float zoom;
		private Color backgroundColor;
		public string label;
		public Color Color => backgroundColor;
		public Anchor anchor;
		public event Action<Vector2Int> Clicked;
		public event Action<Vector2> Scrolled;

		public TexturePreview(Anchor anchor, string label = null) {
			texture = null;
			this.anchor = anchor;
			this.label = label;
			backgroundColor = Color.clear;
			zoom = 1;
		}
	
		public TexturePreview(ref Texture2D texture, float zoom, Color backgroundColor, Anchor anchor = Anchor.Left, string label = null) {
			this.texture = texture;
			this.backgroundColor = backgroundColor;
			this.zoom = zoom;
			this.anchor = anchor;
			this.label = label;
		}

		public TexturePreview(ref Texture2D texture, Anchor anchor = Anchor.Left, string label = null) {
			this.texture = texture;
			backgroundColor = Color.clear;
			this.anchor = anchor;
			this.label = label;
			zoom = 1;
		}
		public TexturePreview(Sprite sprite, Anchor anchor = Anchor.Left, string label = null) {
			texture = EditorUtility.CopyTexture(sprite);
			backgroundColor = Color.clear;
			this.anchor = anchor;
			this.label = label;
			zoom = 1;
		}

		public void ChangeTexture(ref Texture2D texture) {
			this.texture = texture;
		}

		public void ChangeTexture(Sprite sprite) {
			texture = EditorUtility.CopyTexture(sprite);
		}

		public void ChangeZoom(float zoom) {
			this.zoom = zoom;
		}

		public void ChangeBackgroundColor(Color color) {
			backgroundColor = color;
		}

		public void ChangeLabel(string label) {
			this.label = label;
		}

		public void OnClicked(Vector2Int pos) {
			Clicked?.Invoke(pos);
		}

		public void OnScrolled(Vector2 delta) {
			Scrolled?.Invoke(delta);
		}

	}
	
	[CustomPropertyDrawer(typeof(TexturePreview))]
	public class TexturePreviewDrawer : PropertyDrawer {
		private const float SideBorder = 16f;
		private const float LabelHeight = 16f;
		private const float Space = 8f;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginChangeCheck();
			var preview = fieldInfo.GetValue(property.serializedObject.targetObject) as TexturePreview;
			
			bool hasLabel = !string.IsNullOrEmpty(preview.label);
			

			if (preview.texture != null) {
				Rect rect = hasLabel ? EditorGUILayout.GetControlRect(false, preview.Height + LabelHeight*3) 
					: EditorGUILayout.GetControlRect(false, preview.Height + LabelHeight);

				float additionalSpace = 0;
			
				float previewRectX = GetRectX(rect, preview);
				if (hasLabel) {
					string newLabel = preview.label;
					var style = new GUIStyle {fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft};
				
					var labelRect = new Rect(previewRectX, rect.y, rect.width, LabelHeight);
					EditorGUI.LabelField(labelRect, newLabel, style);
					additionalSpace += (LabelHeight*2);
				}
			
				var previewRect = new Rect(previewRectX, rect.position.y + additionalSpace , preview.Width, preview.Height);
				if (previewRect.Contains(Event.current.mousePosition)) {
					if (Event.current.type == EventType.MouseDown) {
						var mouseToRectPosition = new Vector2
						{
							x = Event.current.mousePosition.x - previewRect.x,
							y = Event.current.mousePosition.y - previewRect.y
						};
						var remappedPosition = new Vector2Int
						{
							x = Mathf.FloorToInt(mouseToRectPosition.x.Remap(0, preview.Width, 0, preview.texture.width)),
							y = Mathf.FloorToInt(mouseToRectPosition.y.Remap(0, preview.Height, preview.texture.height, 0))
						};
						preview.OnClicked(remappedPosition);
						Event.current.Use();
					}
					if (Event.current.type == EventType.ScrollWheel) {
						preview.OnScrolled(Event.current.delta);
						Event.current.Use();
					}
				}

				EditorUtility.DrawBorders(previewRect, 2, Color.gray);
				Color guiColor = GUI.color;
				GUI.color = preview.Color;
				EditorGUI.DrawTextureTransparent(previewRect, preview.texture);
				GUI.color = guiColor;
			}
			EditorGUI.EndChangeCheck();
		}
		
		private float GetRectX(Rect rect, TexturePreview preview) {
			switch (preview.anchor) {
				case TexturePreview.Anchor.Left: 
					return rect.x + SideBorder;
			
				case TexturePreview.Anchor.Right:
					return rect.xMax - SideBorder - preview.Width;
			
				case TexturePreview.Anchor.Center:
					return rect.center.x - (preview.Width/2);
			
				default:
					return rect.x + SideBorder;
			}
		}

		public override bool CanCacheInspectorGUI(SerializedProperty property)
		{
			return false;
		}
	}
}