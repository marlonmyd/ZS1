using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace AdvancedInspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Light), true)]
    public class LightEditor : InspectorEditor
    {
        private static Color disabledLightColor = new Color(0.5f, 0.45f, 0.2f, 0.5f);
        private static Color lightColor = new Color(0.95f, 0.95f, 0.5f, 0.5f);

        protected override void RefreshFields()
        {
            Type type = typeof(Light);
            SerializedObject so = new SerializedObject(targets);

            fields.Add(new InspectorField(type, Instances, type.GetProperty("type"), new HelpAttribute(new HelpAttribute.HelpDelegate(HelpLightType)),
                new DescriptorAttribute("Type", "The type of the light.", "http://docs.unity3d.com/ScriptReference/Light-type.html")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("range"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsPointOrSpot)),
                new DescriptorAttribute("Range", "The range of the light.", "http://docs.unity3d.com/ScriptReference/Light-range.html")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("spotAngle"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsSpot)),
                new DescriptorAttribute("Spot Angle", "The angle of the light's spotlight cone in degrees.", "http://docs.unity3d.com/ScriptReference/Light-spotAngle.html")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("color"), 
                new DescriptorAttribute("Color", "The color of the light.", "http://docs.unity3d.com/ScriptReference/Light-color.html")));
            fields.Add(new InspectorField(type, Instances, type.GetProperty("intensity"),
                new DescriptorAttribute("Intensity", "The Intensity of a light is multiplied with the Light color.", "http://docs.unity3d.com/ScriptReference/Light-intensity.html")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("cookie"),
                new DescriptorAttribute("Cookie", "The cookie texture projected by the light.", "http://docs.unity3d.com/ScriptReference/Light-cookie.html")));
            fields.Add(new InspectorField(type, Instances, type.GetProperty("cookieSize"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsDirectional)),
                new DescriptorAttribute("Cookie Size", "The size of a directional light's cookie.", "http://docs.unity3d.com/ScriptReference/Light-cookieSize.html")));

            //Acts like a group
            fields.Add(new InspectorField(type, Instances, type.GetProperty("shadows"), new HelpAttribute(new HelpAttribute.HelpDelegate(HelpShadowDeferred)),
                new HelpAttribute(new HelpAttribute.HelpDelegate(HelpShadowPro)), new DescriptorAttribute("Shadow Type", "How this light casts shadows", "http://docs.unity3d.com/ScriptReference/Light-shadows.html")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("shadowStrength"), new InspectAttribute(new InspectAttribute.InspectDelegate(HasShadow)),
                new DescriptorAttribute("Strength", "How this light casts shadows.", "http://docs.unity3d.com/ScriptReference/Light-shadowStrength.html")));
            fields.Add(new InspectorField(type, Instances, so.FindProperty("m_Shadows.m_Resolution"), new InspectAttribute(new InspectAttribute.InspectDelegate(HasShadow)),
                new DescriptorAttribute("Resolution", "The shadow's resolution.")));
            fields.Add(new InspectorField(type, Instances, type.GetProperty("shadowBias"), new InspectAttribute(new InspectAttribute.InspectDelegate(HasShadow)),
                new DescriptorAttribute("Bias", "Shadow mapping bias.", "http://docs.unity3d.com/ScriptReference/Light-shadowBias.html")));
            fields.Add(new InspectorField(type, Instances, type.GetProperty("shadowSoftness"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsSoft)),
                new DescriptorAttribute("Softness", "Softness of directional light's soft shadows.", "http://docs.unity3d.com/ScriptReference/Light-shadowSoftness.html")));
            fields.Add(new InspectorField(type, Instances, type.GetProperty("shadowSoftnessFade"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsSoft)),
                new DescriptorAttribute("Softness Fade", "Fadeout speed of directional light's soft shadows.", "http://docs.unity3d.com/ScriptReference/Light-shadowSoftnessFade.html")));

            fields.Add(new InspectorField(type, Instances, so.FindProperty("m_DrawHalo"), 
                new DescriptorAttribute("Draw Halo", "Draw a halo around the light. Now work with the Halo class.")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("flare"),
                new DescriptorAttribute("Flare", "The flare asset to use for this light.", "http://docs.unity3d.com/ScriptReference/Light-flare.html")));

            fields.Add(new InspectorField(type, Instances, so.FindProperty("m_RenderMode"), 
                new DescriptorAttribute("Render Mode", "The rendering path for the lights.")));

            fields.Add(new InspectorField(type, Instances, so.FindProperty("m_CullingMask"), 
                new DescriptorAttribute("Culling Mask", "The object that are affected or ignored by the light.")));

            fields.Add(new InspectorField(type, Instances, so.FindProperty("m_Lightmapping"), 
                new DescriptorAttribute("Lightmapping", "How is the lightmapping handled.")));

            fields.Add(new InspectorField(type, Instances, type.GetProperty("areaSize"), new InspectAttribute(new InspectAttribute.InspectDelegate(IsArea)),
                new DescriptorAttribute("Lightmapping", "The size of the area light. Editor only.", "http://docs.unity3d.com/ScriptReference/Light-areaSize.html")));
        }

        public bool IsPointOrSpot()
        {
            if (IsPoint() || IsSpot())
                return true;

            return false;
        }

        public bool IsPoint()
        {
            return ((Light)target).type == LightType.Point;
        }

        public bool IsSpot()
        {
            return ((Light)target).type == LightType.Spot;
        }

        public bool IsDirectional()
        {
            return ((Light)target).type == LightType.Directional;
        }

        public bool IsArea()
        {
            return ((Light)target).type == LightType.Area;
        }

        public bool HasShadow()
        {
            Light light = (Light)target;
            return light.shadows == LightShadows.Hard || light.shadows == LightShadows.Soft;
        }

        public bool IsSoft()
        {
            return ((Light)target).shadows == LightShadows.Soft;
        }

        public bool DoesAnyCameraUseDeferred()
        {
            Camera[] allCameras = Camera.allCameras;
            for (int i = 0; i < allCameras.Length; i++)
                if (allCameras[i].actualRenderingPath == RenderingPath.DeferredLighting)
                    return true;

            return false;
        }

        public HelpAttribute HelpShadowDeferred()
        {
            if (HasShadow() && !IsDirectional() && !DoesAnyCameraUseDeferred())
                return new HelpAttribute(HelpType.Warning, "Only directional lights have shadow in foward rendering.");

            return null;
        }

        public HelpAttribute HelpShadowPro()
        {
            if (HasShadow() && IsPointOrSpot() && !UnityEditorInternal.InternalEditorUtility.HasPro())
                return new HelpAttribute(HelpType.Warning, "Real time shadow for point and spot lights requires Unity Pro.");

            return null;
        }

        public HelpAttribute HelpLightType()
        {
            if (IsArea() && !UnityEditorInternal.InternalEditorUtility.HasPro())
                return new HelpAttribute(HelpType.Warning, "Area lights require Unity Pro.");

            return null;
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (Event.current.type == EventType.Used)
                return;

            Light light = (Light)target;
            Color color = Handles.color;

            if (light.enabled)
                Handles.color = lightColor;
            else
                Handles.color = disabledLightColor;

            float range = light.range;
            switch (light.type)
            {
                case LightType.Spot:
                {
                    Color color2 = Handles.color;
                    color2.a = Mathf.Clamp01(color.a * 2f);
                    Handles.color = color2;
                    Vector2 angleAndRange = new Vector2(light.spotAngle, light.range);
                    angleAndRange = ConeHandle(light.transform.rotation, light.transform.position, angleAndRange, 1f, 1f, true);
                    if (GUI.changed)
                    {
                        Undo.RecordObject(light, "Adjust Spot Light");
                        light.spotAngle = angleAndRange.x;
                        light.range = Mathf.Max(angleAndRange.y, 0.01f);
                    }

                    break;
                }

                case LightType.Point:
                {
                    range = Handles.RadiusHandle(Quaternion.identity, light.transform.position, range, true);
                    if (GUI.changed)
                    {
                        Undo.RecordObject(light, "Adjust Point Light");
                        light.range = range;
                    }

                    break;
                }

                case LightType.Area:
                {
                    EditorGUI.BeginChangeCheck();
                    Vector2 vector2 = RectHandles(light.transform.rotation, light.transform.position, light.areaSize);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(light, "Adjust Area Light");
                        light.areaSize = vector2;
                    }

                    break;
                }
            }
            Handles.color = color;
        }

        private Vector2 ConeHandle(Quaternion rotation, Vector3 position, Vector2 angleAndRange, float angleScale, float rangeScale, bool handlesOnly)
        {
            float x = angleAndRange.x;
            float y = angleAndRange.y;
            float r = y * rangeScale;
            
            Vector3 forward = rotation * Vector3.forward;
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;

            bool changed = GUI.changed;
            GUI.changed = false;
            r = SizeSlider(position, forward, r);
            if (GUI.changed)
                y = Mathf.Max(0f, r / rangeScale);

            GUI.changed |= changed;
            changed = GUI.changed;
            GUI.changed = false;

            float angle = (r * Mathf.Tan((0.01745329f * x) / 2f)) * angleScale;
            angle = SizeSlider(position + (forward * r), up, angle);
            angle = SizeSlider(position + (forward * r), -up, angle);
            angle = SizeSlider(position + (forward * r), right, angle);
            angle = SizeSlider(position + (forward * r), -right, angle);

            if (GUI.changed)
                x = Mathf.Clamp((57.29578f * Mathf.Atan(angle / (r * angleScale))) * 2f, 0f, 179f);

            GUI.changed |= changed;
            if (!handlesOnly)
            {
                Handles.DrawLine(position, (Vector3)((position + (forward * r)) + (up * angle)));
                Handles.DrawLine(position, (Vector3)((position + (forward * r)) - (up * angle)));
                Handles.DrawLine(position, (Vector3)((position + (forward * r)) + (right * angle)));
                Handles.DrawLine(position, (Vector3)((position + (forward * r)) - (right * angle)));
                Handles.DrawWireDisc(position + ((Vector3)(r * forward)), forward, angle);
            }

            return new Vector2(x, y);
        }

        private Vector2 RectHandles(Quaternion rotation, Vector3 position, Vector2 size)
        {
            Vector3 forward = (Vector3)(rotation * Vector3.forward);
            Vector3 up = (Vector3)(rotation * Vector3.up);
            Vector3 right = (Vector3)(rotation * Vector3.right);

            float radiusX = 0.5f * size.x;
            float radiusY = 0.5f * size.y;

            Vector3 v1 = (position + (up * radiusY)) + (right * radiusX);
            Vector3 v2 = (position - (up * radiusY)) + (right * radiusX);
            Vector3 v3 = (position - (up * radiusY)) - (right * radiusX);
            Vector3 v4 = (position + (up * radiusY)) - (right * radiusX);

            Handles.DrawLine(v1, v2);
            Handles.DrawLine(v2, v3);
            Handles.DrawLine(v3, v4);
            Handles.DrawLine(v4, v1);

            Color color = Handles.color;
            color.a = Mathf.Clamp01(color.a * 2f);
            Handles.color = color;

            radiusY = SizeSlider(position, up, radiusY);
            radiusY = SizeSlider(position, -up, radiusY);
            radiusX = SizeSlider(position, right, radiusX);
            radiusX = SizeSlider(position, -right, radiusX);

            if (((Tools.current != Tool.Move) && (Tools.current != Tool.Scale)) || Tools.pivotRotation != PivotRotation.Local)
                Handles.DrawLine(position, position + forward);

            size.x = 2f * radiusX;
            size.y = 2f * radiusY;

            return size;
        }

        private float SizeSlider(Vector3 p, Vector3 direction, float radius)
        {
            Vector3 position = p + (direction * radius);
            float handleSize = HandleUtility.GetHandleSize(position);

            bool changed = GUI.changed;
            GUI.changed = false;

            position = Handles.Slider(position, direction, handleSize * 0.03f, new Handles.DrawCapFunction(Handles.DotCap), 0f);

            if (GUI.changed)
                radius = Vector3.Dot(position - p, direction);

            GUI.changed |= changed;
            return radius;
        }
    }
}
